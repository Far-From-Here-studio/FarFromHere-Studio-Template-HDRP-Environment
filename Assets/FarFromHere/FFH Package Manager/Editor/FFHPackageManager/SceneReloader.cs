using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace FFH.PackageManager
{
    public static class SceneReloader
    {
        public static void ReloadOpenScenes()
        {
            // Save current active scene index
            var activeScene = EditorSceneManager.GetActiveScene();

            // Get all open scenes
            int sceneCount = EditorSceneManager.sceneCount;
            List<string> scenePaths = new List<string>();
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    scenePaths.Add(scene.path);
                }
            }

            // Clear out the old scenes
            var newscene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Reload the scenes
            foreach (string scenePath in scenePaths)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            EditorSceneManager.CloseScene(newscene, true);
            //EditorSceneManager.SetActiveScene(activeScene);

            // Refresh the AssetDatabase and the editor
            AssetDatabase.Refresh();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
