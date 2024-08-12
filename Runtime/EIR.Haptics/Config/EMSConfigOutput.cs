using TMPro;
using UnityEngine;

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
            if (haptic == null) { haptic = EIRManager.Instance.Haptics; }
            else {
                text[0].SetText(haptic.ConfigSignal.Gain.ToString());
                text[1].SetText(haptic.ConfigSignal.Frequency.ToString());
                text[2].SetText(haptic.ConfigSignal.PulseWidth.ToString());
            }
#endif
        }
    }
}