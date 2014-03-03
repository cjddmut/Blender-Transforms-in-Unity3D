using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UMA
{
    public abstract class ModalEdit
    {
        public KeyCode TriggerKey;
        protected bool IsInMode;

        private int _ControlID;
        private Tool _Tool;

        public virtual void Start()
        {
            _ControlID = EditorGUIUtility.hotControl;
            IsInMode = true;

            if (Data.EnableMouseConfirmCancel)
            {
                _Tool = Tools.current;
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
            IsInMode = false;
            HandleUtility.AddDefaultControl(_ControlID);

            if (Data.EnableMouseConfirmCancel)
            {
                Tools.current = _Tool;
            }
        }

        private void HandleCancelConfirmEvents()
        {
            // Cancel or confirm?
            if (Data.EnableMouseConfirmCancel && Event.current.type == EventType.MouseDown && Event.current.button == 0 ||
                Event.current.type == EventType.KeyDown &&
                    (Event.current.keyCode == KeyCode.Return ||
                     Event.current.keyCode == KeyCode.KeypadEnter ||
                     Event.current.keyCode == TriggerKey))
            {
                Confirm();
                Event.current.Use();
            }
            else
            {
                bool shouldCancel = false;

                if (Data.EnableMouseConfirmCancel && Event.current.type == EventType.MouseDown && Event.current.button == 1)
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