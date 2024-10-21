using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valkyrie.EIR.Examples.Utilities {

    /// <summary>
    /// Quick scene loader.
    /// </summary>
    public class MoveToScene : MonoBehaviour {

        /// <summary>
        /// Loads the scene (non-additively) by name.
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadScene(string sceneName) {
            if (SceneManager.GetSceneByName(sceneName) != null)
                SceneManager.LoadScene(sceneName);
        }
    }
}