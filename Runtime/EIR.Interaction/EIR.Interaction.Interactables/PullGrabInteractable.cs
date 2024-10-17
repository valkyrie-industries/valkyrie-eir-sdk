using UnityEngine;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction.Interactables {

    /// <summary>
    /// Resistance Grab Interactable: checks how much the resistance band is stretched and with how much force.
    /// Every frame it sends this force to the InteractionManager, which then exerts EMS on the required hand.
    /// </summary>
    public class PullGrabInteractable : GrabInteractable {

        #region Serialized Variables

        [SerializeField]
        protected float minExtension = 0.1f;
        [SerializeField]
        protected float maxExtension = 1.0f;
        [SerializeField]
        protected Transform attachmentPoint;

        #endregion

        #region Private Methods

        protected override void Interacting() {
            if (grabbing) {
                if (attachmentPoint != null) {
                    // Calculate force of resistance:
                    float elasticForce = ValkyrieEIRExtensionMethods.Map((attachmentPoint.position - transform.position).magnitude, minExtension, maxExtension);
                    InvokeOnForce(currentlyInteractingBodyPart.BodyPart, elasticForce);
                }
            }
        }

        #endregion
    }
}