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

        // Use this for initialization
        static TransformManager()
        {
            SceneView.onSceneGUIDelegate += SceneGUI;

            // Create our model edit singletons.
            _TranslateEdit = new Translate();
            _RotateEdit = new Rotate();
            _ScaleEdit = new Scale();
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

                // We force the scene to continue to update if we are in a mode.
                HandleUtility.Repaint();
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

        }

        public static void ModalFinished()
        {
            _ActiveModal = null;
        }

        private static void FixStupidUnityBug()
        {
            // For MenuItems it seems that unity won't allow non modifers for Windows, this catches that and fixes it.
            // I will use consider each event used to prevent other features from using it, this could cause issues.

            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (!Event.current.shift && Event.current.keyCode == KeyCode.H)
                {
                    HideManager.HideObjects();
                    Event.current.Use();
                }

                if (Event.current.shift && Event.current.keyCode == KeyCode.H)
                {
                    // Unity is really dumb. Shift + H MenuItem will stop you from typing an uppercase H when renaming an object...
                    HideManager.HideOthers();
                    Event.current.Use();
                }
            }
        }
    }
}
