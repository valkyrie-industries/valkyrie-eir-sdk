using TMPro;
using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {

    public class DebugOutput : MonoBehaviour {
#if EIR_HAPTICS
        private HapticManager haptics;
#endif

        [SerializeField]
        private TextMeshProUGUI leftCalibration;
        [SerializeField]
        private TextMeshProUGUI rightCalibration;
        [SerializeField]
        private TextMeshProUGUI mappedIntensities;

        private void Awake() {
            DontDestroyOnLoad(this);
        }

#if EIR_HAPTICS
        private void Update() {
            if (haptics == null) haptics = EIRManager.Instance.Haptics;
            else {
                leftCalibration.SetText($"Left Calibration Lower: {haptics.LowerLimits[(int)BodyPart.leftHand]}.Upper: {haptics.UpperLimits[(int)BodyPart.leftHand]}.");
                rightCalibration.SetText($"Right Calibration Lower: {haptics.LowerLimits[(int)BodyPart.rightHand]}.Upper: {haptics.UpperLimits[(int)BodyPart.rightHand]}.");
                mappedIntensities.SetText($"Active Mapped Intensities: L: {haptics.MappedIntensities[(int)BodyPart.leftHand]}. R: {haptics.MappedIntensities[(int)BodyPart.rightHand]}.");
            }
        }
#endif
    }
}