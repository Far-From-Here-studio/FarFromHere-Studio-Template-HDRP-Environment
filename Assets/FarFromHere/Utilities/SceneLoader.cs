using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SceneLoader : MonoBehaviour
{
    public bool LoadAtStart;
    public int ListIndex = 0;

    public List<SceneList> sceneLists;

    [System.Serializable]
    public class SceneList
    {
        public string listName;
        public List<UnityEngine.Object> scenes;
        public List<string> scenesName;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
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
#endif

    private void Start()
    {
       if(LoadAtStart) LoadScene(sceneLists[ListIndex].scenesName[ListIndex]);
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
