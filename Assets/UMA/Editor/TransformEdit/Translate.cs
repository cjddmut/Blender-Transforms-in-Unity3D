using UnityEngine;
using UnityEditor;
using System.Collections;

public class Translate : ModalEdit
{
    private Vector2 _OriginalMousePos;
    private Vector3[] _ToObjects;
    private Vector3 _OriginalAvgPos;
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

        // Just in case the order isn't guaranteed, I'm going to save the selecteds.
        _Selected = Selection.GetTransforms(SelectionMode.TopLevel);
        _ToObjects = new Vector3[_Selected.Length];
        _OriginalAvgPos = Vector3.zero;
        _State.Init();

        for (int i = 0; i < _Selected.Length; i++)
        {
            _OriginalAvgPos += _Selected[i].position;
        }

        _OriginalAvgPos /= _Selected.Length;

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

        CalculatePosition(_OriginalMousePos, Event.current.mousePosition);
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
        for (int i = 0; i < _Selected.Length; i++)
        {
            _Selected[i].position = from + _ToObjects[i];
        }
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

    private void CalculatePosition(Vector2 mousePos1, Vector2 mousePos2)
    {
        Vector3 moveTo = Vector3.zero;
        Camera sceneCam = SceneView.lastActiveSceneView.camera;

        Vector3 toNewPos = mousePos2 - mousePos1;
        toNewPos.y *= -1;

        // There is some slight movement even if the mouse hasn't moved (floating point?). So don't go further.
        if (toNewPos.sqrMagnitude == 0)
        {
            return;
        }

        Vector3 inSP = sceneCam.WorldToScreenPoint(_OriginalAvgPos);
        inSP += toNewPos;

        Vector3 objNewPos = sceneCam.ScreenToWorldPoint(inSP);

        if (_State.MySpace == Space.World)
        {
            if (_State.MyMode == TransformState.Mode.Free)
            {
                moveTo = objNewPos;
            }
            else if (_State.MyMode == TransformState.Mode.SingleAxis)
            {
                moveTo = _OriginalAvgPos + GetGlobalProjectedAxisMotion(objNewPos, _State.MyAxis);
            }
            else if (_State.MyMode == TransformState.Mode.DoubleAxis)
            {
                if (_State.MyAxis == TransformState.Axis.X)
                {
                    moveTo = _OriginalAvgPos + 
                        GetGlobalProjectedAxisMotion(objNewPos, TransformState.Axis.Y) + 
                        GetGlobalProjectedAxisMotion(objNewPos, TransformState.Axis.Z);
                }
                else if (_State.MyAxis == TransformState.Axis.Y)
                {
                    moveTo = _OriginalAvgPos + 
                        GetGlobalProjectedAxisMotion(objNewPos, TransformState.Axis.X) + 
                        GetGlobalProjectedAxisMotion(objNewPos, TransformState.Axis.Z);
                }
                else
                {
                    moveTo = _OriginalAvgPos + 
                        GetGlobalProjectedAxisMotion(objNewPos, TransformState.Axis.X) + 
                        GetGlobalProjectedAxisMotion(objNewPos, TransformState.Axis.Y);
                }
            }


            moveTo = CheckThreshold(moveTo);

            if (_State.IsSnapping)
            {
                moveTo = HandleSnapping(moveTo);
            }

            UpdatePositions(moveTo);
        }
        else
        {
            for (int i = 0; i < _Selected.Length; i++ )
            {
                if (_State.MyMode == TransformState.Mode.SingleAxis)
                {
                    moveTo = _OriginalAvgPos + _ToObjects[i] + GetLocalProjectedAxisMotion(_Selected[i], objNewPos, _State.MyAxis);
                }
                else
                {
                    if (_State.MyAxis == TransformState.Axis.X)
                    {
                        moveTo = _OriginalAvgPos + _ToObjects[i] +
                            GetLocalProjectedAxisMotion(_Selected[i], objNewPos, TransformState.Axis.Y) +
                            GetLocalProjectedAxisMotion(_Selected[i], objNewPos, TransformState.Axis.Z);
                    }
                    else if (_State.MyAxis == TransformState.Axis.Y)
                    {
                        moveTo = _OriginalAvgPos + _ToObjects[i] +
                            GetLocalProjectedAxisMotion(_Selected[i], objNewPos, TransformState.Axis.X) +
                            GetLocalProjectedAxisMotion(_Selected[i], objNewPos, TransformState.Axis.Z);
                    }
                    else
                    {
                        moveTo = _OriginalAvgPos + _ToObjects[i] +
                            GetLocalProjectedAxisMotion(_Selected[i], objNewPos, TransformState.Axis.X) +
                            GetLocalProjectedAxisMotion(_Selected[i], objNewPos, TransformState.Axis.Y);
                    }
                }

                moveTo = CheckThreshold(moveTo);

                if (_State.IsSnapping)
                {
                    moveTo = HandleSnapping(moveTo);
                }

                _Selected[i].position = moveTo;
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

    private Vector3 GetGlobalProjectedAxisMotion(Vector3 newPos, TransformState.Axis a)
    {
        Vector3 toNew = newPos - _OriginalAvgPos;
        Vector3 allowed = Vector3.zero;

        if (a == TransformState.Axis.X)
        {
            allowed.x = 1;
        }
        else if (a == TransformState.Axis.Y)
        {
            allowed.y = 1;
        }
        else
        {
            allowed.z = 1;
        }

        // TODO: What I'd prefer to do is project on the allowed axis in screen space and convert that, but
        //       I'm having trouble getting it to work. This works ok for now but will approach the issue later.
        return Vector3.Project(toNew, allowed);
    }

    private Vector3 GetLocalProjectedAxisMotion(Transform obj, Vector3 newPos, TransformState.Axis a)
    {
        Vector3 toNew = newPos - _OriginalAvgPos;
        Vector3 allowed = Vector3.zero;

        if (a == TransformState.Axis.X)
        {
            allowed = obj.TransformDirection(Vector3.right);
        }
        else if (a == TransformState.Axis.Y)
        {
            allowed = obj.TransformDirection(Vector3.up);
        }
        else
        {
            allowed = obj.TransformDirection(Vector3.forward);
        }

        // TODO: What I'd prefer to do is project on the allowed axis in screen space and convert that, but
        //       I'm having trouble getting it to work. This works ok for now but will approach the issue later.
        return Vector3.Project(toNew, allowed);
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
