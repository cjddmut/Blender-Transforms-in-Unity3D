using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityMadeAwesome.BlenderInUnity
{
    [InitializeOnLoad]
    public class TransformManager
    {
        private static Translate translateEdit;
        private static Rotate rotateEdit;
        private static Scale scaleEdit;
        private static ModalEdit activeModal;
        private static ModalEdit delayStart;

        private static bool swallowMouse = false;
        private static int mouseButton;

        // Use this for initialization
        static TransformManager()
        {
            SceneView.onSceneGUIDelegate += SceneGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyOnGUI;

            // Create our model edit singletons.
            translateEdit = new Translate();
            rotateEdit = new Rotate();
            scaleEdit = new Scale();
        }

        static void HierarchyOnGUI(int i, Rect r)
        {
            if (!Data.transformEditingEnabled)
            {
                return;
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == Data.translateKey)
                {
                    SceneView.lastActiveSceneView.Focus();
                    Event.current.Use();

                    // Hey translate! We'll start it on next SceneGUI!
                    delayStart = translateEdit;
                }
                
                if (Event.current.keyCode == Data.rotateKey)
                {
                    SceneView.lastActiveSceneView.Focus();
                    Event.current.Use();
                    delayStart = rotateEdit;
                }

                if (Event.current.keyCode == Data.scaleKey)
                {
                    SceneView.lastActiveSceneView.Focus();
                    Event.current.Use();
                    delayStart = scaleEdit;
                }

            }
        }

        static void SceneGUI(SceneView sceneView)
        {
            if (!Data.transformEditingEnabled)
            {
                return;
            }

            if (activeModal != null)
            {
                activeModal.Update();

                if (EditorWindow.focusedWindow != sceneView)
                {
                    // SceneView lost focus but we're in a mode so we force it back.
                    sceneView.Focus();
                }

                // We force the scene to continue to update if we are in a mode.
                HandleUtility.Repaint();
            }

            if (delayStart != null)
            {
                // We got a message to start!
                if (activeModal != null)
                {
                    activeModal.Cancel();
                }

                activeModal = delayStart;
                delayStart = null;
                activeModal.Start();
            }


            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == Data.translateKey)
                {
                    Event.current.Use();

                    if (activeModal != null)
                    {
                        activeModal.Cancel();
                    }

                    activeModal = translateEdit;
                    activeModal.Start();
                }
                else if (Event.current.keyCode == Data.rotateKey)
                {
                    Event.current.Use();

                    if (activeModal != null)
                    {
                        activeModal.Cancel();
                    }

                    activeModal = rotateEdit;
                    activeModal.Start();
                }
                if (Event.current.keyCode == Data.scaleKey)
                {
                    Event.current.Use();

                    if (activeModal != null)
                    {
                        activeModal.Cancel();
                    }

                    activeModal = scaleEdit;
                    activeModal.Start();
                }
            }

            if (swallowMouse)
            {
                if (Event.current.button == mouseButton)
                {
                    if (Event.current.type == EventType.MouseUp)
                    {
                        swallowMouse = false;
                    }
                    
                    Event.current.Use();
                }
            }
        }

        public static void ModalFinished()
        {
            activeModal = null;
        }

        public static void SwallowMouseUntilUp(int button)
        {
            swallowMouse = true;
            mouseButton = button;
        }
    }
}
