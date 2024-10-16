using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {
    public abstract class EMSDisplay : MonoBehaviour {
#if EIR_HAPTICS
        HapticManager haptics;

        [SerializeField]
        protected DeviceRole role;

#endif
        //Intensity levels from 0-1. 0 represents the user's lower limit and 1 represents the user's upper limit
        [SerializeField]
        protected float[] signalLevels = { 0, 0 };



#if EIR_HAPTICS

        protected void LateUpdate() {

            if (haptics == null)
            {
                haptics = EIRManager.Instance.Haptics;

                if (haptics == null)
                    return;
            }

            signalLevels[(int)DeviceRole.A] = haptics.IndicatorSignal[(int)DeviceRole.A];
            signalLevels[(int)DeviceRole.B] = haptics.IndicatorSignal[(int)DeviceRole.B];
        }

#endif
    }
}


