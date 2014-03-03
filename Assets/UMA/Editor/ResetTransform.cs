using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UMA
{
    public class ResetTransform
    {
        [MenuItem(Data.PACKAGE_NAME + "/Reset/Position _&g", priority = Data.RESET_TRANSFORM_PRIORITY)]
        static void ResetPosition()
        {
            Undo.RecordObjects(Selection.transforms, "Reset Position");

            foreach (Transform transform in Selection.transforms)
            {
                transform.localPosition = Vector3.zero;
            }
        }

        [MenuItem(Data.PACKAGE_NAME + "/Reset/Position _&g", validate = true)]
        static bool ResetPositionCheck()
        {
            return Data.ResetTransformsEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/Reset/Rotation _&r", priority = Data.RESET_TRANSFORM_PRIORITY)]
        static void ResetRotation()
        {
            Undo.RecordObjects(Selection.transforms, "Reset Rotation");

            foreach (Transform transform in Selection.transforms)
            {
                transform.localRotation = Quaternion.identity;
            }

        }

        [MenuItem(Data.PACKAGE_NAME + "/Reset/Rotation _&r", validate = true)]
        static bool ResetRotationCheck()
        {
            return Data.ResetTransformsEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/Reset/Scale _&s", priority = Data.RESET_TRANSFORM_PRIORITY)]
        static void ResetScale()
        {
            Undo.RecordObjects(Selection.transforms, "Reset Scale");

            foreach (Transform transform in Selection.transforms)
            {
                transform.localScale = Vector3.one;
            }
        }

        [MenuItem(Data.PACKAGE_NAME + "/Reset/Scale _&s", validate = true)]
        static bool ResetScaleCheck()
        {
            return Data.ResetTransformsEnabled;
        }

        // TODO: Right now this is just sort of lumped in with the Reset Transform functions.
        [MenuItem(Data.PACKAGE_NAME + "/Create Empty Child _%&n", priority = Data.OTHERS_PRIORITY)]
        static void CreateEmptyChild()
        {
            // TODO: Where does it make sense to place the child? Default is 0, 0, 0 in world space.
            GameObject empty = new GameObject();
            empty.name = "GameObject";

            Transform active = Selection.activeTransform;
            empty.transform.parent = active;

            Selection.activeGameObject = empty;

            Undo.RegisterCreatedObjectUndo(empty, "Create Empty Child");
        }

        [MenuItem(Data.PACKAGE_NAME + "/Create Empty Child _%&n", validate = true)]
        static bool CreateEmptyChildCheck()  
        {
            return Data.ResetTransformsEnabled;
        }
    }
}
