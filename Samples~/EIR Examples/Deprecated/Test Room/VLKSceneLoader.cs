using UnityEngine;
using UnityEngine.SceneManagement;

public class VLKSceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if(SceneManager.GetSceneByName(sceneName) != null)
            SceneManager.LoadScene(sceneName);
    }
}
