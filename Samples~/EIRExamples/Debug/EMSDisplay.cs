using UnityEngine;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example visual output for EMS signal levels.
    /// </summary>
    public abstract class EMSDisplay : MonoBehaviour {

        #region Serialized Variables

        // raw signal levels from 0-1 with 1 being max calibration
        [SerializeField]
        protected float[] signalLevels = { 0, 0 };

        [SerializeField]
        protected BodyPart part;

        #endregion

        #region Unity Methods

#if EIR_HAPTICS

        protected void LateUpdate() {

            if (EIRManager.Instance == null || EIRManager.Instance.Haptics == null)  return;

            signalLevels[(int)BodyPart.leftHand] = EIRManager.Instance.Haptics.IndicatorSignal[(int)BodyPart.leftHand];
            signalLevels[(int)BodyPart.rightHand] = EIRManager.Instance.Haptics.IndicatorSignal[(int)BodyPart.rightHand];
        }

#endif

        #endregion
    }
}


