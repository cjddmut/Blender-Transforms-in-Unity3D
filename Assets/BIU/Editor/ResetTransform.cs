using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.BlenderInUnity
{
    public class ResetTransform
    {
        [MenuItem("Edit/UMA/Reset Position &g", priority = Data.RESET_TRANSFORM_PRIORITY)]
        static void ResetPosition()
        {
            Undo.RecordObjects(Selection.transforms, "Reset Position");

            foreach (Transform transform in Selection.transforms)
            {
                transform.localPosition = Vector3.zero;
            }
        }

        [MenuItem("Edit/UMA/Reset Position &g", validate = true)]
        static bool ResetPositionCheck()
        {
            return Data.resetTransformsEnabled;
        }

        [MenuItem("Edit/UMA/Reset Rotation &r", priority = Data.RESET_TRANSFORM_PRIORITY)]
        static void ResetRotation()
        {
            Undo.RecordObjects(Selection.transforms, "Reset Rotation");

            foreach (Transform transform in Selection.transforms)
            {
                transform.localRotation = Quaternion.identity;
            }

        }

        [MenuItem("Edit/UMA/Reset Rotation &r", validate = true)]
        static bool ResetRotationCheck()
        {
            return Data.resetTransformsEnabled;
        }

        [MenuItem("Edit/UMA/Reset Scale &s", priority = Data.RESET_TRANSFORM_PRIORITY)]
        static void ResetScale()
        {
            Undo.RecordObjects(Selection.transforms, "Reset Scale");

            foreach (Transform transform in Selection.transforms)
            {
                transform.localScale = Vector3.one;
            }
        }

        [MenuItem("Edit/UMA/Reset Scale &s", validate = true)]
        static bool ResetScaleCheck()
        {
            return Data.resetTransformsEnabled;
        }

        // TODO: Right now this is just sort of lumped in with the Reset Transform functions.
        [MenuItem("Edit/UMA/Create Empty Child %&n", priority = Data.OTHERS_PRIORITY)]
        static void CreateEmptyChild()
        {
            GameObject empty = new GameObject();
            empty.name = "GameObject";

            Transform active = Selection.activeTransform;
            empty.transform.parent = active;

            if (active != null)
            {
                // This will make the child's location (0, 0, 0) in local space.
                empty.transform.position = active.position;
            }

            Selection.activeGameObject = empty;

            Undo.RegisterCreatedObjectUndo(empty, "Create Empty Child");
        }

        [MenuItem("Edit/UMA/Create Empty Child %&n", validate = true)]
        static bool CreateEmptyChildCheck()  
        {
            return Data.resetTransformsEnabled;
        }
    }
}
