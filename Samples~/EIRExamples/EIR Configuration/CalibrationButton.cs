using UnityEngine;
using TMPro;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example calibration button.
    /// </summary>
    public class CalibrationButton : MonoBehaviour {

        #region Serialized Variables

        [SerializeField]
        private bool isLeft;
        [SerializeField]
        private GameObject label;

        #endregion

        #region Private Variables

        private int handIndex;
        private TextMeshProUGUI text;
        private int lastValue = 0;

        #endregion

        #region Unity Methods

        private void OnEnable() {
            handIndex = isLeft ? 1 : 0;
#if EIR_HAPTICS
            if (label != null && text == null) text = label.GetComponent<TextMeshProUGUI>();
            if (text != null && EIRManager.Instance != null) {
                if (EIRManager.Instance.Haptics != null && EIRManager.Instance.Haptics.CalibrationIndex != null) {
                    Debug.Log($"[Calibration Button] Initialising with value {EIRManager.Instance.Haptics.CalibrationIndex[handIndex]}");
                    lastValue = EIRManager.Instance.Haptics.CalibrationIndex[handIndex];
                    text.SetText((EIRManager.Instance.Haptics.CalibrationIndex[handIndex] + 1).ToString());
                }
                else {
                    Debug.Log($"[Calibration Button] Initialising with last recorded value {lastValue}");
                    text.SetText((lastValue + 1).ToString());
                }
            }
#endif
        }

        #endregion

        #region Public Variables

        /// <summary>
        /// Increments or decrements the calibration for the set hand dependent on whether up is true or false.
        /// </summary>
        /// <param name="up"></param>
        public void UpdateCalibrationIndex(bool up) {
#if EIR_HAPTICS

            int currentIndex = EIRManager.Instance.Haptics.CalibrationIndex[handIndex];
            if (up) {
                if (currentIndex < HapticManager.CALIBRATION_INDEX_LENGTH)
                    currentIndex += 1;
            } else {
                if (currentIndex > 0)
                    currentIndex -= 1;
            }
            EIRManager.Instance.Haptics.CalibrationIndex[handIndex] = currentIndex;
            lastValue = currentIndex;

            if (EIRManager.Instance.Haptics == null) {
                Debug.LogError("[Calibration Button] No HapticManager instance found.");
                return;
            }

            EIRManager.Instance.Haptics.ModifyCalibrationByIndex(isLeft, currentIndex);
            label.GetComponent<TextMeshProUGUI>().text = (currentIndex + 1).ToString(); // to count from 1 to 11, instead of 0 to 10
#endif
        }

        #endregion
    }
}