using TMPro;
using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Output class to interface between the HapticManager and the Quick Menu's EMS state.
    /// </summary>
    public class EMSConfigOutput : MonoBehaviour {

        #region Serialized Variables

        [SerializeField]
        private TextMeshProUGUI[] text;

        #endregion

        #region Private Variables

#if EIR_HAPTICS
        private HapticManager haptic;
#endif

        #endregion

        #region Unity Methods

        private void Update() {
#if EIR_HAPTICS
            if (!gameObject.activeInHierarchy) return;
            if (haptic == null) { haptic = EIRManager.Instance.Haptics; }
            else {
                text[0].SetText(haptic.ConfigSignal.Gain.ToString());
                text[1].SetText(haptic.ConfigSignal.Frequency.ToString());
                text[2].SetText(haptic.ConfigSignal.PulseWidth.ToString());
            }
#endif
        }

        #endregion
    }
}