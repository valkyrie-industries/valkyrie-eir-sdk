#if EIR_HAPTICS
using UnityEngine;
using Valkyrie.EIR.Haptics;
using System.Linq;
#endif
using Valkyrie.EIR.Interaction;

namespace Valkyrie.EIR.Interaction.Interactables
{
    /// <summary>
    /// TouchInteractable that sends haptic preset at the touch of a bodypart.
    /// </summary>
    public class HapticPresetTouchInteractable : TouchInteractable {
#if EIR_HAPTICS


        [SerializeField]
        HapticPreset.PresetType presetType;

        public BodyPart[] targetParts;

        // Update is called once per frame
        public override void TouchInteraction(Vector3 velocity) {
            HapticPreset hapticPresetProperties = HapticPreset.CreateDefaultPreset(presetType, interactionDuration, HapticPreset.LoopType.None);
            InvokeHapticPresetRequest(currentlyInteractingBodyPart.BodyPart, hapticPresetProperties);
        }

        public override void TouchInteraction(Vector3 velocity, BodyPart part) {
            if (targetParts.Any(chosenPart => chosenPart == part)) {
                HapticPreset hapticPresetProperties = HapticPreset.CreateDefaultPreset(presetType, interactionDuration, HapticPreset.LoopType.None);
                InvokeHapticPresetRequest(part, hapticPresetProperties);
                Debug.Log($"[Haptic Preset Touch Interactable] Starting Haptic Preset Runner for body part: {part}");
            }
        }
#else

        private void Start() {
            throw new System.NotImplementedException($"[Valkyrie.EIR SDK] HapticPresetTouchInteractable component {gameObject.name} is on dependent on EIR Haptics. If Haptics is not required, please use TouchInteractable instead");
        }
#endif
    }
}