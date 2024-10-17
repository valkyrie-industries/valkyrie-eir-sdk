using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Valkyrie.EIR.Interaction.Interactables {

    /// <summary>
    /// Checks if it is grabbed by the hand (the "currently interacting body part")
    /// </summary>  
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ClimbGrabInteractable : GrabInteractable {

        #region Serialized Variables

        [SerializeField]
        private float multiplier = 100f;

        #endregion

        #region Private Variables

        private Vector3 justGrabbedPosition;
        private Vector3 previouslyGrabbedPosition = Vector3.zero;
        private XROrigin xrOrigin;

        #endregion

        #region Unity Methods

        protected override void Start() {
            xrOrigin = FindObjectOfType<XROrigin>();
            base.Start();

        }

        protected override void Update() {
            base.Update();
            if (justGrabbed)
                justGrabbedPosition = xrOrigin.transform.position;
        }

        #endregion

        #region Private Methods

        protected override void Interacting() {
            if (grabbing) {

                // calculate force of the object
                Vector3 force = Vector3.zero;

                if (xrOrigin != null && !justGrabbed) {
                    force = (xrOrigin.transform.position - justGrabbedPosition) * multiplier; // need to make it a xrOrigin, because the hand is on the same place

                    Vector3 displacement = (currentlyInteractingBodyPart.transform.position - previouslyGrabbedPosition);

                    xrOrigin.transform.position -= displacement;
                }

                previouslyGrabbedPosition = currentlyInteractingBodyPart.transform.position;
                InvokeOnForce(currentlyInteractingBodyPart.BodyPart, force.magnitude);
            }
        }

        #endregion
    }
}