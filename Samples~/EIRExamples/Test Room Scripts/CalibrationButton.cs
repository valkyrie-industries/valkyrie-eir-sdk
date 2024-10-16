using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {
    public class CalibrationButton : MonoBehaviour {

        public bool isLeft;

        private int handIndex;
        [SerializeField]
        private GameObject label;

        private void Start() {
            handIndex = isLeft ? 1 : 0;
#if EIR_HAPTICS
            if (label != null)
                label.GetComponent<TextMeshProUGUI>().text = (EIRManager.Instance.Haptics.CalibrationIndex[handIndex] + 1).ToString();
#endif
        }

        public void UpdateCalibrationIndex(bool up) {
#if EIR_HAPTICS

            int currentIndex = EIRManager.Instance.Haptics.CalibrationIndex[handIndex];
            if (up) {
                if (currentIndex < HapticManager.CALIBRATION_INDEX_LENGTH)
                    currentIndex += 1;
            }
            else {
                if (currentIndex > 0)
                    currentIndex -= 1;
            }
            EIRManager.Instance.Haptics.CalibrationIndex[handIndex] = currentIndex;

            if (EIRManager.Instance.Haptics == null) {
                Debug.LogError("[Calibration Button] No HapticManager instance found.");
                return;
            }

            EIRManager.Instance.Haptics.ModifyCalibrationByIndex(isLeft, currentIndex);
            label.GetComponent<TextMeshProUGUI>().text = (currentIndex + 1).ToString(); // to count from 1 to 11, instead of 0 to 10
			#endif
        }
    }
}