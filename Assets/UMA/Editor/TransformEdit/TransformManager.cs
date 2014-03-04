using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UMA
{
    [InitializeOnLoad]
    public class TransformManager
    {
        private static Translate _TranslateEdit;
        private static Rotate _RotateEdit;
        private static Scale _ScaleEdit;
        private static ModalEdit _ActiveModal;
        private static ModalEdit _DelayStart;

        private static bool _SwallowMouse = false;
        private static int _MouseButton;

        // Use this for initialization
        static TransformManager()
        {
            SceneView.onSceneGUIDelegate += SceneGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyOnGUI;

            // Create our model edit singletons.
            _TranslateEdit = new Translate();
            _RotateEdit = new Rotate();
            _ScaleEdit = new Scale();
        }

        static void HierarchyOnGUI(int i, Rect r)
        {
            if (!Data.TransformEditingEnabled)
            {
                return;
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == Data.TranslateKey)
                {
                    SceneView.lastActiveSceneView.Focus();
                    Event.current.Use();

                    // Hey translate! We'll start it on next SceneGUI!
                    _DelayStart = _TranslateEdit;
                }
                
                if (Event.current.keyCode == Data.RotateKey)
                {
                    SceneView.lastActiveSceneView.Focus();
                    Event.current.Use();
                    _DelayStart = _RotateEdit;
                }

                if (Event.current.keyCode == Data.ScaleKey)
                {
                    SceneView.lastActiveSceneView.Focus();
                    Event.current.Use();
                    _DelayStart = _ScaleEdit;
                }

            }
        }

        static void SceneGUI(SceneView sceneView)
        {
            if (!Data.TransformEditingEnabled)
            {
                return;
            }

            if (_ActiveModal != null)
            {
                _ActiveModal.Update();

                if (EditorWindow.focusedWindow != sceneView)
                {
                    // SceneView lost focus but we're in a mode so we force it back.
                    sceneView.Focus();
                }

                // We force the scene to continue to update if we are in a mode.
                HandleUtility.Repaint();
            }

            if (_DelayStart != null)
            {
                // We got a message to start!
                if (_ActiveModal != null)
                {
                    _ActiveModal.Cancel();
                }

                _ActiveModal = _DelayStart;
                _DelayStart = null;
                _ActiveModal.Start();
            }


            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == Data.TranslateKey)
                {
                    Event.current.Use();

                    if (_ActiveModal != null)
                    {
                        _ActiveModal.Cancel();
                    }

                    _ActiveModal = _TranslateEdit;
                    _ActiveModal.Start();
                }
                else if (Event.current.keyCode == Data.RotateKey)
                {
                    Event.current.Use();

                    if (_ActiveModal != null)
                    {
                        _ActiveModal.Cancel();
                    }

                    _ActiveModal = _RotateEdit;
                    _ActiveModal.Start();
                }
                if (Event.current.keyCode == Data.ScaleKey)
                {
                    Event.current.Use();

                    if (_ActiveModal != null)
                    {
                        _ActiveModal.Cancel();
                    }

                    _ActiveModal = _ScaleEdit;
                    _ActiveModal.Start();
                }
            }

            if (_SwallowMouse)
            {
                if (Event.current.button == _MouseButton)
                {
                    if (Event.current.type == EventType.MouseUp)
                    {
                        _SwallowMouse = false;
                    }
                    
                    Event.current.Use();
                }
            }
        }

        public static void ModalFinished()
        {
            _ActiveModal = null;
        }

        public static void SwallowMouseUntilUp(int button)
        {
            _SwallowMouse = true;
            _MouseButton = button;
        }
    }
}
