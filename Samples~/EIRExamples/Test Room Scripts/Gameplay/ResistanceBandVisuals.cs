﻿using UnityEngine;
using TMPro;
using Valkyrie.EIR.Utilities;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction.Interactables;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Resistance band visuals. 
    /// Controls the colour and visuals of the band during interaction
    /// </summary>
    public class ResistanceBandVisuals : MonoBehaviour {

        #region Serialized Variables

#if EIR_INTERACTION
        [SerializeField]
        public CalibrationPullGrabInteractable interactable;
#endif

        [SerializeField]
        private LineRenderer line;
        [SerializeField]
        private Transform interactableAttachmentPoint;
        [SerializeField]
        private Transform machineAttachmentPoint;
        [SerializeField]
        private ParticleSystem particles;
        [SerializeField]
        private TMP_Text levelLabel;

        #endregion

        #region Public Methods

        /// <summary>
        /// Modifies the size and colour of the band from the calibration value.
        /// </summary>
        /// <param name="calibrationLevel"></param>
        public void InitialiseVisuals(int calibrationLevel) {
#if EIR_INTERACTION
            // Change colour depending on the calibrationRange:
            Color bandColor = ValkyrieEIRExtensionMethods.ColorBasedOnCalibrationLevel(calibrationLevel);
            line.GetComponent<Renderer>().material.color = bandColor;
            var handles = interactable.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mesh in handles)
                mesh.material.SetColor("_BaseColor", bandColor);

            // Change size depending on the calibrationRange:
            interactable.transform.localScale = ValkyrieEIRExtensionMethods.SizeBasedOnCalibrationLevel(calibrationLevel, 0.8f, 1.2f);

            if (calibrationLevel < 3) {
                particles.gameObject.SetActive(false);
            } else {
                particles.gameObject.SetActive(true);
                ParticleSystem.MainModule main = particles.main;
                main.maxParticles = (int)ValkyrieEIRExtensionMethods.MapToExpRange(calibrationLevel);
            }

            if (levelLabel != null)
                levelLabel.text = (calibrationLevel + 1).ToString();
#endif
        }

        #endregion

        #region Private Methods

        private void Update() {
            DrawResistanceBand();
        }

        private void DrawResistanceBand() {
            line.SetPosition(0, machineAttachmentPoint.position);
            line.SetPosition(1, interactableAttachmentPoint.position);
        }

        #endregion
    }
}