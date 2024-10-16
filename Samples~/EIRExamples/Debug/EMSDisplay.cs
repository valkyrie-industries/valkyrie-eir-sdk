using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {
    public abstract class EMSDisplay : MonoBehaviour {

#if EIR_HAPTICS
        HapticManager haptics;
#endif
        // raw signal levels from 0-1 with 1 being max calibration
        [SerializeField]
        protected float[] signalLevels = { 0, 0 };

        [SerializeField]
        protected BodyPart part;

#if EIR_HAPTICS

        protected void LateUpdate() {
            if (haptics == null) {
                haptics = EIRManager.Instance.Haptics;

                if (haptics == null)
                    return;
            }

            signalLevels[(int)BodyPart.leftHand] = haptics.IndicatorSignal[(int)BodyPart.leftHand];
            signalLevels[(int)BodyPart.rightHand] = haptics.IndicatorSignal[(int)BodyPart.rightHand];
        }

#endif
    }
}


