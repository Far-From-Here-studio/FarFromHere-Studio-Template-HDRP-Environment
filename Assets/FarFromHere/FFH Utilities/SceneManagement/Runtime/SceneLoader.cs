using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace FFH.Utilities.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        public bool LoadAtStart;
        public int ListIndexToLoadAtStart = 0;


        public List<SceneList> sceneLists;

        [System.Serializable]
        public class SceneList
        {
            public string listName;
            public List<UnityEngine.Object> scenes;
            [HideInInspector]
            public List<string> scenesName;
        }

        // Store the last known state of the scenes in the script
        private List<string> previousScenePaths = new List<string>();

        public void LoadScene(string sceneName)
        {
            if (!IsSceneLoaded(sceneName))
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            else
            {
                Debug.LogWarning("Scene " + sceneName + " is already loaded.");
            }
        }

        private bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        [ContextMenu("Populate Scene List Names")]
        public void PopulateSceneListNames()
        {
            foreach (SceneList sceneList in sceneLists)
            {
                List<string> sceneNames = new List<string>();

                foreach (UnityEngine.Object scene in sceneList.scenes)
                {
                    sceneNames.Add(scene.name);
                }

                sceneList.scenesName = sceneNames;
            }

            EditorUtility.SetDirty(this);
        }
        public void SetEditorBuildSettingsScenes()
        {
            // Retrieve existing build settings scenes
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Create a hash set for efficient lookup of the scenes that should be in the build from the script's lists
            HashSet<string> targetScenePaths = new HashSet<string>();

            // Populate the hash set with scenes from the script's scene lists
            foreach (SceneList sceneList in sceneLists)
            {
                foreach (UnityEngine.Object sceneAsset in sceneList.scenes)
                {
                    string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                    if (!string.IsNullOrEmpty(scenePath))
                    {
                        targetScenePaths.Add(scenePath);
                    }
                }
            }

            // Remove scenes from the build settings that were removed from the script's scene lists (compared to previous state)
            for (int i = editorBuildSettingsScenes.Count - 1; i >= 0; i--)
            {
                string buildScenePath = editorBuildSettingsScenes[i].path;
                if (previousScenePaths.Contains(buildScenePath) && !targetScenePaths.Contains(buildScenePath))
                {
                    // Remove scene from build settings if it was previously in the script's list but now it's not
                    editorBuildSettingsScenes.RemoveAt(i);
                }
            }

            // Add new scenes that are in the scene lists but not already in the build settings
            foreach (string scenePath in targetScenePaths)
            {
                bool sceneAlreadyInBuild = editorBuildSettingsScenes.Exists(buildScene => buildScene.path == scenePath);
                if (!sceneAlreadyInBuild)
                {
                    EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(scenePath, true);
                    editorBuildSettingsScenes.Add(newScene);
                }
            }

            // Apply the updated scene list to the build settings
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();

            // Update the previousScenePaths list to reflect the current state of the script's scene lists
            previousScenePaths = new List<string>(targetScenePaths);
        }

        // Helper function to check if a scene was added from the script lists
        private bool IsSceneInLists(string scenePath)
        {
            foreach (SceneList sceneList in sceneLists)
            {
                foreach (UnityEngine.Object sceneAsset in sceneList.scenes)
                {
                    if (AssetDatabase.GetAssetPath(sceneAsset) == scenePath)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnValidate()
        {
            PopulateSceneListNames();
            SetEditorBuildSettingsScenes();
        }
#endif

        private void OnEnable()
        {
            if (LoadAtStart) LoadScene(sceneLists[ListIndexToLoadAtStart].scenesName[ListIndexToLoadAtStart]);
        }

        public void UnloadScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        public void LoadAllScenesInList(string listName)
        {
            SceneList sceneList = sceneLists.Find(list => list.listName == listName);
            if (sceneList != null)
            {
#if UNITY_EDITOR
                foreach (UnityEngine.Object scene in sceneList.scenes)
                {

                    if (!EditorApplication.isPlaying)
                    {
                        LoadSceneEditor(scene);
                    }
                    else
                    {
                        LoadScene(scene.name);
                    }
                }
#else
            foreach (string sceneName in sceneList.scenesName)
            {
                LoadScene(sceneName);
            }
#endif
            }
        }

        public void UnloadAllScenesInList(string listName)
        {
            SceneList sceneList = sceneLists.Find(list => list.listName == listName);
            if (sceneList != null)
            {
#if UNITY_EDITOR
                foreach (UnityEngine.Object scene in sceneList.scenes)
                {

                    if (!EditorApplication.isPlaying)
                    { UnloadSceneEditor(scene); }
                    else { UnloadScene(scene.name); }
                }
#else
            foreach (string sceneName in sceneList.scenesName)
            {
                UnloadScene(sceneName);
            }
#endif


            }
        }

#if UNITY_EDITOR
        public void UnloadSceneEditor(UnityEngine.Object sceneAsset)
        {
            string scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneName);

            EditorSceneManager.CloseScene(scene, true);
        }
        public void LoadSceneEditor(UnityEngine.Object sceneAsset)
        {
            string scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }
#endif

    }
}