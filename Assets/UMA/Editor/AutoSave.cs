using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[InitializeOnLoad]
public class AutoSave
{

    private const int TIME_BEFORE_SAVE = 30; // Seconds

    private static double _TimeTilSave = 0;
    private static bool _CanSave = false;

    static AutoSave()
    {


        EditorApplication.update = Update;

        // So we don't save too often, we'll make sure TIME_BEFORE_SAVE time has passed before the first
        // auto save.
        _TimeTilSave = EditorApplication.timeSinceStartup + _TimeTilSave;
    }

    static void Update()
    {
        // Are we enabled?
        if (!Config.AutoSaveEnabled)
        {
            return;
        }

        if (!_CanSave && EditorApplication.timeSinceStartup < _TimeTilSave)
        {
            // Not allowed to save yet.
            return;
        }
        else
        {
            // Open it up!
            _CanSave = true;
            _TimeTilSave = EditorApplication.timeSinceStartup + Config.AutoSaveFrequency;
        }


        // Should we save?
        if (EditorApplication.isPlayingOrWillChangePlaymode || 
            EditorApplication.isCompiling || 
            EditorApplication.timeSinceStartup > _TimeTilSave)
        {
            // SSSSSSSSAAAAAAAAAAAVVVVVVVVVVEEEEEEEEEEEE!

            // May not work on mac?
            string[] pathSplit = EditorApplication.currentScene.Split('/');
            pathSplit[pathSplit.Length - 1] = Config.AUTOSAVE_FOLDER + "/" + pathSplit[pathSplit.Length - 1];            

            string path = string.Join("/", pathSplit);

            // This isn't the best...
            pathSplit = path.Split('/');
            pathSplit[pathSplit.Length -1] = " ";

            string dirPath = string.Join("/", pathSplit);

            DirectoryInfo dir = new DirectoryInfo(dirPath);

            if (!dir.Exists)
            {
                // TODO: Unity will complain once on creation about the folder, be nice to remove that.
                dir.Create();
            }

            bool success = EditorApplication.SaveScene(path, true);

            if (!success)
            {
                Debug.LogWarning(Config.PACKAGE_NAME + " - Scene auto save failed.");
            }

            _CanSave = false;
            _TimeTilSave = EditorApplication.timeSinceStartup + TIME_BEFORE_SAVE;

        }
    }
}
