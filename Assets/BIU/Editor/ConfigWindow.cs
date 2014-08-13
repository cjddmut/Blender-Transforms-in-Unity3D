using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.BlenderInUnity
{
    public class ConfigWindow : EditorWindow
    {
        [MenuItem("Window/Unity Made Awesome/Blender Transforms In Unity")]
        static void OpenWindow()
        {
            ConfigWindow window = (ConfigWindow)EditorWindow.GetWindow(typeof(ConfigWindow));
            window.title = "BTU Config";
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            Data.transformEditingEnabled = EditorGUILayout.BeginToggleGroup("Transform Edit Enabled", Data.transformEditingEnabled);

            Data.snappingEnabledByDefault = EditorGUILayout.Toggle("Snap By Default", Data.snappingEnabledByDefault);
            Data.translateSnapIncrement = EditorGUILayout.FloatField("Translate Snap Increment", Data.translateSnapIncrement);
            Data.rotateSnapIncrement = EditorGUILayout.FloatField("Rotate Snap Increment", Data.rotateSnapIncrement);
            Data.scaleSnapIncrement = EditorGUILayout.FloatField("Scale Snap Increment", Data.scaleSnapIncrement);

            EditorGUILayout.Space();

            Data.useTInstedOfR = EditorGUILayout.Toggle("'R' for Rotate", Data.useTInstedOfR);
            Data.enableMouseConfirmCancel = EditorGUILayout.Toggle("Enable Mouse (iffy)", Data.enableMouseConfirmCancel);

            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            Data.resetTransformsEnabled = EditorGUILayout.BeginToggleGroup("Reset Transform Hot Keys Enabled", Data.resetTransformsEnabled);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            Data.cameraControlEnabled = EditorGUILayout.BeginToggleGroup("Camera Control Hot Keys Enabled", Data.cameraControlEnabled);
            EditorGUILayout.EndToggleGroup();
        }

        void OnLostFocus()
        {
            // We lost focus, save data that might have changed.

            // I place the save data here instead of in OnDisable in case the window is left opened and "OnDisable" is never called. (Like
            // if the user does a bunch of work and then quits without playing or compiling scripts). Now we save when the window is
            // no longer being used. The only concern would be if there is a window where scripts can compile since I'm not sure if that
            // would trigger this function. If scripts will only compile based off of an action from the user then we should be fine since
            // that action will cause the window to lose focus.
            Data.SaveData();
        }
    }
}