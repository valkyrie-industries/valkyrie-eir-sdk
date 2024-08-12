using TMPro;
using UnityEngine;

namespace Valkyrie.EIR.Haptics {

    /// <summary>
    /// Output class to interface between the PlayerSettings class and the Quick Menu's EIR Bands state.
    /// </summary>
    public class CalibrationOutput : MonoBehaviour {

        [SerializeField]
        private bool isLeft;

        private TextMeshProUGUI text;
        private int prevValue = 0;

        private void Update() {
            if (text == null) {
                text = GetComponent<TextMeshProUGUI>();
            }
            // static values, we can revisit this later.
            int value = isLeft ? EIRManager.Instance.Haptics.CalibrationIndex[1] : EIRManager.Instance.Haptics.CalibrationIndex[0];
            if (value != prevValue) {
                prevValue = value;
                text.SetText((value + 1).ToString());
            }
        }
    }
}