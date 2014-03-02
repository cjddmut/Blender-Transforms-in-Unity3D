using UnityEngine;
using UnityEditor;
using System.Collections;

public class ResetTransform
{

    [MenuItem(Config.PACKAGE_NAME + "/Reset Position _&g")]
    static void ResetPosition()
    {
        Undo.RecordObjects(Selection.transforms, "Reset Position");

        foreach (Transform transform in Selection.transforms)
        {
            transform.localPosition = Vector3.zero;
        }
    }

    [MenuItem(Config.PACKAGE_NAME + "/Reset Rotation _&r")]
    static void ResetRotation()
    {
        Undo.RecordObjects(Selection.transforms, "Reset Rotation");

        foreach (Transform transform in Selection.transforms)
        {
            transform.localRotation = Quaternion.identity;
        }

    }

    [MenuItem(Config.PACKAGE_NAME + "/Reset Scale _&s")]
    static void ResetScale()
    {
        Undo.RecordObjects(Selection.transforms, "Reset Scale");

        foreach (Transform transform in Selection.transforms)
        {
            transform.localScale = Vector3.one;
        }
    }

    [MenuItem(Config.PACKAGE_NAME + "/Create Empty Child _%&n")]
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

}
