using UnityEngine;
using UnityEngine.UI;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Button that communicates with FeelManager to play feelings
    /// </summary>
    public class FeelButton : MonoBehaviour {

        #region Serialized Variables

        [SerializeField]
        private Button button;

        #endregion

        #region Unity Methods

        private void Start() {
#if EIR_HAPTICS
            if (FeelManager.Instance != null) FeelManager.Instance.OnFeelingStatusChange += OnFeelingStatusEvent;
#endif
        }
#if EIR_HAPTICS
        private void OnDestroy() {
            if (FeelManager.Instance != null && FeelManager.Instance.OnFeelingStatusChange != null) FeelManager.Instance.OnFeelingStatusChange -= OnFeelingStatusEvent;
        }
#endif
        #endregion

        #region Public Methods

        public void PlayFeeling(string feelID) {
#if EIR_HAPTICS
            if (FeelManager.Instance != null) FeelManager.Instance.PlayFeeling(feelID);
            else Debug.Log($"[Feel Button] No FeelManager present in scene.");
#endif
        }
        #endregion

        #region Private Methods

        private void OnFeelingStatusEvent(bool status) {
            button.interactable = !status;
        }

        #endregion
    }
}

