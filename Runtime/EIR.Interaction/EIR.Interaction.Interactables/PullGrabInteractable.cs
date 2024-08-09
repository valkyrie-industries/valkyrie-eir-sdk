using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction.Interactables
{
    /// <summary>
    /// Resistance Grab Interactable: checks how much the resistance band is stretched and with how much force.
    /// Every frame it sends this force to the InteractionManager, which then exerts EMS on the required hand.
    /// </summary>
    public class PullGrabInteractable : GrabInteractable
    {
        public float minExtension = 0.1f;
        public float maxExtension = 1.0f;
        public Transform attachmentPoint;

        public override void Update()
        {
            base.Update();
        }
        public override void Interacting()
        {
            if (grabbing)
            {
                if (attachmentPoint != null)
                {
                    // Calculate force of resistance:
                    float elasticForce = ValkyrieEIRExtensionMethods.Map((attachmentPoint.position - transform.position).magnitude, minExtension, maxExtension);
                    InvokeOnForce(currentlyInteractingBodyPart.BodyPart, elasticForce);
                }
            }
        }
    }
}