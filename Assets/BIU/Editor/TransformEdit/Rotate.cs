using UnityEngine;
using UnityEditor;
using System.Collections;

namespace BIU
{
    public class Rotate : ModalEdit
    {
        private Vector2 originalMousePos;
        private Vector3 avgPos;
        private Vector3[] originalRotations;
        private Vector3[] originalPositions;
        private Transform[] selected;
        private TransformState state;

        private const float ROTATION_THRESHOLD = 0.02f;

        public Rotate()
        {
            state = new TransformState();
            triggerKey = Data.rotateKey;
        }

        public override void Start()
        {
            base.Start();

            originalMousePos = Event.current.mousePosition;

            // Just in case the order isn't guaranteed, I'm going to save the selecteds.
            selected = Selection.GetTransforms(SelectionMode.TopLevel);
            avgPos = Vector3.zero;
            originalRotations = new Vector3[selected.Length];
            originalPositions = new Vector3[selected.Length];
            state.Init();

            for (int i = 0; i < selected.Length; i++)
            {
                avgPos += selected[i].position;
                originalRotations[i] = selected[i].eulerAngles;
                originalPositions[i] = selected[i].position;
            }

            avgPos /= selected.Length;

            Undo.IncrementCurrentGroup();
        }

        public override void Update()
        {
            base.Update();

            if (!isInMode)
            {
                return;
            }

            state.HandleEvent();
            
            // We reset everything to push to the UNDO stack.
            ResetRotations();
            Undo.RecordObjects(selected, "Rotate");

            state.DrawLines(avgPos, selected);
            CalculateRotation(originalMousePos, Event.current.mousePosition);
        }

        public override void Confirm()
        {
            // Done son!
            base.Confirm();
        }

        public override void Cancel()
        {
            base.Cancel();
            ResetRotations();
        }

        private void ResetRotations()
        {
            for (int i = 0; i < selected.Length; i++)
            {
                selected[i].eulerAngles = originalRotations[i];
                selected[i].position = originalPositions[i];
            }
        }

        private void UpdateRotations(Vector3 axis, float angle)
        {
            foreach (Transform t in selected)
            {
                t.RotateAround(avgPos, axis, angle);
            }
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

            Vector2 inSP = sceneCam.WorldToScreenPoint(avgPos);
            inSP.y = sceneCam.pixelHeight - inSP.y;

            float angle = Vector2.Angle(mousePos1 - inSP, mousePos2 - inSP);
            Vector3 axis = Vector3.zero;

            if (Vector3.Cross(mousePos1 - inSP, mousePos2 - inSP).z < 0)
            {
                angle = 360 - angle;
            }

            if (state.mySpace == Space.World)
            {
                if (state.myMode == TransformState.Mode.Free)
                {
                    // The best way I could figure out how to get a direction to the camera plane was to cast
                    // a ray from the camera and use the -direction. I thought using an InverseTransformDir on
                    // Vector3.back would have worked but uh nope.
                    axis = -sceneCam.ScreenPointToRay(inSP).direction;
                }
                else
                {
                    Vector3 toCam = sceneCam.transform.position - avgPos;

                    // For rotations, pressing 'z' or 'shift+z' result in the same behavior. I'll support both
                    // just for consistency but I could just nix all shift commands.
                    if (state.myAxis == TransformState.Axis.X)
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
                    else if (state.myAxis == TransformState.Axis.Y)
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
                foreach (Transform t in selected)
                {
                    Vector3 caminObjSP = t.transform.InverseTransformPoint(sceneCam.transform.position);

                    if (state.myAxis == TransformState.Axis.X)
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
                    else if (state.myAxis == TransformState.Axis.Y)
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
                        if (caminObjSP.z >= 0)
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
            HandleSnapping();
        }

        private void CheckThreshold()
        {
            for (int i = 0; i < selected.Length; i++)
            {
                // Clean up work. Who understands Quaternions?! Not me!

                // TODO: Err, this "clean up" doesn't seem to impact the inspector values which was kinda the point.
                Vector3 eulerAngles = selected[i].eulerAngles;

                eulerAngles.x %= 360;

                if (Mathf.Abs(originalRotations[i].x - eulerAngles.x) < ROTATION_THRESHOLD)
                {
                    eulerAngles.x = originalRotations[i].x;
                }

                eulerAngles.y %= 360;

                if (Mathf.Abs(originalRotations[i].y - eulerAngles.y) < ROTATION_THRESHOLD)
                {
                    eulerAngles.y = originalRotations[i].y;
                }

                eulerAngles.z %= 360;

                if (Mathf.Abs(originalRotations[i].z - eulerAngles.z) < ROTATION_THRESHOLD)
                {
                    eulerAngles.z = originalRotations[i].z;
                }

                selected[i].eulerAngles = eulerAngles;
            }
        }

        private void HandleSnapping()
        {
            if (!state.isSnapping || Data.rotateSnapIncrement == 0)
            {
                return;
            }

            foreach (Transform t in selected)
            {
                Vector3 vecToSnap = t.eulerAngles;

                vecToSnap /= Data.rotateSnapIncrement;

                vecToSnap.x = Mathf.Round(vecToSnap.x);
                vecToSnap.y = Mathf.Round(vecToSnap.y);
                vecToSnap.z = Mathf.Round(vecToSnap.z);

                vecToSnap *= Data.rotateSnapIncrement;

                t.eulerAngles = vecToSnap;
            }
        }
    }
}
