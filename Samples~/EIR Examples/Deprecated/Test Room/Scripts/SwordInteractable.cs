using UnityEngine;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction.Interactables;
#endif
using Valkyrie.EIR.Haptics;


namespace Valkyrie.EIR.Examples {

#if EIR_INTERACTION

    public class SwordInteractable : GrabImpactInteractable {
#else
    public class SwordInteractable : MonoBehaviour {

#endif
        HapticManager haptics;

        HapticPresetRunner swordRunner;

        [Header("Sword")]
        [SerializeField]
        bool enablePassiveSine;

#if EIR_INTERACTION
        public override void Start() {
            base.Start();

#if EIR_HAPTICS
            haptics = EIRManager.Instance.Haptics;

            if (haptics == null)
                Debug.LogError("No haptics manager found!");
#endif
            rig.maxAngularVelocity = 20;
        }
#endif

#if EIR_INTERACTION
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
#endif


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




