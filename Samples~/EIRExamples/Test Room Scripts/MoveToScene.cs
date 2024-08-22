
using UnityEngine.SceneManagement;
using UnityEngine;

public class MoveToScene : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName) != null)
            SceneManager.LoadScene(sceneName);
    }
}
