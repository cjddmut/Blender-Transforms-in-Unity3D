using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UMA
{
    public class ConfigWindow : EditorWindow
    {
        [MenuItem(Data.PACKAGE_NAME + "/Configuation Window", priority = Data.WINDOW_CONFIG_PRIORITY)]
        [MenuItem("Window/UMA Configuration")]
        static void OpenWindow()
        {
            ConfigWindow window = (ConfigWindow)EditorWindow.GetWindow(typeof(ConfigWindow));
            window.title = Data.PACKAGE_NAME + " Config";
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            // Autosave
            Data.TransformEditingEnabled = EditorGUILayout.BeginToggleGroup("Transform Edit Enabled", Data.TransformEditingEnabled);

            Data.SnappingEnabledByDefault = EditorGUILayout.Toggle("Snap By Default", Data.SnappingEnabledByDefault);
            Data.TranslateSnapIncrement = EditorGUILayout.FloatField("Translate Snap Increment", Data.TranslateSnapIncrement);
            Data.RotateSnapIncrement = EditorGUILayout.FloatField("Rotate Snap Increment", Data.RotateSnapIncrement);
            Data.ScaleSnapIncrement = EditorGUILayout.FloatField("Scale Snap Increment", Data.ScaleSnapIncrement);

            EditorGUILayout.Space();

            Data.UseTInsteadOfR = EditorGUILayout.Toggle("'T' for Rotate", Data.UseTInsteadOfR);
            Data.EnableMouseConfirmCancel = EditorGUILayout.Toggle("Enable Mouse (iffy)", Data.EnableMouseConfirmCancel);

            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            Data.AutoSaveEnabled = EditorGUILayout.BeginToggleGroup("Auto Save Enabled", Data.AutoSaveEnabled);
            Data.AutoSaveFrequency = EditorGUILayout.FloatField("Frequency (minutes)", Data.AutoSaveFrequency);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            Data.HidingEnabled = EditorGUILayout.BeginToggleGroup("Hide Objects Enabled", Data.HidingEnabled);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            Data.ResetTransformsEnabled = EditorGUILayout.BeginToggleGroup("Reset Transform Enabled", Data.ResetTransformsEnabled);
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