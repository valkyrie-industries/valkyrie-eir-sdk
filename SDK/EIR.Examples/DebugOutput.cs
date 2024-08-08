using TMPro;
using UnityEngine;
using Valkyrie.EIR.Haptics;


namespace Valkyrie.EIR.Examples {

    public class DebugOutput : MonoBehaviour {

        private HapticManager haptics;

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

            leftCalibration.SetText($"Left Calibration Lower: {HapticManager.LowerLimits[(int)BodyPart.leftHand]}.Upper: {HapticManager.UpperLimits[(int)BodyPart.leftHand]}.");
            rightCalibration.SetText($"Right Calibration Lower: {HapticManager.LowerLimits[(int)BodyPart.rightHand]}.Upper: {HapticManager.UpperLimits[(int)BodyPart.rightHand]}.");
            mappedIntensities.SetText($"Active Mapped Intensities: L: {haptics.MappedIntensities[(int)BodyPart.leftHand]}. R: {haptics.MappedIntensities[(int)BodyPart.rightHand]}.");

        }
#endif
    }
}