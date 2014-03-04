using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UMA
{
    [InitializeOnLoad]
    public class CameraControl
    {
        // UUUGGGHHH! MenuItems present well but the number of issues/bugs with is make it frustrating...
        // Ctrl plus num row works but ctrl with num pad does not (num pad without ctrl does work).
        // So we hook into sceneview to catch ctrl+numpad events...

        // TODO: I keep having to write mouse/keyboard event handlers, I should write one that handles
        // all of them and sends messages to see if anyone wants to handle one or something.

        private static bool[] _NumPadUp;

        static CameraControl()
        {
            SceneView.onSceneGUIDelegate += SceneGUI;
            _NumPadUp = new bool[10];

            for (int i = 0; i < _NumPadUp.Length; i++)
            {
                _NumPadUp[i] = true;
            }
        }

        static void SceneGUI(SceneView sceneView)
        {
            if (!Data.CameraControlEnabled)
            {
                return;
            }

            // Handle events!

            if (Event.current.type == EventType.keyDown)
            {
                if (Event.current.keyCode == KeyCode.Keypad1 ||
                    Event.current.keyCode == KeyCode.Keypad3 ||
                    Event.current.keyCode == KeyCode.Keypad5 ||
                    Event.current.keyCode == KeyCode.Keypad7)
                {
                    // We still gotta check by individuals
                    if (Event.current.keyCode == KeyCode.Keypad1 && _NumPadUp[Event.current.keyCode - KeyCode.Keypad0])
                    {
                        if (Event.current.control)
                        {
                            LookFromBack();
                        }
                        else
                        {
                            LookFromFront();
                        }
                    }

                    if (Event.current.keyCode == KeyCode.Keypad3 && _NumPadUp[Event.current.keyCode - KeyCode.Keypad0])
                    {
                        if (Event.current.control)
                        {
                            LookFromLeft();
                        }
                        else
                        {
                            LookFromRight();
                        }
                    }

                    if (Event.current.keyCode == KeyCode.Keypad5 && _NumPadUp[Event.current.keyCode - KeyCode.Keypad0])
                    {
                        ChangePerspective();
                    }

                    if (Event.current.keyCode == KeyCode.Keypad7 && _NumPadUp[Event.current.keyCode - KeyCode.Keypad0])
                    {
                        if (Event.current.control)
                        {
                            LookFromBottom();
                        }
                        else
                        {
                            LookFromTop();
                        }
                    }

                    _NumPadUp[Event.current.keyCode - KeyCode.Keypad0] = false;
                    Event.current.Use();
                }
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                // We check if we got a key up event that we care about.
                if (Event.current.keyCode == KeyCode.Keypad1 ||
                    Event.current.keyCode == KeyCode.Keypad3 ||
                    Event.current.keyCode == KeyCode.Keypad5 ||
                    Event.current.keyCode == KeyCode.Keypad7)
                {
                    _NumPadUp[Event.current.keyCode - KeyCode.Keypad0] = true;
                    Event.current.Use();
                }
            }
        }

        // For every function I make sure 2D mode is off, this is really a 3D feature.
        [MenuItem(Data.PACKAGE_NAME + "/View/Change Perspective", priority = Data.VIEW_PRIORITY)]
        static void ChangePerspective()
        {
            SceneView.lastActiveSceneView.in2DMode = false;
            SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot, 
                                                 SceneView.lastActiveSceneView.rotation, 
                                                 SceneView.lastActiveSceneView.size, 
                                                 !SceneView.lastActiveSceneView.orthographic);
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Change Perspective", validate = true)]
        static bool ChangePerspectiveCheck()
        {
            return Data.CameraControlEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Front", priority = Data.VIEW_PRIORITY)]
        static void LookFromFront()
        {
            ApplyRotation(Quaternion.Euler(0, 180, 0));
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Front", validate = true)]
        static bool LookFromFrontCheck()
        {
            return Data.CameraControlEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Back", priority = Data.VIEW_PRIORITY)]
        static void LookFromBack()
        {
            ApplyRotation(Quaternion.Euler(0, 0, 0));
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Back", validate = true)]
        static bool LookFromBackCheck()
        {
            return Data.CameraControlEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Right", priority = Data.VIEW_PRIORITY)]
        static void LookFromRight()
        {
            ApplyRotation(Quaternion.Euler(0, 90, 0));
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Right", validate = true)]
        static bool LookFromRightCheck()
        {
            return Data.CameraControlEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Left", priority = Data.VIEW_PRIORITY)]
        static void LookFromLeft()
        {
            ApplyRotation(Quaternion.Euler(0, -90, 0));
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Left", validate = true)]
        static bool LookFromLeftCheck()
        {
            return Data.CameraControlEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Top", priority = Data.VIEW_PRIORITY)]
        static void LookFromTop()
        {
            ApplyRotation(Quaternion.Euler(90, 180, 0));
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Top", validate = true)]
        static bool LookFromTopCheck()
        {
            return Data.CameraControlEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Bottom", priority = Data.VIEW_PRIORITY)]
        static void LookFromBottom()
        {
            ApplyRotation(Quaternion.Euler(-90, 180, 0));
        }

        [MenuItem(Data.PACKAGE_NAME + "/View/Bottom", validate = true)]
        static bool LookFromLeftBottom()
        {
            return Data.CameraControlEnabled;
        }

        private static void ApplyRotation(Quaternion rot)
        {
            SceneView.lastActiveSceneView.in2DMode = false;
            Transform active = Selection.activeTransform;

            if (active != null)
            {
                // TODO: Should this be active or average transform?
                SceneView.lastActiveSceneView.LookAt(Selection.activeTransform.position, rot);
            }
            else
            {
                SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot, rot);
            }
            
            SceneView.lastActiveSceneView.Repaint();
        }
    }
}