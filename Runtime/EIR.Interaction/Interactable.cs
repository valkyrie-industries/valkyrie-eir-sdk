using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Interaction {

    /// <summary>
    /// Valkyrie Interactable: interacts with the body part and calculates the intensity of interaction.
    /// Every frame performs the public virtual function Interacting() which can be overriden to invoke interaction-specific functionality.
    /// </summary>
    public class Interactable : MonoBehaviour {

        #region Delegates

        public delegate void OnInteractableEventHandler(Interactable i);
        public delegate void OnForceEventHandler(BodyPart part, float force, bool ignoreCalibration = false);

#if EIR_HAPTICS
        public delegate void OnHapticPresetEventHandler(BodyPart bodyPart, HapticPreset preset);
        public delegate void OnHapticPresetTypeEventHandler(BodyPart bodyPart, HapticPreset.PresetType presetType);
#endif

        #endregion

        #region Events

        public static event OnForceEventHandler OnForce;

#if EIR_HAPTICS
        public static event OnHapticPresetEventHandler OnHapticPresetRequested;
        public static event OnHapticPresetTypeEventHandler OnHapticPresetTypeRequested;
#endif

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the currently interacting body part.
        /// </summary>
        public InteractingBodyPart currentlyInteractingBodyPart {
            get => m_currentlyInteractingBodyPart;
            set => m_currentlyInteractingBodyPart = value;
        }

        #endregion

        #region Serialized Variables

        [SerializeField]
        private InteractingBodyPart m_currentlyInteractingBodyPart;

        #endregion

        #region Unity Methods

        protected virtual void Update() {
            Interacting();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Runs each frame, locked to the Update loop.
        /// </summary>
        protected virtual void Interacting() {
            // empty in base, define here in overrides what happens when interactable is interacting with the interacting body part.
        }

#if EIR_HAPTICS

        /// <summary>
        /// Asks the HapticManager to create a HapticPresetRunner with the input HapticPreset.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="hapticPreset"></param>
        protected void InvokeHapticPresetRequest(BodyPart part, HapticPreset hapticPreset) {
            OnHapticPresetRequested?.Invoke(part, hapticPreset);
        }

        /// <summary>
        /// Asks the HapticManager to create a HapticPresetRunner with the input HapticPreset PresetType.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="presetType"></param>
        protected void InvokeHapticPresetTypeRequest(BodyPart part, HapticPreset.PresetType presetType) {
            OnHapticPresetTypeRequested?.Invoke(part, presetType);
        }
#endif

        /// <summary>
        /// Invokes force on the input body part. HapticManager will generate a HapticPresetRunner with the given force values.
        /// Optionally, ignore calibration for a weaker or stronger pulse.
        /// Important! Stronger pulses should only be used for calibration purposes, and should be avoided in other circumstances.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="force"></param>
        /// <param name="ignoreCalibration"></param>
        protected void InvokeOnForce(BodyPart bodyPart, float force, bool ignoreCalibration = false) {
            OnForce?.Invoke(bodyPart, force, ignoreCalibration);
        }

        #endregion
    }
}