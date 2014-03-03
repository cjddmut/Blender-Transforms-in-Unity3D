using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.IO;

namespace UMA
{
    [InitializeOnLoad]
    public class HideManager : UnityEditor.AssetModificationProcessor
    {
        private static Stack _HiddenObjects = new Stack();
        private static bool _RehideObjects;
        private static bool _SeenHideKeyUp = true;

        static HideManager()
        {
            EditorApplication.update += Update;
            SceneView.onSceneGUIDelegate += SceneGUI;
            _RehideObjects = false;
        }

        static void SceneGUI(SceneView sceneView)
        {
            if (!Data.HidingEnabled)
            {
                return;
            }

            // As far as I can tell/figure out, you have to hook into sceneView to get events.

            // This is to fix a really stupid bug in Unity...
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.H)
            {
                if (_SeenHideKeyUp)
                {
                    // Hey we can do something! Is Shift being held?
                    if (Event.current.shift)
                    {
                        HideOthers();
                    }
                    else
                    {
                        HideObjects();
                    }

                    Event.current.Use();
                    _SeenHideKeyUp = false;
                }
            }
            else
            {
                _SeenHideKeyUp = true;
            }

        }

        static void Update()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && _HiddenObjects.Count != 0)
            {
                // On play we just show the hidden objects. Scripts are reinitiliazed after play so we lose state.
                // If there's a strong desire to persist then write out state to either a file or use
                // the EditorPrefs class to store in Registry and reload after play.
                UnhideObjects();
            }

            if (_RehideObjects)
            {
                // Must have just saved, rehide the objects.
                SetVisible(false);
            }

            if (EditorApplication.isCompiling && _HiddenObjects.Count != 0)
            {
                // If scripts are compiled then we lose state, if this makes this functionality worthless
                // then store state externally.
                UnhideObjects();
            }
        }

        public static string[] OnWillSaveAssets(string[] paths)
        {
            // Get the name of the scene to save.
            string sceneName = string.Empty;

            foreach (string path in paths)
            {
                if (path.Contains(".unity"))
                {
                    sceneName = Path.GetFileNameWithoutExtension(path);
                }
            }

            if (sceneName.Length == 0)
            {
                return paths;
            }

            // We're saving the scene so let's make sure we unhide those objects from Hide Manager. Though
            // the scene does save, it'll keep the '*' though.
            HideManager.TempShowObjects();

            return paths;
        }

        [MenuItem(Data.PACKAGE_NAME + "/Hide/Hide Object(s)", priority = Data.HIDE_OBJECTS_PRIORITY)]
        public static void HideObjects()
        {
            foreach (GameObject go in Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel))
            {
                if (go.renderer && go.renderer.enabled)
                {
                    _HiddenObjects.Push(go);
                    go.renderer.enabled = false;
                }
            }
        }

        [MenuItem(Data.PACKAGE_NAME + "/Hide/Hide Object(s)", validate = true)]
        static bool HighObjectsCheck()
        {
            return Data.HidingEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/Hide/Unhide Objects(s) _&h", priority = Data.HIDE_OBJECTS_PRIORITY)]
        public static void UnhideObjects()
        {


            while (_HiddenObjects.Count > 0)
            {
                GameObject go = _HiddenObjects.Pop() as GameObject;
                go.renderer.enabled = true;
            }
        }

        [MenuItem(Data.PACKAGE_NAME + "/Hide/Unhide Objects(s) _&h", validate = true)]
        static bool UnhideObjectsCheck()
        {
            return Data.HidingEnabled;
        }

        [MenuItem(Data.PACKAGE_NAME + "/Hide/Hide Others", priority = Data.HIDE_OBJECTS_PRIORITY)]
        public static void HideOthers()
        {
            if (!Selection.activeObject)
            {
                // Nothing selected, don't do anything.
                return;
            }

            Object[] gameobjects = GameObject.FindObjectsOfType(typeof(GameObject));
            Object[] remainVisible = Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep);

            Hashtable objsToHide = new Hashtable();

            foreach (GameObject go in gameobjects)
            {
                objsToHide.Add(go.GetInstanceID(), true);
            }

            foreach (GameObject go in remainVisible)
            {
                if (objsToHide.ContainsKey(go.GetInstanceID()))
                {
                    objsToHide[go.GetInstanceID()] = false;
                }
            }

            foreach (GameObject go in gameobjects)
            {
                if (go.renderer && go.renderer.enabled && (bool)objsToHide[go.GetInstanceID()])
                {
                    _HiddenObjects.Push(go);
                    go.renderer.enabled = false;
                }
            }
        }

        [MenuItem(Data.PACKAGE_NAME + "/Hide/Hide Others", validate = true)]
        static bool HideOthersCheck() 
        {
            return Data.HidingEnabled;
        }

        private static void TempShowObjects()
        {
            SetVisible(true);
            _RehideObjects = true;
        }

        private static void SetVisible(bool isVisible)
        {
            foreach (GameObject g in _HiddenObjects)
            {
                if (g != null)
                {
                    g.renderer.enabled = isVisible;
                }
            }
        }
    }
}