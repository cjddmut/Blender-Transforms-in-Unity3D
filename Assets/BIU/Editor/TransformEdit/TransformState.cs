using UnityEngine;
using UnityEditor;
using System.Collections;

namespace BIU
{
    public class TransformState
    {
        public Mode myMode;
        public Axis myAxis;
        public Space mySpace;
        public bool isSnapping;
        public bool onlyLocal;

        private bool seenCtrlUp;

        private const float LINE_LENGTH = 500;

        private static Color xAxis = new Color(1, 0, 0, 0.5f);
        private static Color yAxis = new Color(0, 1, 0, 0.5f);
        private static Color zAxis = new Color(0, 0, 1, 0.5f);

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
            seenCtrlUp = true;
            isSnapping = Data.snappingEnabledByDefault;
            onlyLocal = false;
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
                    if (myMode == Mode.Free)
                    {
                        myMode = Mode.DoubleAxis;
                        myAxis = impactedAxis;
                    }
                    else if (myMode == Mode.SingleAxis)
                    {
                        myMode = Mode.DoubleAxis;
                        myAxis = impactedAxis;
                        mySpace = Space.World;
                    }
                    else
                    {
                        if (myAxis == impactedAxis)
                        {
                            if (mySpace == Space.World && !onlyLocal)
                            {
                                mySpace = Space.Self;
                            }
                            else
                            {
                                ResetAxisLock();
                            }
                        }
                        else
                        {
                            myAxis = impactedAxis;
                            mySpace = Space.World;
                        }
                    }
                }
                else
                {
                    // Three possibilities here.
                    if (myMode == Mode.Free)
                    {
                        myMode = Mode.SingleAxis;
                        myAxis = impactedAxis;
                    }
                    else if (myMode == Mode.SingleAxis)
                    {
                        if (myAxis == impactedAxis)
                        {
                            if (mySpace == Space.World && !onlyLocal)
                            {
                                mySpace = Space.Self;
                            }
                            else
                            {
                                ResetAxisLock();
                            }
                        }
                        else
                        {
                            myAxis = impactedAxis;
                            mySpace = Space.World;
                        }
                    }
                    else
                    {
                        myMode = Mode.SingleAxis;
                        myAxis = impactedAxis;
                        mySpace = Space.World;
                    }
                }

                Event.current.Use();
            }

            // Snapping? We don't care if it has been marked as Use though we will mark it.
            if (Event.current.control)
            {
                if (seenCtrlUp)
                {
                    isSnapping = !isSnapping;
                    Event.current.Use();
                    seenCtrlUp = false;
                }
            }
            else
            {
                seenCtrlUp = true;
            }
        }

        //
        // TransformState will own drawing the lines since otherwise this code would be duplicated in translate, rotate, and scale.
        //
        public void DrawLines(Vector3 originalAvgPos, Transform[] selected)
        {
            if (myMode == Mode.Free)
            {
                // Nothing to draw in free.
                return;
            }

            bool drawX = (myMode == Mode.SingleAxis && myAxis == Axis.X) || (myMode == Mode.DoubleAxis && myAxis != Axis.X) ? true : false;
            bool drawY = (myMode == Mode.SingleAxis && myAxis == Axis.Y) || (myMode == Mode.DoubleAxis && myAxis != Axis.Y) ? true : false;
            bool drawZ = (myMode == Mode.SingleAxis && myAxis == Axis.Z) || (myMode == Mode.DoubleAxis && myAxis != Axis.Z) ? true : false;

            Vector3 p1;
            Vector3 p2;

            if (mySpace == Space.Self || onlyLocal)
            {
                foreach (Transform t in selected)
                {
                    if (drawX)
                    {

                        p1 = t.position + t.TransformDirection(Vector3.right) * LINE_LENGTH;
                        p2 = t.position + t.TransformDirection(Vector3.left) * LINE_LENGTH;
                        Handles.color = xAxis;
                        Handles.DrawLine(p1, p2);
                    }

                    if (drawY)
                    {
                        p1 = t.position + t.TransformDirection(Vector3.up) * LINE_LENGTH;
                        p2 = t.position + t.TransformDirection(Vector3.down) * LINE_LENGTH;
                        Handles.color = yAxis;
                        Handles.DrawLine(p1, p2);
                    }

                    if (drawZ)
                    {
                        p1 = t.position + t.TransformDirection(Vector3.forward) * LINE_LENGTH;
                        p2 = t.position + t.TransformDirection(Vector3.back) * LINE_LENGTH;
                        Handles.color = zAxis;
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
                    Handles.color = xAxis;
                    Handles.DrawLine(p1, p2);
                }

                if (drawY)
                {
                    p1 = originalAvgPos;
                    p2 = originalAvgPos;
                    p1.y += LINE_LENGTH;
                    p2.y -= LINE_LENGTH;
                    Handles.color = yAxis;
                    Handles.DrawLine(p1, p2);
                }

                if (drawZ)
                {
                    p1 = originalAvgPos;
                    p2 = originalAvgPos;
                    p1.z += LINE_LENGTH;
                    p2.z -= LINE_LENGTH;
                    Handles.color = zAxis;
                    Handles.DrawLine(p1, p2);
                }
            }
        }
        private void ResetAxisLock()
        {
            myMode = Mode.Free;
            mySpace = Space.World;
            myAxis = Axis.None;
        }
    }
}