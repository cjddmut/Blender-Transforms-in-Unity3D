using UnityEngine;
using System.Collections;

public class Config
{
    // Well, this isn't configurable but whatevs.
    public const string PACKAGE_NAME = "UMA";
    public const string AUTOSAVE_FOLDER = "umasaves";

    //
    // Configurable data.
    //

    //
    // Auto Save
    //

    public static bool AutoSaveEnabled = true;
    public static float AutoSaveFrequency = 5f; // Minutes

    //
    // Hiding
    //

    public static bool HidingEnabled = true;

    //
    // Reset Transforms
    //

    public static bool ResetTransformsEnabled = true;

    //
    // Transform Editing
    //

    public static bool TransformEditingEnabled = true;
    public static bool UseTInsteadOfR = false;
    public static bool EnableMouseConfirmCancel = false;
}
