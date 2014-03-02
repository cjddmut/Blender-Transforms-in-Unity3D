using UnityEngine;
using UnityEditor;
using System.Collections;

public class Translate : ModalEdit
{
    private Vector2 _OriginalMousePos;
    private Vector3[] _ToObjects;
    private Vector3 _OriginalAvgPos;
    private Vector3 _LastKnownGoodPos;
    private Vector3[] _LastKnownGoodLocalPos;
    private Transform[] _Selected;
    private TransformState _State;

    private const float MOVEMENT_THRESHOLD = 0.01f;
    private const float SNAP_AMOUNT = 1f;

    public Translate()
    {
        _State = new TransformState();
    }

    public override void Start()
    {
        _OriginalMousePos = Event.current.mousePosition;
        _OriginalMousePos.y = SceneView.lastActiveSceneView.camera.pixelHeight - _OriginalMousePos.y;

        // Just in case the order isn't guaranteed, I'm going to save the selecteds.
        _Selected = Selection.GetTransforms(SelectionMode.TopLevel);
        _ToObjects = new Vector3[_Selected.Length];
        _LastKnownGoodLocalPos = new Vector3[_Selected.Length];
        _OriginalAvgPos = Vector3.zero;
        _State.Init();

        for (int i = 0; i < _Selected.Length; i++)
        {
            _OriginalAvgPos += _Selected[i].position;
            _LastKnownGoodPos = _Selected[i].position;
        }

        _OriginalAvgPos /= _Selected.Length;
        _LastKnownGoodPos = _OriginalAvgPos;

        // Now that we have the average position, we get a bunch of vectors to each objects.
        for (int i = 0; i < _Selected.Length; i++)
        {
            _ToObjects[i] = _Selected[i].position - _OriginalAvgPos;
        }

        Undo.IncrementCurrentGroup();
    }

    public override void Update()
    {
        if (HandleEvent())
        {
            // This means we're done, let the handler know.
            TransformManager.ModalFinished();
            return;
        }

        // We reset everything to push to the UNDO stack.
        UpdatePositions(_OriginalAvgPos);
        Undo.RecordObjects(_Selected, "Translate");

        _State.DrawLines(_OriginalAvgPos, _Selected);

        CalculatePosition();
    }

    public override void Confirm()
    {
        // Done son!
    }

    public override void Cancel()
    {
        UpdatePositions(_OriginalAvgPos);
    }

    private void UpdatePositions(Vector3 from)
    {
        from = CheckThreshold(from);

        if (_State.IsSnapping)
        {
            from = HandleSnapping(from);
        }

        _LastKnownGoodPos = from;

        for (int i = 0; i < _Selected.Length; i++)
        {
            _Selected[i].position = from + _ToObjects[i];
        }
    }

    private void UpdateSinglePosition(Vector3 moveTo, int index)
    {
        moveTo = CheckThreshold(moveTo);

        if (_State.IsSnapping)
        {
            moveTo = HandleSnapping(moveTo);
        }

        _LastKnownGoodLocalPos[index] = moveTo;
        _Selected[index].position = moveTo;
    }

    private bool HandleEvent()
    {
        // Cancel or confirm?
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 ||
            Event.current.type == EventType.KeyDown && 
                (Event.current.keyCode == KeyCode.Return || 
                 Event.current.keyCode == KeyCode.KeypadEnter || 
                 Event.current.keyCode == KeyCode.G))
        {
            // Confirm is left click, 'g' or Enter.
            Confirm();
            Event.current.Use();
            return true;
        }
        else if (Event.current.type == EventType.MouseDown && Event.current.button == 1 ||
                 Event.current.type == EventType.KeyDown && 
                 (Event.current.keyCode == KeyCode.Escape || 
                  Event.current.keyCode == KeyCode.Space))
        {
            // Right click is cancel, space or ESC.
            Cancel();
            Event.current.Use();
            return true;
        }

        _State.HandleEvent();

        return false;
    }

    private void CalculatePosition()
    {
        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        
        // Raw mouse coordinates have y flipped from what unity uses.
        Vector2 mousePos = Event.current.mousePosition;
        mousePos.y = sceneCam.pixelHeight - mousePos.y;

        Vector2 toNewMouse = mousePos - _OriginalMousePos;

        Vector2 objInSP = sceneCam.WorldToScreenPoint(_OriginalAvgPos);
        Vector2 newObjPosInSP = objInSP + toNewMouse;

        // There is some slight movement even if the mouse hasn't moved (floating point?). So don't go further.
        if (toNewMouse.sqrMagnitude == 0)
        {
            return;
        }

        Vector3 moveTo = Vector3.zero;
        Plane movePlane = new Plane();
        Vector3 camViewPlaneNormal = UMAUtil.GetCamViewPlaneNormal(sceneCam.camera);
        Ray rayToNewPos = sceneCam.ScreenPointToRay(newObjPosInSP);

        if (_State.MySpace == Space.World)
        {
            if (_State.MyMode == TransformState.Mode.Free)
            {
                movePlane = new Plane(camViewPlaneNormal, _OriginalAvgPos);
                moveTo = CastRayAndGetPosition(movePlane, rayToNewPos);
            }
            else if (_State.MyMode == TransformState.Mode.SingleAxis)
            {
                Vector3 planeNormal = camViewPlaneNormal;
                moveTo = _OriginalAvgPos;

                if (_State.MyAxis == TransformState.Axis.X)
                {
                    planeNormal.x = 0;
                }
                else if (_State.MyAxis == TransformState.Axis.Y)
                {
                    planeNormal.y = 0;
                }
                else
                {
                    planeNormal.z = 0;
                }

                moveTo = GetGlobalProjectedAxisMotion(planeNormal, rayToNewPos);
            }
            else
            {
                if (_State.MyAxis == TransformState.Axis.X)
                {
                    movePlane = new Plane(Vector3.Cross(Vector3.up, Vector3.forward), _OriginalAvgPos);
                }
                else if (_State.MyAxis == TransformState.Axis.Y)
                {
                    movePlane = new Plane(Vector3.Cross(Vector3.right, Vector3.forward), _OriginalAvgPos);
                }
                else
                {
                    movePlane = new Plane(Vector3.Cross(Vector3.right, Vector3.up), _OriginalAvgPos);
                }

                moveTo = CastRayAndGetPosition(movePlane, rayToNewPos);
            }

            UpdatePositions(moveTo);
        }
        else
        {
            for (int i = 0; i < _Selected.Length; i++)
            {
                // Local space!
                if (_State.MyMode == TransformState.Mode.SingleAxis)
                {
                    // Working in local space for simplicity.
                    Vector3 camViewPlaneNormalLocal = _Selected[i].InverseTransformDirection(camViewPlaneNormal);

                    Ray rayToNewPosLocal = new Ray();
                    rayToNewPosLocal.origin = _Selected[i].InverseTransformPoint(rayToNewPos.origin);
                    rayToNewPosLocal.direction = _Selected[i].InverseTransformDirection(rayToNewPos.direction);

                    if (_State.MyAxis == TransformState.Axis.X)
                    {
                        camViewPlaneNormalLocal.x = 0;
                    }
                    else if (_State.MyAxis == TransformState.Axis.Y)
                    {
                        camViewPlaneNormalLocal.y = 0;
                    }
                    else
                    {
                        camViewPlaneNormalLocal.z = 0;
                    }

                    moveTo = GetLocalProjectedAxisMotion(camViewPlaneNormalLocal, rayToNewPosLocal, i);

                    // Convert back to global space.
                    moveTo = _Selected[i].TransformPoint(moveTo);
                }
                else
                {
                    Vector3 vec1 = Vector3.zero;
                    Vector3 vec2 = Vector3.zero; ;

                    if (_State.MyAxis == TransformState.Axis.X)
                    {
                        vec1 = _Selected[i].TransformDirection(Vector3.up);
                        vec2 = _Selected[i].TransformDirection(Vector3.forward);
                    }
                    else if (_State.MyAxis == TransformState.Axis.Y)
                    {
                        vec1 = _Selected[i].TransformDirection(Vector3.right);
                        vec2 = _Selected[i].TransformDirection(Vector3.forward);
                    }
                    else
                    {
                        vec1 = _Selected[i].TransformDirection(Vector3.right);
                        vec2 = _Selected[i].TransformDirection(Vector3.up);
                    }

                    movePlane = new Plane(Vector3.Cross(vec1, vec2), _Selected[i].position);
                    moveTo = CastRayAndGetPositionLocal(movePlane, rayToNewPos, i);
                }

                UpdateSinglePosition(moveTo, i);
            }
        }
    }

    private Vector3 CheckThreshold(Vector3 v)
    {
        // Floating point thresholds!
        if (Mathf.Abs(v.x - _OriginalAvgPos.x) < MOVEMENT_THRESHOLD)
        {
            v.x = _OriginalAvgPos.x;
        }

        if (Mathf.Abs(v.y - _OriginalAvgPos.y) < MOVEMENT_THRESHOLD)
        {
            v.y = _OriginalAvgPos.y;
        }

        if (Mathf.Abs(v.z - _OriginalAvgPos.z) < MOVEMENT_THRESHOLD)
        {
            v.z = _OriginalAvgPos.z;
        }

        return v;
    }

    private Vector3 CastRayAndGetPosition(Plane plane, Ray ray)
    {
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            return ray.origin + (ray.direction * distance);
        }
        else
        {
            return _LastKnownGoodPos;
        }
    }

    private Vector3 CastRayAndGetPositionLocal(Plane plane, Ray ray, int index)
    {
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            return ray.origin + (ray.direction * distance);
        }
        else
        {
            return _LastKnownGoodLocalPos[index];
        }
    }

    private Vector3 GetGlobalProjectedAxisMotion(Vector3 planeNormal, Ray mouseRay)
    {
        Plane movePlane = new Plane(planeNormal, _OriginalAvgPos);
        float distance;
        Vector3 contactPoint;
        Vector3 moveTo = _OriginalAvgPos;

        if (movePlane.Raycast(mouseRay, out distance))
        {
            contactPoint = mouseRay.origin + (mouseRay.direction * distance);

            if (_State.MyAxis == TransformState.Axis.X)
            {
                moveTo.x = contactPoint.x;
            }
            else if (_State.MyAxis == TransformState.Axis.Y)
            {
                moveTo.y = contactPoint.y;
            }
            else
            {
                moveTo.z = contactPoint.z;
            }
        }
        else
        {
            moveTo = _LastKnownGoodPos;
        }

        return moveTo;
    }

    private Vector3 GetLocalProjectedAxisMotion(Vector3 planeNormal, Ray mouseRay, int index)
    {
        Plane movePlane = new Plane(planeNormal, Vector3.zero);
        float distance;
        Vector3 contactPoint;
        Vector3 moveTo = Vector3.zero;

        if (movePlane.Raycast(mouseRay, out distance))
        {
            contactPoint = mouseRay.origin + (mouseRay.direction * distance);

            if (_State.MyAxis == TransformState.Axis.X)
            {
                moveTo.x = contactPoint.x;
            }
            else if (_State.MyAxis == TransformState.Axis.Y)
            {
                moveTo.y = contactPoint.y;
            }
            else
            {
                moveTo.z = contactPoint.z;
            }
        }
        else
        {
            // This is slightly unnecessary as I will just convert it back to world space.
            moveTo = _Selected[index].InverseTransformPoint(_LastKnownGoodLocalPos[index]);
        }

        return moveTo;
    }

    private Vector3 HandleSnapping(Vector3 vecToSnap)
    {
        vecToSnap /= SNAP_AMOUNT;

        vecToSnap.x = Mathf.Round(vecToSnap.x);
        vecToSnap.y = Mathf.Round(vecToSnap.y);
        vecToSnap.z = Mathf.Round(vecToSnap.z);

        vecToSnap *= SNAP_AMOUNT;

        return vecToSnap;
    }
}
