using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAdditiveScene : MonoBehaviour
{
    public string sceneNameToAdditively = "YourSceneName";

    private void Start()
    {
        LoadScene();
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneNameToAdditively, LoadSceneMode.Additive);
    }
}
