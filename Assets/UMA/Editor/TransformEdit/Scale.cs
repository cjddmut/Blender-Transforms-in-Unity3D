using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UMA
{
    public class Scale : ModalEdit
    {
        private float _OrignalDistance;
        private Vector3 _AvgPos;
        private Vector3[] _OriginalScales;
        private Transform[] _Selected;
        private TransformState _State;

        public Scale()
        {
            _State = new TransformState();
            TriggerKey = Data.ScaleKey;
        }

        public override void Start()
        {
            base.Start();

            // Just in case the order isn't guaranteed, I'm going to save the selecteds.
            _Selected = Selection.GetTransforms(SelectionMode.TopLevel);
            _AvgPos = Vector3.zero;
            _OriginalScales = new Vector3[_Selected.Length];
            _State.Init();
            _State.OnlyLocal = true;

            for (int i = 0; i < _Selected.Length; i++)
            {
                _AvgPos += _Selected[i].position;
                _OriginalScales[i] = _Selected[i].localScale;
            }

            _AvgPos /= _Selected.Length;

            Vector2 avgInSP = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(_AvgPos);
            avgInSP.y = SceneView.lastActiveSceneView.camera.pixelHeight - avgInSP.y;
            _OrignalDistance = Vector2.Distance(Event.current.mousePosition, avgInSP);

            if (_OrignalDistance == 0)
            {
                _OrignalDistance = 0.1f;
            }

            Undo.IncrementCurrentGroup();
        }

        public override void Update()
        {
            base.Update();

            if (!IsInMode)
            {
                return;
            }

            _State.HandleEvent();
            
            // We reset everything to push to the UNDO stack.
            ResetScale();
            Undo.RecordObjects(_Selected, "Scale");

            _State.DrawLines(_AvgPos, _Selected);
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
            for (int i = 0; i < _Selected.Length; i++)
            {
                _Selected[i].localScale = _OriginalScales[i];
            }
        }

        private void UpdateScale(Vector3 scaleBy)
        {
            foreach (Transform t in _Selected)
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
            Vector2 avgInSP = sceneCam.WorldToScreenPoint(_AvgPos);
            avgInSP.y = sceneCam.pixelHeight - avgInSP.y;

            float newDistance = Vector2.Distance(mousePos, avgInSP);

            // There is some slight movement even if the mouse hasn't moved (floating point?). So don't go further.
            if (newDistance - _OrignalDistance == 0)
            {
                return;
            }

            float scaleFactor = newDistance / _OrignalDistance;

            if (_State.MyMode == TransformState.Mode.Free)
            {
                scaleBy *= scaleFactor;
            }
            else if (_State.MyMode == TransformState.Mode.SingleAxis)
            {
                if (_State.MyAxis == TransformState.Axis.X)
                {
                    scaleBy.x *= scaleFactor;
                }
                else if (_State.MyAxis == TransformState.Axis.Y)
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

                if (_State.MyAxis == TransformState.Axis.X)
                {
                    scaleBy.x = 1;
                }
                else if (_State.MyAxis == TransformState.Axis.Y)
                {
                    scaleBy.y = 1;
                }
                else
                {
                    scaleBy.z = 1;
                }
            }

            if (_State.IsSnapping)
            {
                scaleBy = HandleSnapping(scaleBy);
            }

            UpdateScale(scaleBy);
        }

        private Vector3 HandleSnapping(Vector3 vecToSnap)
        {
            vecToSnap /= Data.ScaleSnapIncrement;

            vecToSnap.x = Mathf.Round(vecToSnap.x);
            vecToSnap.y = Mathf.Round(vecToSnap.y);
            vecToSnap.z = Mathf.Round(vecToSnap.z);

            vecToSnap *= Data.ScaleSnapIncrement;

            return vecToSnap;
        }
    }
}
