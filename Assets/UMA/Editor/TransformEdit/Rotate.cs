using UnityEngine;
using UnityEditor;
using System.Collections;

public class Rotate : ModalEdit
{
    private Vector2 _OriginalMousePos;
    private Vector3 _AvgPos;
    private Vector3[] _OriginalRotations;
    private Vector3[] _OriginalPositions;
    private Transform[] _Selected;
    private TransformState _State;

    private const float ROTATION_THRESHOLD = 0.02f;
    private const float SNAP_AMOUNT = 45f;

    public Rotate()
    {
        _State = new TransformState();
    }

    public override void Start()
    {
        _OriginalMousePos = Event.current.mousePosition;

        // Just in case the order isn't guaranteed, I'm going to save the selecteds.
        _Selected = Selection.GetTransforms(SelectionMode.TopLevel);
        _AvgPos = Vector3.zero;
        _OriginalRotations = new Vector3[_Selected.Length];
        _OriginalPositions = new Vector3[_Selected.Length];
        _State.Init();

        for (int i = 0; i < _Selected.Length; i++)
        {
            _AvgPos += _Selected[i].position;
            _OriginalRotations[i] = _Selected[i].eulerAngles;
            _OriginalPositions[i] = _Selected[i].position;
        }

        _AvgPos /= _Selected.Length;

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
        ResetRotations();
        Undo.RecordObjects(_Selected, "Rotate");

        _State.DrawLines(_AvgPos, _Selected);
        CalculateRotation(_OriginalMousePos, Event.current.mousePosition);
    }

    public override void Confirm()
    {
        // Done son!
    }

    public override void Cancel()
    {
        ResetRotations();
    }

    private void ResetRotations()
    {
        for (int i = 0; i < _Selected.Length; i++)
        {
            _Selected[i].eulerAngles = _OriginalRotations[i];
            _Selected[i].position = _OriginalPositions[i];
        }
    }

    private void UpdateRotations(Vector3 axis, float angle)
    {
        foreach (Transform t in _Selected)
        {
            t.RotateAround(_AvgPos, axis, angle);
        }
    }

    private bool HandleEvent()
    {
        // Cancel or confirm?
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 ||
            Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return ||
                 Event.current.keyCode == KeyCode.KeypadEnter ||
                 Event.current.keyCode == KeyCode.R))
        {
            // Confirm is left click, 'r' or Enter.
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

    private void CalculateRotation(Vector2 mousePos1, Vector2 mousePos2)
    {
        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        Vector2 toNewPos = mousePos2 - mousePos1;

        // There is some slight movement even if the mouse hasn't moved (floating point?). So don't go further.
        if (toNewPos.sqrMagnitude == 0)
        {
            return;
        }

        Vector2 inSP = sceneCam.WorldToScreenPoint(_AvgPos);
        inSP.y = sceneCam.pixelHeight - inSP.y;

        float angle = Vector2.Angle(mousePos1 - inSP, mousePos2 - inSP);
        Vector3 axis = Vector3.zero;

        if (Vector3.Cross(mousePos1 - inSP, mousePos2 - inSP).z < 0)
        {
            angle = 360 - angle;
        }

        if (_State.MySpace == Space.World)
        {
            if (_State.MyMode == TransformState.Mode.Free)
            {
                // The best way I could figure out how to get a direction to the camera plane was to cast
                // a ray from the camera and use the -direction. I thought using an InverseTransformDir on
                // Vector3.back would have worked but uh nope.
                axis = -sceneCam.ScreenPointToRay(inSP).direction;
            }
            else
            {
                Vector3 toCam = sceneCam.transform.position - _AvgPos;

                // For rotations, pressing 'z' or 'shift+z' result in the same behavior. I'll support both
                // just for consistency but I could just nix all shift commands.
                if (_State.MyAxis == TransformState.Axis.X)
                {
                    if (toCam.x >= 0)
                    {
                        axis = Vector3.right;
                    }
                    else
                    {
                        axis = Vector3.left;
                    }
                }
                else if (_State.MyAxis == TransformState.Axis.Y)
                {
                    if (toCam.y >= 0)
                    {
                        axis = Vector3.up;
                    }
                    else
                    {
                        axis = Vector3.down;
                    }
                }
                else
                {
                    if (toCam.z >= 0)
                    {
                        axis = Vector3.forward;
                    }
                    else
                    {
                        axis = Vector3.back;
                    }
                }
            }

            UpdateRotations(axis, angle);
        }
        else
        {
            // As above, 'x' and 'shift+x' is the same.
            foreach(Transform t in _Selected)
            {
                Vector3 caminObjSP = t.transform.InverseTransformPoint(sceneCam.transform.position);

                if (_State.MyAxis == TransformState.Axis.X)
                {
                    if (caminObjSP.x >= 0)
                    {
                        axis = t.transform.TransformDirection(Vector3.right);
                    }
                    else
                    {
                        axis = t.transform.TransformDirection(Vector3.left);
                    }
                }
                else if (_State.MyAxis == TransformState.Axis.Y)
                {
                    if (caminObjSP.y >= 0)
                    {
                        axis = t.transform.TransformDirection(Vector3.up);
                    }
                    else
                    {
                        axis = t.transform.TransformDirection(Vector3.down);
                    }
                }
                else
                {
                    if (caminObjSP.x >= 0)
                    {
                        axis = t.transform.TransformDirection(Vector3.forward);
                    }
                    else
                    {
                        axis = t.transform.TransformDirection(Vector3.back);
                    }
                }

                t.RotateAround(t.position, axis, angle);
            }

        }

        CheckThreshold();
    }

    private void CheckThreshold()
    {
        for (int i = 0; i < _Selected.Length; i++)
        {
            // Clean up work. Who understands Quaternions?! Not me!

            // TODO: Err, this "clean up" doesn't seem to impact the inspector values which was kinda the point.
            Vector3 eulerAngles = _Selected[i].eulerAngles;

            eulerAngles.x %= 360;

            if (Mathf.Abs(_OriginalRotations[i].x - eulerAngles.x) < ROTATION_THRESHOLD)
            {
                eulerAngles.x = _OriginalRotations[i].x;
            }

            eulerAngles.y %= 360;

            if (Mathf.Abs(_OriginalRotations[i].y - eulerAngles.y) < ROTATION_THRESHOLD)
            {
                eulerAngles.y = _OriginalRotations[i].y;
            }

            eulerAngles.z %= 360;

            if (Mathf.Abs(_OriginalRotations[i].z - eulerAngles.z) < ROTATION_THRESHOLD)
            {
                eulerAngles.z = _OriginalRotations[i].z;
            }

            _Selected[i].eulerAngles = eulerAngles;
        }
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
