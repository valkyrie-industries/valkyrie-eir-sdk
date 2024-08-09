using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {
    public abstract class EMSDisplay : MonoBehaviour {
        #if EIR_HAPTICS
        HapticManager haptics;
#endif
        //Raw signal levels from 0-1 with 1 being max calibration
        [SerializeField]
        protected float[] signalLevels = { 0, 0 };

        [SerializeField]
        protected BodyPart part;

#if EIR_HAPTICS

        private void Awake() {
            haptics = EIRManager.Instance.Haptics;

            //if (haptics == null)
            //    Destroy(this);
        }

        protected void LateUpdate() {
            if (haptics == null)
                return;
            signalLevels[(int)BodyPart.leftHand] = haptics.indicatorSignal[(int)BodyPart.leftHand];
            signalLevels[(int)BodyPart.rightHand] = haptics.indicatorSignal[(int)BodyPart.rightHand];
            //Debug.Log(signalLevels[(int)part]);
        }

#endif
    }
}


