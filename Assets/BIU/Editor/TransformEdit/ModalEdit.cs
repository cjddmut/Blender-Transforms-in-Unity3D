using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.BlenderInUnity
{
    public abstract class ModalEdit
    {
        public KeyCode triggerKey;
        protected bool isInMode;

        private int controlID;
        private Tool tool;

        public virtual void Start()
        {
            controlID = EditorGUIUtility.hotControl;
            isInMode = true;

            if (Data.enableMouseConfirmCancel)
            {
                tool = Tools.current;
                Tools.current = Tool.None;
            }
        }

        public virtual void Update()
        {
            HandleUtility.AddDefaultControl(0);
            HandleCancelConfirmEvents();
        }

        public virtual void Confirm()
        {
            Done();
        }

        public virtual void Cancel()
        {
            Done();
        }

        private void Done()
        {
            TransformManager.ModalFinished();
            isInMode = false;
            HandleUtility.AddDefaultControl(controlID);

            if (Data.enableMouseConfirmCancel)
            {
                Tools.current = tool;
            }
        }

        private void HandleCancelConfirmEvents()
        {
            // Cancel or confirm?
            if (Data.enableMouseConfirmCancel && Event.current.type == EventType.MouseDown && Event.current.button == 0 ||
                Event.current.type == EventType.KeyDown &&
                    (Event.current.keyCode == KeyCode.Return ||
                     Event.current.keyCode == KeyCode.KeypadEnter ||
                     Event.current.keyCode == triggerKey))
            {
                Confirm();
                Event.current.Use();
            }
            else
            {
                bool shouldCancel = false;

                if (Data.enableMouseConfirmCancel && Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    // We canceled with a mouse click, tell TransformManager to swallow all right clicks until an up is received.
                    TransformManager.SwallowMouseUntilUp(Event.current.button);
                    shouldCancel = true;
                }

                if (shouldCancel || Event.current.type == EventType.KeyDown && 
                     (Event.current.keyCode == KeyCode.Escape || 
                      Event.current.keyCode == KeyCode.Space))
                {
                    Cancel();
                    Event.current.Use();
                }
            }
        }
    }
}