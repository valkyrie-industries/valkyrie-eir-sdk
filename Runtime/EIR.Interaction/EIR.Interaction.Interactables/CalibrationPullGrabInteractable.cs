using UnityEngine;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction.Interactables {

    /// <summary>
    /// Calibration Resistance Grab Interactable: checks how much it is stretched and with which force.
    /// Every frame it sends this force to the InteractionManager, which then exerts EMS on the required hand.
    /// </summary>
    public class CalibrationPullGrabInteractable : PullGrabInteractable {

        #region Serialized Variables

        [SerializeField]
        private float minValue, maxValue;

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the calibration limits (minimum and maximum values).
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void InitialiseCalibrationLimits(float min, float max) {
            minValue = min;
            maxValue = max;
        }

        /// <summary>
        /// Overrides send zero force, but ignores calibration.
        /// </summary>
        public override void SendZeroForce() {
            if (currentlyInteractingBodyPart != null)
                InvokeOnForce(currentlyInteractingBodyPart.BodyPart, 0, true);
        }

        #endregion

        #region Private Methods

        protected override void Interacting() {
            if (grabbing) {
                // calculate force of resistance within a range of 0 and 1
                float elasticForce = ValkyrieEIRExtensionMethods.Map((attachmentPoint.position - transform.position).magnitude, minExtension, maxExtension);
                // convert 0/1 range to the EMS signal
                float convertedEMSSignal = Mathf.Lerp(minValue, maxValue, elasticForce);
                // send uncalibrated values & bypass calibration
                InvokeOnForce(currentlyInteractingBodyPart.BodyPart, convertedEMSSignal, true);
            }
        }

        #endregion
    }
}