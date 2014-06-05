using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UMA
{
    [InitializeOnLoad]
    public class Data
    {
        //
        // Unconfigurable Data
        //

        public const string PACKAGE_NAME = "UMA";

        public const int WINDOW_CONFIG_PRIORITY = 1;
        public const int VIEW_PRIORITY = 101;
        public const int RESET_TRANSFORM_PRIORITY = 102;
        public const int HIDE_OBJECTS_PRIORITY = 201;
        public const int OTHERS_PRIORITY = 301;

        // Keys for the Transform Edit. Not const cause at least rotation can change.
        public static KeyCode TranslateKey = KeyCode.G;
        public static KeyCode RotateKey = KeyCode.T;
        public static KeyCode ScaleKey = KeyCode.S;

        //
        // Configurable data.
        //

        //
        // Auto Save
        //

        public static bool AutoSaveEnabled = true;
        public static float AutoSaveFrequency = 5f; // Minutes
        public static int SavesToKeep = 5;

        //
        // Hiding
        //

        // I'm  no longer supporting the hiding functionality as I didn't really find it useful. Turning it off by default now.
        public static bool HidingEnabled = false;

        //
        // Reset Transforms
        //

        public static bool ResetTransformsEnabled = true;

        //
        // Transform Editing
        //

        public static bool TransformEditingEnabled = true;
        public static bool SnappingEnabledByDefault = false;
        public static float TranslateSnapIncrement = 1;
        public static float RotateSnapIncrement = 45;
        public static float ScaleSnapIncrement = 1;
        public static bool UseRInsteadOfT = false;
        public static bool EnableMouseConfirmCancel = false;
        public static float TranslateOriginSize = 0.005f;
        public static Color TranslateOriginColor = new Color(0.96f, 0.77f, 0, 1);
        //
        // Camera Control
        //

        public static bool CameraControlEnabled = true;

        static Data()
        {
            // Loaded up, load up da settings.
            Data.LoadData();
        }

        public static void SaveData()
        {
            // TODO: I bet there's a way to do this easier with SerializeObject, explore later. If not, consider a cool solution
            //       using reflection later. This is currently a little unwieldy.
            EditorPrefs.SetBool(PACKAGE_NAME + " - AS", AutoSaveEnabled);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - AS Freq", AutoSaveFrequency);
            EditorPrefs.SetInt(PACKAGE_NAME + " - AS Saves", SavesToKeep);
            EditorPrefs.SetBool(PACKAGE_NAME + " - Hide", HidingEnabled);
            EditorPrefs.SetBool(PACKAGE_NAME + " - ResetT", ResetTransformsEnabled);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE", TransformEditingEnabled);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE Snap", SnappingEnabledByDefault);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - TE T Snap", TranslateSnapIncrement);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - TE R Snap", RotateSnapIncrement);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - TE S Snap", ScaleSnapIncrement);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE TiR", UseRInsteadOfT);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE Mouse", EnableMouseConfirmCancel);
            EditorPrefs.SetBool(PACKAGE_NAME + " - CC", CameraControlEnabled);

            UpdateInternalData();
        }

        public static void LoadData()
        {
            // If the first key is missing then just assume we have no data to load and go with defaults.
            if (!EditorPrefs.HasKey(PACKAGE_NAME + " - AS"))
            {
                return;
            }

            AutoSaveEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - AS");
            AutoSaveFrequency = EditorPrefs.GetFloat(PACKAGE_NAME + " - AS Freq");
            SavesToKeep = EditorPrefs.GetInt(PACKAGE_NAME + " - AS Saves");
            HidingEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - Hide");
            ResetTransformsEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - ResetT");
            TransformEditingEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - TE");
            SnappingEnabledByDefault = EditorPrefs.GetBool(PACKAGE_NAME + " - TE Snap");
            TranslateSnapIncrement = EditorPrefs.GetFloat(PACKAGE_NAME + " - TE T Snap");
            RotateSnapIncrement = EditorPrefs.GetFloat(PACKAGE_NAME + " - TE R Snap");
            ScaleSnapIncrement = EditorPrefs.GetFloat(PACKAGE_NAME + " - TE S Snap");
            UseRInsteadOfT = EditorPrefs.GetBool(PACKAGE_NAME + " - TE TiR");
            EnableMouseConfirmCancel = EditorPrefs.GetBool(PACKAGE_NAME + " - TE Mouse");
            CameraControlEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - CC");

            UpdateInternalData();
        }

        public static void UpdateInternalData()
        {
            // Any action that needs to be taken based off the data.

            if (UseRInsteadOfT)
            {
                RotateKey = KeyCode.R;
            }
        }
    }
}