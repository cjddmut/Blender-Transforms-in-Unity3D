using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.BlenderInUnity
{
    [InitializeOnLoad]
    public class Data
    {
        //
        // Unconfigurable Data
        //

        public const string PACKAGE_NAME = "UMA.BIU";

        public const int WINDOW_CONFIG_PRIORITY = 1;
        public const int VIEW_PRIORITY = 101;
        public const int RESET_TRANSFORM_PRIORITY = 102;
        public const int OTHERS_PRIORITY = 301;

        // Keys for the Transform Edit. Not const cause at least rotation can change.
        public static KeyCode translateKey = KeyCode.G;
        public static KeyCode rotateKey = KeyCode.R;
        public static KeyCode scaleKey = KeyCode.S;

        //
        // Configurable data.
        //

        //
        // Reset Transforms
        //

        public static bool resetTransformsEnabled = true;

        //
        // Transform Editing
        //

        public static bool transformEditingEnabled = true;
        public static bool snappingEnabledByDefault = false;
        public static float translateSnapIncrement = 1;
        public static float rotateSnapIncrement = 45;
        public static float scaleSnapIncrement = 1;
        public static bool useTInstedOfR = false;
        public static bool enableMouseConfirmCancel = false;
        public static float translateOriginSize = 0.005f;
        public static Color translateOriginColor = new Color(0.96f, 0.77f, 0, 1);
        //
        // Camera Control
        //

        public static bool cameraControlEnabled = true;

        static Data()
        {
            // Loaded up, load up da settings.
            Data.LoadData();
        }

        public static void SaveData()
        {
            // TODO: I bet there's a way to do this easier with SerializeObject, explore later. If not, consider a cool solution
            //       using reflection later. This is currently a little unwieldy.
            EditorPrefs.SetBool(PACKAGE_NAME + " - ResetT", resetTransformsEnabled);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE", transformEditingEnabled);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE Snap", snappingEnabledByDefault);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - TE T Snap", translateSnapIncrement);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - TE R Snap", rotateSnapIncrement);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - TE S Snap", scaleSnapIncrement);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE TiR", useTInstedOfR);
            EditorPrefs.SetBool(PACKAGE_NAME + " - TE Mouse", enableMouseConfirmCancel);
            EditorPrefs.SetBool(PACKAGE_NAME + " - CC", cameraControlEnabled);

            UpdateInternalData();
        }

        public static void LoadData()
        {
            // If the first key is missing then just assume we have no data to load and go with defaults.
            if (!EditorPrefs.HasKey(PACKAGE_NAME + " - ResetT"))
            {
                return;
            }

            resetTransformsEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - ResetT");
            transformEditingEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - TE");
            snappingEnabledByDefault = EditorPrefs.GetBool(PACKAGE_NAME + " - TE Snap");
            translateSnapIncrement = EditorPrefs.GetFloat(PACKAGE_NAME + " - TE T Snap");
            rotateSnapIncrement = EditorPrefs.GetFloat(PACKAGE_NAME + " - TE R Snap");
            scaleSnapIncrement = EditorPrefs.GetFloat(PACKAGE_NAME + " - TE S Snap");
            useTInstedOfR = EditorPrefs.GetBool(PACKAGE_NAME + " - TE TiR");
            enableMouseConfirmCancel = EditorPrefs.GetBool(PACKAGE_NAME + " - TE Mouse");
            cameraControlEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - CC");

            UpdateInternalData();
        }

        public static void UpdateInternalData()
        {
            // Any action that needs to be taken based off the data.

            if (useTInstedOfR)
            {
                rotateKey = KeyCode.T;
            }
        }
    }
}