using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Interaction.Interactables;
using Valkyrie.EIR.Haptics;
using Valkyrie.EIR.Interaction;

namespace Valkyrie.EIR.Examples {
    public class SwordInteractable : GrabImpactInteractable
    {
        HapticManager haptics;

        HapticPresetRunner swordRunner;

        [Header("Sword")]
        [SerializeField]
        bool enablePassiveSine;

        public override void Start()
        {
            base.Start();

#if EIR_HAPTICS
            haptics = EIRManager.Instance.Haptics;

            if (haptics == null)
                Debug.LogError("No haptics manager found!");
#endif

            rig.maxAngularVelocity = 20;
        }

        public override void Interacting()
        {
            if (enablePassiveSine)
            {
                //If we are grabbed, begin the passive sine
                if (grabbing && justGrabbed)
                {
                    HapticPreset props = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine, 1, HapticPreset.LoopType.Loop);
                    swordRunner = haptics.CreateHapticPresetRunner(currentlyInteractingBodyPart.BodyPart, props, 0.4f);

                }

                //If we were being grabbed last frame but are not any more, stop the preset
                if (justDropped && swordRunner != null)
                    swordRunner.Stop();
            }

        }


        /*
        private void OnCollisionEnter(Collision collision)
        {
            if (currentlyInteractingBodyPart != null)
            {
                HapticPresetProperties props = HapticPresetRunner.CreateDefaultPreset(HapticPresetRunner.PresetType.maximum,0.2f,false,currentlyInteractingBodyPart.velocity.magnitude / 8);
                haptics.RequestPreset(currentlyInteractingBodyPart.bodyPart, props);
            }

        }
        */

    }
}




