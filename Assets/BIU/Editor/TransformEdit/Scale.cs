using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.BlenderInUnity
{
    public class Scale : ModalEdit
    {
        private float orignalDistance;
        private Vector3 avgPos;
        private Vector3[] originalScales;
        private Transform[] selected;
        private TransformState state;

        public Scale()
        {
            state = new TransformState();
            triggerKey = Data.scaleKey;
        }

        public override void Start()
        {
            base.Start();

            // Just in case the order isn't guaranteed, I'm going to save the selecteds.
            selected = Selection.GetTransforms(SelectionMode.TopLevel);
            avgPos = Vector3.zero;
            originalScales = new Vector3[selected.Length];
            state.Init();
            state.onlyLocal = true;

            for (int i = 0; i < selected.Length; i++)
            {
                avgPos += selected[i].position;
                originalScales[i] = selected[i].localScale;
            }

            avgPos /= selected.Length;

            Vector2 avgInSP = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(avgPos);
            avgInSP.y = SceneView.lastActiveSceneView.camera.pixelHeight - avgInSP.y;
            orignalDistance = Vector2.Distance(Event.current.mousePosition, avgInSP);

            if (orignalDistance == 0)
            {
                orignalDistance = 0.1f;
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
            ResetScale();
            Undo.RecordObjects(selected, "Scale");

            state.DrawLines(avgPos, selected);
            CalculateScale();
        }

        public override void Confirm()
        {
            // Done son!
            base.Confirm();
        }

        public override void Cancel()
        {
            base.Cancel();
            ResetScale();
        }

        private void ResetScale()
        {
            for (int i = 0; i < selected.Length; i++)
            {
                selected[i].localScale = originalScales[i];
            }
        }

        private void UpdateScale(Vector3 scaleBy)
        {
            foreach (Transform t in selected)
            {
                Vector3 ls = t.localScale;

                ls.x *= scaleBy.x;
                ls.y *= scaleBy.y;
                ls.z *= scaleBy.z;

                t.localScale = ls;
            }
        }

        private void CalculateScale()
        {
            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            Vector2 mousePos = Event.current.mousePosition;
            Vector3 scaleBy = Vector3.one;
            Vector2 avgInSP = sceneCam.WorldToScreenPoint(avgPos);
            avgInSP.y = sceneCam.pixelHeight - avgInSP.y;

            float newDistance = Vector2.Distance(mousePos, avgInSP);

            // There is some slight movement even if the mouse hasn't moved (floating point?). So don't go further.
            if (newDistance - orignalDistance == 0)
            {
                return;
            }

            float scaleFactor = newDistance / orignalDistance;

            if (state.myMode == TransformState.Mode.Free)
            {
                scaleBy *= scaleFactor;
            }
            else if (state.myMode == TransformState.Mode.SingleAxis)
            {
                if (state.myAxis == TransformState.Axis.X)
                {
                    scaleBy.x *= scaleFactor;
                }
                else if (state.myAxis == TransformState.Axis.Y)
                {
                    scaleBy.y *= scaleFactor;

                }
                else
                {
                    scaleBy.z *= scaleFactor;
                }
            }
            else
            {
                scaleBy *= scaleFactor;

                if (state.myAxis == TransformState.Axis.X)
                {
                    scaleBy.x = 1;
                }
                else if (state.myAxis == TransformState.Axis.Y)
                {
                    scaleBy.y = 1;
                }
                else
                {
                    scaleBy.z = 1;
                }
            }

            UpdateScale(scaleBy);
            HandleSnapping();
        }

        private void HandleSnapping()
        {
            if (!state.isSnapping || Data.scaleSnapIncrement == 0)
            {
                return;
            }

            foreach (Transform t in selected)
            {
                Vector3 vecToSnap = t.localScale;

                vecToSnap /= Data.scaleSnapIncrement;

                vecToSnap.x = Mathf.Round(vecToSnap.x);
                vecToSnap.y = Mathf.Round(vecToSnap.y);
                vecToSnap.z = Mathf.Round(vecToSnap.z);

                vecToSnap *= Data.scaleSnapIncrement;

                t.localScale = vecToSnap;
            }
        }
    }
}
