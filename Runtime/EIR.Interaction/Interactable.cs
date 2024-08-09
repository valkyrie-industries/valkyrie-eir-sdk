using System.Collections;
using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Interaction
{
    /// <summary>
    /// Valkyrie Interactable: interacts with the body part and calculates the intensity of interaction
    /// Every frame performs the public virtual function Interacting()
    /// </summary>
    public class Interactable : MonoBehaviour {

        public delegate void OnInteractableEventHandler(Interactable i);
        public delegate void OnForceEventHandler(BodyPart part, float force, bool ignoreCalibration = false);

        public delegate void OnHapticPresetEventHandler(BodyPart bodyPart, HapticPreset preset);
        public static event OnHapticPresetEventHandler OnHapticPresetRequested;
        public delegate void OnHapticPresetTypeEventHandler(BodyPart bodyPart, HapticPreset.PresetType presetType);
        public static event OnHapticPresetTypeEventHandler OnHapticPresetTypeRequested;

        public static event OnForceEventHandler OnForce;


        public InteractingBodyPart currentlyInteractingBodyPart {
            get => m_currentlyInteractingBodyPart;
            set => m_currentlyInteractingBodyPart = value;
        }
        [SerializeField]
        InteractingBodyPart m_currentlyInteractingBodyPart;

        public virtual void Update() {
            Interacting();
        }

        protected void InvokeOnForce(BodyPart bodyPart, float force, bool ignoreCalibration = false) {
            OnForce?.Invoke(bodyPart, force, ignoreCalibration);
        }

        public virtual void Interacting() {
            // empty in base, define here in overrides what happens when interactable is interacting with the interacting body part
        }

#if EIR_HAPTICS
        protected void InvokeHapticPresetRequest(BodyPart part, HapticPreset hapticPreset) {
            OnHapticPresetRequested?.Invoke(part, hapticPreset);
        }

        protected void InvokeHapticPresetTypeRequest(BodyPart part, HapticPreset.PresetType presetType) {
            OnHapticPresetTypeRequested?.Invoke(part, presetType);
        }
#endif
    }
}