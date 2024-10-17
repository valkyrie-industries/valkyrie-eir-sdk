using TMPro;
using UnityEngine;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Output class to interface between the PlayerSettings class and a UX EIR Bands state.
    /// </summary>
    public class CalibrationOutput : MonoBehaviour {

        #region Serialized Variables

        [SerializeField]
        private bool isLeft;

        #endregion

        #region Private Variables

        private TextMeshProUGUI text;
        private int prevValue = 0;

        #endregion

        #region Unity Methods

        private void Update() {
#if EIR_HAPTICS
            if (text == null) {
                text = GetComponent<TextMeshProUGUI>();
            }
            // static values, we can revisit this later.
            int value = isLeft ? EIRManager.Instance.Haptics.CalibrationIndex[1] : EIRManager.Instance.Haptics.CalibrationIndex[0];
            if (value != prevValue) {
                prevValue = value;
                text.SetText((value + 1).ToString());
            }
#endif
        }

        #endregion

    }
}