﻿using UnityEngine;
using Valkyrie.EIR.Haptics;

namespace Valkyrie.EIR.Examples {
    public abstract class EMSDisplay : MonoBehaviour {
        HapticManager haptics;
        //Raw signal levels from 0-1 with 1 being max calibration
        [SerializeField]
        protected float[] signalLevels = { 0, 0 };

        [SerializeField]
        protected BodyPart part;

#if EIR_HAPTICS

        private void Start() {
            haptics = EIRManager.Instance.Haptics;

            //if (haptics == null)
            //    Destroy(this);
        }

        protected void LateUpdate() {
            if (haptics == null)
                return;
            signalLevels[(int)BodyPart.leftHand] = haptics.IndicatorSignal[(int)BodyPart.leftHand];
            signalLevels[(int)BodyPart.rightHand] = haptics.IndicatorSignal[(int)BodyPart.rightHand];
            //Debug.Log(signalLevels[(int)part]);
        }

#endif
    }
}

