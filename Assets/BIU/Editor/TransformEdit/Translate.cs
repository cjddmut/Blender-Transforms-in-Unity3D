using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.BlenderInUnity
{
    public class Translate : ModalEdit
    {
        private Vector2 originalMousePos;
        private Vector3[] toObjects;
        private Vector3 originalAvgPos;
        private Vector3 lastKnownGoodPos;
        private Vector3[] lastKnownGoodLocalPos;
        private Transform[] selected;
        private TransformState state;

        private const float MOVEMENT_THRESHOLD = 0.01f;

        public Translate()
        {
            state = new TransformState();
            triggerKey = Data.translateKey;
        }

        public override void Start()
        {
            base.Start();

            originalMousePos = Event.current.mousePosition;
            originalMousePos.y = SceneView.lastActiveSceneView.camera.pixelHeight - originalMousePos.y;

            // Just in case the order isn't guaranteed, I'm going to save the selecteds.
            selected = Selection.GetTransforms(SelectionMode.TopLevel);
            toObjects = new Vector3[selected.Length];
            lastKnownGoodLocalPos = new Vector3[selected.Length];
            originalAvgPos = Vector3.zero;
            state.Init();

            for (int i = 0; i < selected.Length; i++)
            {
                originalAvgPos += selected[i].position;
                lastKnownGoodLocalPos[i] = selected[i].position;
            }

            originalAvgPos /= selected.Length;
            lastKnownGoodPos = originalAvgPos;

            // Now that we have the average position, we get a bunch of vectors to each objects.
            for (int i = 0; i < selected.Length; i++)
            {
                toObjects[i] = selected[i].position - originalAvgPos;
            }

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
            UpdatePositions(originalAvgPos);
            Undo.RecordObjects(selected, "Translate");

            state.DrawLines(originalAvgPos, selected);

            CalculatePosition();
        }

        public override void Confirm()
        {
            // Done son!
            base.Confirm();
        }

        public override void Cancel()
        {
            base.Cancel();
            UpdatePositions(originalAvgPos);
        }

        private void UpdatePositions(Vector3 from)
        {
            from = CheckThreshold(from);

            if (state.isSnapping)
            {
                from = HandleSnapping(from);
            }

            lastKnownGoodPos = from;

            for (int i = 0; i < selected.Length; i++)
            {
                selected[i].position = from + toObjects[i];
            }
        }

        private void UpdateSinglePosition(Vector3 moveTo, int index)
        {
            moveTo = CheckThreshold(moveTo);

            if (state.isSnapping)
            {
                moveTo = HandleSnapping(moveTo);
            }

            lastKnownGoodLocalPos[index] = moveTo;
            selected[index].position = moveTo;
        }

        private void CalculatePosition()
        {
            Camera sceneCam = SceneView.lastActiveSceneView.camera;

            // Raw mouse coordinates have y flipped from what unity uses.
            Vector2 mousePos = Event.current.mousePosition;
            mousePos.y = sceneCam.pixelHeight - mousePos.y;

            Vector2 toNewMouse = mousePos - originalMousePos;

            Vector2 objInSP = sceneCam.WorldToScreenPoint(originalAvgPos);
            Vector2 newObjPosInSP = objInSP + toNewMouse;
            Vector3 camViewPlaneNormal = Util.GetCamViewPlaneNormal(sceneCam);

            // There is some slight movement even if the mouse hasn't moved (floating point?). So don't go further.
            if (toNewMouse.sqrMagnitude != 0)
            {
                Vector3 moveTo = Vector3.zero;
                Plane movePlane = new Plane();
                Ray rayToNewPos = sceneCam.ScreenPointToRay(newObjPosInSP);

                if (state.mySpace == Space.World)
                {
                    if (state.myMode == TransformState.Mode.Free)
                    {
                        movePlane = new Plane(camViewPlaneNormal, originalAvgPos);
                        moveTo = CastRayAndGetPosition(movePlane, rayToNewPos);
                    }
                    else if (state.myMode == TransformState.Mode.SingleAxis)
                    {
                        Vector3 planeNormal = camViewPlaneNormal;
                        moveTo = originalAvgPos;

                        if (state.myAxis == TransformState.Axis.X)
                        {
                            planeNormal.x = 0;
                        }
                        else if (state.myAxis == TransformState.Axis.Y)
                        {
                            planeNormal.y = 0;
                        }
                        else
                        {
                            planeNormal.z = 0;
                        }

                        moveTo = GetGlobalProjectedAxisMotion(planeNormal.normalized, rayToNewPos);
                    }
                    else
                    {
                        if (state.myAxis == TransformState.Axis.X)
                        {
                            movePlane = new Plane(Vector3.Cross(Vector3.up, Vector3.forward), originalAvgPos);
                        }
                        else if (state.myAxis == TransformState.Axis.Y)
                        {
                            movePlane = new Plane(Vector3.Cross(Vector3.right, Vector3.forward), originalAvgPos);
                        }
                        else
                        {
                            movePlane = new Plane(Vector3.Cross(Vector3.right, Vector3.up), originalAvgPos);
                        }

                        moveTo = CastRayAndGetPosition(movePlane, rayToNewPos);
                    }

                    UpdatePositions(moveTo);
                }
                else
                {
                    for (int i = 0; i < selected.Length; i++)
                    {
                        // Local space!
                        if (state.myMode == TransformState.Mode.SingleAxis)
                        {
                            // Working in local space for simplicity.
                            Vector3 camViewPlaneNormalLocal = selected[i].InverseTransformDirection(camViewPlaneNormal);

                            Ray rayToNewPosLocal = new Ray();
                            rayToNewPosLocal.origin = selected[i].InverseTransformPoint(rayToNewPos.origin);
                            rayToNewPosLocal.direction = selected[i].InverseTransformDirection(rayToNewPos.direction);

                            if (state.myAxis == TransformState.Axis.X)
                            {
                                camViewPlaneNormalLocal.x = 0;
                            }
                            else if (state.myAxis == TransformState.Axis.Y)
                            {
                                camViewPlaneNormalLocal.y = 0;
                            }
                            else
                            {
                                camViewPlaneNormalLocal.z = 0;
                            }

                            moveTo = GetLocalProjectedAxisMotion(camViewPlaneNormalLocal, rayToNewPosLocal, i);

                            // Convert back to global space.
                            moveTo = selected[i].TransformPoint(moveTo);
                        }
                        else
                        {
                            Vector3 vec1 = Vector3.zero;
                            Vector3 vec2 = Vector3.zero; ;

                            if (state.myAxis == TransformState.Axis.X)
                            {
                                vec1 = selected[i].TransformDirection(Vector3.up);
                                vec2 = selected[i].TransformDirection(Vector3.forward);
                            }
                            else if (state.myAxis == TransformState.Axis.Y)
                            {
                                vec1 = selected[i].TransformDirection(Vector3.right);
                                vec2 = selected[i].TransformDirection(Vector3.forward);
                            }
                            else
                            {
                                vec1 = selected[i].TransformDirection(Vector3.right);
                                vec2 = selected[i].TransformDirection(Vector3.up);
                            }

                            movePlane = new Plane(Vector3.Cross(vec1, vec2), selected[i].position);
                            moveTo = CastRayAndGetPositionLocal(movePlane, rayToNewPos, i);
                        }

                        UpdateSinglePosition(moveTo, i);
                    }
                }
            }

            Handles.color = Data.translateOriginColor;

            if (state.mySpace == Space.World)
            {
                Handles.DrawSolidDisc(lastKnownGoodPos,
                                      camViewPlaneNormal,
                                      Data.translateOriginSize * (sceneCam.transform.position - lastKnownGoodPos).magnitude);
            }
            else
            {
                foreach(Vector3 v in lastKnownGoodLocalPos)
                {
                    Handles.DrawSolidDisc(v,
                                          camViewPlaneNormal,
                                          Data.translateOriginSize * (sceneCam.transform.position - v).magnitude);
                }
            }

        }

        private Vector3 CheckThreshold(Vector3 v)
        {
            // Floating point thresholds!
            if (Mathf.Abs(v.x - originalAvgPos.x) < MOVEMENT_THRESHOLD)
            {
                v.x = originalAvgPos.x;
            }

            if (Mathf.Abs(v.y - originalAvgPos.y) < MOVEMENT_THRESHOLD)
            {
                v.y = originalAvgPos.y;
            }

            if (Mathf.Abs(v.z - originalAvgPos.z) < MOVEMENT_THRESHOLD)
            {
                v.z = originalAvgPos.z;
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
                return lastKnownGoodPos;
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
                return lastKnownGoodLocalPos[index];
            }
        }

        private Vector3 GetGlobalProjectedAxisMotion(Vector3 planeNormal, Ray mouseRay)
        {
            Plane movePlane = new Plane(planeNormal, originalAvgPos);
            float distance;
            Vector3 contactPoint;
            Vector3 moveTo = originalAvgPos;

            if (movePlane.Raycast(mouseRay, out distance))
            {
                contactPoint = mouseRay.origin + (mouseRay.direction * distance);

                if (state.myAxis == TransformState.Axis.X)
                {
                    moveTo.x = contactPoint.x;
                }
                else if (state.myAxis == TransformState.Axis.Y)
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
                moveTo = lastKnownGoodPos;
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

                if (state.myAxis == TransformState.Axis.X)
                {
                    moveTo.x = contactPoint.x;
                }
                else if (state.myAxis == TransformState.Axis.Y)
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
                moveTo = selected[index].InverseTransformPoint(lastKnownGoodLocalPos[index]);
            }

            return moveTo;
        }

        private Vector3 HandleSnapping(Vector3 vecToSnap)
        {
            if (Data.translateSnapIncrement == 0)
            {
                // I guess no snapping...
                return vecToSnap;
            }

            vecToSnap /= Data.translateSnapIncrement;

            vecToSnap.x = Mathf.Round(vecToSnap.x);
            vecToSnap.y = Mathf.Round(vecToSnap.y);
            vecToSnap.z = Mathf.Round(vecToSnap.z);

            vecToSnap *= Data.translateSnapIncrement;

            return vecToSnap;
        }
    }
}
