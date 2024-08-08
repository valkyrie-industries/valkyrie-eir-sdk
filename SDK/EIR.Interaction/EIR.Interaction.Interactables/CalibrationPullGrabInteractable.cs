using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction.Interactables
{
    /// <summary>
    /// Calibration Resistance Grab Interactable: checks how much it is stretched and with which force.
    /// Every frame it sends this force to the InteractionManager, which then exerts EMS on the required hand.
    /// </summary>
    public class CalibrationPullGrabInteractable : PullGrabInteractable
    {
        public float minValue, maxValue;

        public void InitialiseCalibrationLimits(float min, float max)
        {
            minValue = min;
            maxValue = max;
        }

        public override void Interacting()
        {
            if (grabbing)
            {
                // Calculate force of resistance:
                // Range between 0 and 1
                float elasticForce = ValkyrieEIRExtensionMethods.Map((attachmentPoint.position - transform.position).magnitude, minExtension, maxExtension);
                // Convert 0/1 range to the EMS signal
                float convertedEMSSignal = Mathf.Lerp(minValue, maxValue, elasticForce);
                // Send uncalibrated values & bypass calibration
                InvokeOnForce(currentlyInteractingBodyPart.BodyPart, convertedEMSSignal, true);
            }
        }

        public override void SendZeroForce()
        {
            if (currentlyInteractingBodyPart != null)
                InvokeOnForce(currentlyInteractingBodyPart.BodyPart, 0, true);
        }
    }
}