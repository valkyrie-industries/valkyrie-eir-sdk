using TMPro;
using UnityEngine;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Simple output script for calibration and mapped intensity values.
    /// </summary>
    public class DebugOutput : MonoBehaviour {

        #region Serialized Variables

        [SerializeField]
        private TextMeshProUGUI leftCalibration;
        [SerializeField]
        private TextMeshProUGUI rightCalibration;
        [SerializeField]
        private TextMeshProUGUI mappedIntensities;

        #endregion

        #region Unity Methods

        private void Awake() {
            DontDestroyOnLoad(this);
        }

#if EIR_HAPTICS
        private void Update() {
                leftCalibration.SetText($"Left Calibration Lower: {EIRManager.Instance.Haptics.LowerLimits[(int)BodyPart.leftHand]}.Upper: {EIRManager.Instance.Haptics.UpperLimits[(int)BodyPart.leftHand]}.");
                rightCalibration.SetText($"Right Calibration Lower: {EIRManager.Instance.Haptics.LowerLimits[(int)BodyPart.rightHand]}.Upper: {EIRManager.Instance.Haptics.UpperLimits[(int)BodyPart.rightHand]}.");
                mappedIntensities.SetText($"Active Mapped Intensities: L: {EIRManager.Instance.Haptics.MappedIntensities[(int)BodyPart.leftHand]}. R: {EIRManager.Instance.Haptics.MappedIntensities[(int)BodyPart.rightHand]}.");
            
        }
#endif

        #endregion
    }
}