using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Haptics;

namespace Valkyrie.EIR.Interaction.Interactables
{
    /// <summary>
    /// Checks if it is grabbed by the hand (the "currently interacting body part")
    /// Public Bools: grabbing, justGrabbed, justDropped
    /// </summary>  
    [RequireComponent(typeof(XRGrabInteractable))]
    public class GrabInteractable : Interactable {
        // Currently uses Unity XR Interactable
        public bool grabbing, justGrabbed, justDropped;


        private XRGrabInteractable xRGrabInteractable;

        public XRGrabInteractable XRGrabInteractable {
            get {
                try {
                    if (xRGrabInteractable == null) {
                        xRGrabInteractable = GetComponent<XRGrabInteractable>();
                        if (xRGrabInteractable == null) {
                            Debug.LogError("[Grab Interactable] XRGrabInteractable component not found on the GameObject or its children.");
                        }
                    }
                    return xRGrabInteractable;
                }
                catch (Exception ex) {
                    Debug.LogError("[Grab Interactable] An error occurred while getting XRGrabInteractable: " + ex.Message);
                    return null;
                }
            }
        }

        public virtual void Start() {

            if (XRGrabInteractable == null) return;

            xRGrabInteractable = GetComponent<XRGrabInteractable>();
            xRGrabInteractable.selectEntered.AddListener(SelectEntered);
            xRGrabInteractable.selectExited.AddListener(SelectExited);
        }

        protected void SelectEntered(SelectEnterEventArgs args) {
            if (args.interactorObject.transform.GetComponentInParent<InteractingBodyPart>() != null) {
                currentlyInteractingBodyPart = args.interactorObject.transform.GetComponentInParent<InteractingBodyPart>();
                grabbing = true;
                justGrabbed = true;
                StartCoroutine(JustGrabbed());
            }
        }


        protected void SelectExited(SelectExitEventArgs args) {
            if (args.interactorObject.transform.GetComponentInParent<InteractingBodyPart>() != null) {
                SendZeroForce();
                currentlyInteractingBodyPart = null;
                grabbing = false;
                justDropped = true;
                StartCoroutine(JustDropped());
            }
        }
        private IEnumerator JustGrabbed() {
            yield return new WaitForEndOfFrame();
            justGrabbed = false;
        }
        private IEnumerator JustDropped() {
            yield return new WaitForEndOfFrame();
            justDropped = false;
        }

        public virtual void SendZeroForce() {
            if (currentlyInteractingBodyPart != null) InvokeOnForce(currentlyInteractingBodyPart.BodyPart, 0);
        }

        public void ApplyForceToGrabbingArm(float force) {
            if (currentlyInteractingBodyPart != null) InvokeOnForce(currentlyInteractingBodyPart.BodyPart, force);
        }

        public void RestarGrabInteractable() {
            xRGrabInteractable.enabled = true;
        }

        // Force drop
        public void ForceDrop(bool doNotWait = false) {
            StartCoroutine(DropRoutine(doNotWait));
        }

        private IEnumerator DropRoutine(bool doNotWait = false) {
            if (currentlyInteractingBodyPart != null) {
                Debug.Log("Force Dropping");
                // Turn off xrgrabinteractable
                BodyPart lastInteractingBodyPart = currentlyInteractingBodyPart.BodyPart;
                xRGrabInteractable.enabled = false;

                if (doNotWait) {
                    yield return new WaitForEndOfFrame();
                    InvokeOnForce(lastInteractingBodyPart, 0);
                    yield break;
                }

                yield return new WaitForSeconds(0.5f); // Used to be 0.2f

                grabbing = false;
                justDropped = true;
                currentlyInteractingBodyPart = null;
                yield return new WaitForEndOfFrame();

                // SendZeros to the last interacting body part & turn back xrgrabinteractable
                InvokeOnForce(lastInteractingBodyPart, 0);
                xRGrabInteractable.enabled = true;

            }
        }
    }
}