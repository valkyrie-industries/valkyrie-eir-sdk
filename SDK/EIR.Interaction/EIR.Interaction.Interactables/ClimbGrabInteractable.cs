using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using Valkyrie.EIR.Interaction;

namespace Valkyrie.EIR.Interaction.Interactables
{
    /// <summary>
    /// Checks if it is grabbed by the hand (the "currently interacting body part")
    /// Public Bools: grabbing, justGrabbed, justDropped
    /// </summary>  
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ClimbGrabInteractable : GrabInteractable
    {

        Vector3 justGrabbedPosition;
        Vector3 previouslyGrabbedPosition = Vector3.zero;

        [SerializeField]
        float multiplier = 100f;

        XROrigin xrOrigin;

        public override void Start()
        {
            xrOrigin = FindObjectOfType<XROrigin>();
            base.Start();

        }

        public override void Update()
        {
            base.Update();
            if (justGrabbed)
                justGrabbedPosition = xrOrigin.transform.position;
        }


        public override void Interacting()
        {
            if (grabbing)
            {
                
                // 1. Calculate force of the object
                Vector3 force = Vector3.zero;

                if (xrOrigin != null && !justGrabbed)
                {
                    force = (xrOrigin.transform.position - justGrabbedPosition) * multiplier; // need to make it a xrOrigin, because the hand is on the same place

                    Vector3 displacement = (currentlyInteractingBodyPart.transform.position - previouslyGrabbedPosition);

                    xrOrigin.transform.position -= displacement;
                }

                previouslyGrabbedPosition = currentlyInteractingBodyPart.transform.position;
                InvokeOnForce(currentlyInteractingBodyPart.BodyPart, force.magnitude);
            }
        }
    }
}