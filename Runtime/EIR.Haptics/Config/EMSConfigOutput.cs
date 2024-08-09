using TMPro;
using UnityEngine;
using Valkyrie.EIR;

namespace Valkyrie.EIR.Haptics {

    /// <summary>
    /// Output class to interface between the HapticManager and the Quick Menu's EMS state.
    /// </summary>
    public class EMSConfigOutput : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI[] text;

        private HapticManager haptic;

        private void Update() {
#if EIR_HAPTICS
            if (!gameObject.activeInHierarchy) return;
            if (haptic == null) {  }
            else {
                text[0].SetText(HapticManager.ConfigSignal.Gain.ToString());
                text[1].SetText(HapticManager.ConfigSignal.Frequency.ToString());
                text[2].SetText(HapticManager.ConfigSignal.PulseWidth.ToString());
            }
#endif
        }
    }
}