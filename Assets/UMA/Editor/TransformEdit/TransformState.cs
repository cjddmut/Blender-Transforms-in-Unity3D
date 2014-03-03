using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UMA
{
    public class TransformState
    {
        public Mode MyMode;
        public Axis MyAxis;
        public Space MySpace;
        public bool IsSnapping;
        public bool OnlyLocal;

        private bool _SeenCtrlUp;

        private const float LINE_LENGTH = 500;

        private static Color _XAxis = new Color(1, 0, 0, 0.5f);
        private static Color _YAxis = new Color(0, 1, 0, 0.5f);
        private static Color _ZAxis = new Color(0, 0, 1, 0.5f);

        public enum Mode
        {
            Free,
            SingleAxis,
            DoubleAxis
        }

        // In Free this variable doesn't matter, in SingleAxis it is the axis we are moving on, in DoubleAxis it is the omitted axis.
        public enum Axis
        {
            None,
            X,
            Y,
            Z
        }

        public void Init()
        {
            ResetAxisLock();
            _SeenCtrlUp = true;
            IsSnapping = Data.SnappingEnabledByDefault;
            OnlyLocal = false;
        }

        public void HandleEvent()
        {
            bool isDirty = false;
            bool shiftPressed = false;
            TransformState.Axis impactedAxis = TransformState.Axis.None;

            // Handle Axis control

            // If we press 'X' or 'shift+X'.
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.X))
            {
                impactedAxis = TransformState.Axis.X;
                shiftPressed = Event.current.shift;
                isDirty = true;
            }

            // If we press 'Y' or 'shift+Y'.
            else if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Y))
            {
                impactedAxis = TransformState.Axis.Y;
                shiftPressed = Event.current.shift;
                isDirty = true;
            }

            // If we press 'Z' or 'shift+Z'.
            else if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Z))
            {
                impactedAxis = Axis.Z;
                shiftPressed = Event.current.shift;
                isDirty = true;
            }

            if (isDirty)
            {
                if (shiftPressed)
                {
                    if (MyMode == Mode.Free)
                    {
                        MyMode = Mode.DoubleAxis;
                        MyAxis = impactedAxis;
                    }
                    else if (MyMode == Mode.SingleAxis)
                    {
                        MyMode = Mode.DoubleAxis;
                        MyAxis = impactedAxis;
                        MySpace = Space.World;
                    }
                    else
                    {
                        if (MyAxis == impactedAxis)
                        {
                            if (MySpace == Space.World && !OnlyLocal)
                            {
                                MySpace = Space.Self;
                            }
                            else
                            {
                                ResetAxisLock();
                            }
                        }
                        else
                        {
                            MyAxis = impactedAxis;
                            MySpace = Space.World;
                        }
                    }
                }
                else
                {
                    // Three possibilities here.
                    if (MyMode == Mode.Free)
                    {
                        MyMode = Mode.SingleAxis;
                        MyAxis = impactedAxis;
                    }
                    else if (MyMode == Mode.SingleAxis)
                    {
                        if (MyAxis == impactedAxis)
                        {
                            if (MySpace == Space.World && !OnlyLocal)
                            {
                                MySpace = Space.Self;
                            }
                            else
                            {
                                ResetAxisLock();
                            }
                        }
                        else
                        {
                            MyAxis = impactedAxis;
                            MySpace = Space.World;
                        }
                    }
                    else
                    {
                        MyMode = Mode.SingleAxis;
                        MyAxis = impactedAxis;
                        MySpace = Space.World;
                    }
                }

                Event.current.Use();
            }

            // Snapping? We don't care if it has been marked as Use though we will mark it.
            if (Event.current.control)
            {
                if (_SeenCtrlUp)
                {
                    IsSnapping = !IsSnapping;
                    Event.current.Use();
                    _SeenCtrlUp = false;
                }
            }
            else
            {
                _SeenCtrlUp = true;
            }
        }

        //
        // TransformState will own drawing the lines since otherwise this code would be duplicated in translate, rotate, and scale.
        //
        public void DrawLines(Vector3 originalAvgPos, Transform[] selected)
        {
            if (MyMode == Mode.Free)
            {
                // Nothing to draw in free.
                return;
            }

            bool drawX = (MyMode == Mode.SingleAxis && MyAxis == Axis.X) || (MyMode == Mode.DoubleAxis && MyAxis != Axis.X) ? true : false;
            bool drawY = (MyMode == Mode.SingleAxis && MyAxis == Axis.Y) || (MyMode == Mode.DoubleAxis && MyAxis != Axis.Y) ? true : false;
            bool drawZ = (MyMode == Mode.SingleAxis && MyAxis == Axis.Z) || (MyMode == Mode.DoubleAxis && MyAxis != Axis.Z) ? true : false;

            Vector3 p1;
            Vector3 p2;

            if (MySpace == Space.Self || OnlyLocal)
            {
                foreach (Transform t in selected)
                {
                    if (drawX)
                    {

                        p1 = t.position + t.TransformDirection(Vector3.right) * LINE_LENGTH;
                        p2 = t.position + t.TransformDirection(Vector3.left) * LINE_LENGTH;
                        Handles.color = _XAxis;
                        Handles.DrawLine(p1, p2);
                    }

                    if (drawY)
                    {
                        p1 = t.position + t.TransformDirection(Vector3.up) * LINE_LENGTH;
                        p2 = t.position + t.TransformDirection(Vector3.down) * LINE_LENGTH;
                        Handles.color = _YAxis;
                        Handles.DrawLine(p1, p2);
                    }

                    if (drawZ)
                    {
                        p1 = t.position + t.TransformDirection(Vector3.forward) * LINE_LENGTH;
                        p2 = t.position + t.TransformDirection(Vector3.back) * LINE_LENGTH;
                        Handles.color = _ZAxis;
                        Handles.DrawLine(p1, p2);
                    }
                }
            }
            else
            {
                if (drawX)
                {
                    p1 = originalAvgPos;
                    p2 = originalAvgPos;
                    p1.x += LINE_LENGTH;
                    p2.x -= LINE_LENGTH;
                    Handles.color = _XAxis;
                    Handles.DrawLine(p1, p2);
                }

                if (drawY)
                {
                    p1 = originalAvgPos;
                    p2 = originalAvgPos;
                    p1.y += LINE_LENGTH;
                    p2.y -= LINE_LENGTH;
                    Handles.color = _YAxis;
                    Handles.DrawLine(p1, p2);
                }

                if (drawZ)
                {
                    p1 = originalAvgPos;
                    p2 = originalAvgPos;
                    p1.z += LINE_LENGTH;
                    p2.z -= LINE_LENGTH;
                    Handles.color = _ZAxis;
                    Handles.DrawLine(p1, p2);
                }
            }
        }
        private void ResetAxisLock()
        {
            MyMode = Mode.Free;
            MySpace = Space.World;
            MyAxis = Axis.None;
        }
    }
}