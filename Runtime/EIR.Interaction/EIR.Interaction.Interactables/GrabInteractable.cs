using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Valkyrie.EIR.Interaction.Interactables
{
    /// <summary>
    /// Checks if it is grabbed by the hand (the "currently interacting body part")
    /// </summary>  
    [RequireComponent(typeof(XRGrabInteractable))]
    public class GrabInteractable : Interactable {

        #region Public Properties

        /// <summary>
        /// Returns whether the interactable is being grabbed or not.
        /// </summary>
        public bool IsGrabbing { get { return grabbing; } }

        /// <summary>
        /// Returns true on the first frame after grabbing to signal that the interactable has just been grabbed on that frame.
        /// </summary>
        public bool isJustGrabbed { get { return justGrabbed; } }

        /// <summary>
        /// Returns true on the first frame after dropping to signal that the interactable has just been dropped on that frame.
        /// </summary>
        public bool isJustDropped { get { return justDropped; } }

        /// <summary>
        /// Returns the interactable's XRGrabInteractable component.
        /// </summary>
        public XRGrabInteractable XRGrabInteractable {
            get {
                try {
                    if (xRGrabInteractable == null) {
                        xRGrabInteractable = GetComponent<XRGrabInteractable>();
                        if (xRGrabInteractable == null) {
                            Debug.LogError("[Grab Interactable] XRGrabInteractable component not found on this GameObject or its children.");
                        }
                    }
                    return xRGrabInteractable;
                } catch (Exception ex) {
                    Debug.LogError($"[Grab Interactable] An error occurred while getting XRGrabInteractable: {ex.Message}");
                    return null;
                }
            }
        }

        #endregion

        #region Serialized Variables

        [SerializeField]
        protected bool grabbing, justGrabbed, justDropped;

        #endregion

        #region Private Variables

        private XRGrabInteractable xRGrabInteractable;

        #endregion

        #region Unity Methods

        protected virtual void Start() {

            if (XRGrabInteractable == null) return;

            xRGrabInteractable = GetComponent<XRGrabInteractable>();
            xRGrabInteractable.selectEntered.AddListener(SelectEntered);
            xRGrabInteractable.selectExited.AddListener(SelectExited);
        }

        #endregion

        #region Public Variables

        /// <summary>
        /// Sends zero force to the currently interacting body part, if one is available.
        /// </summary>
        public virtual void SendZeroForce() {
            if (currentlyInteractingBodyPart != null) InvokeOnForce(currentlyInteractingBodyPart.BodyPart, 0);
        }

        /// <summary>
        /// Applies the input value of force to the currently interacting body part, if one is available.
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForceToGrabbingArm(float force) {
            if (currentlyInteractingBodyPart != null) InvokeOnForce(currentlyInteractingBodyPart.BodyPart, force);
        }

        /// <summary>
        /// Enables the xRGrabInteractable object.
        /// </summary>
        public void RestarGrabInteractable() {
            xRGrabInteractable.enabled = true;
        }

        /// <summary>
        /// Forces the interactable to drop and resets isGrabbing to false.
        /// </summary>
        /// <param name="doNotWait"></param>
        public void ForceDrop(bool doNotWait = false) {
            StartCoroutine(DropInteractable(doNotWait));
        }

        #endregion

        #region Private Variables

        protected void SelectEntered(SelectEnterEventArgs args) {
            if (args.interactorObject.transform.GetComponentInParent<InteractingBodyPart>() != null) {
                currentlyInteractingBodyPart = args.interactorObject.transform.GetComponentInParent<InteractingBodyPart>();
                grabbing = true;
                StartCoroutine(MarkJustGrabbed());
            }
        }


        protected void SelectExited(SelectExitEventArgs args) {
            if (args.interactorObject.transform.GetComponentInParent<InteractingBodyPart>() != null) {
                SendZeroForce();
                currentlyInteractingBodyPart = null;
                grabbing = false;
                StartCoroutine(MarkJustDropped());
            }
        }

        private IEnumerator MarkJustGrabbed() {
            justGrabbed = true;
            yield return new WaitForEndOfFrame();
            justGrabbed = false;
        }

        private IEnumerator MarkJustDropped() {
            justDropped = true;
            yield return new WaitForEndOfFrame();
            justDropped = false;
        }

        private IEnumerator DropInteractable(bool doNotWait = false) {
            if (currentlyInteractingBodyPart != null) {
                // turn off xrgrabinteractable
                BodyPart lastInteractingBodyPart = currentlyInteractingBodyPart.BodyPart;
                xRGrabInteractable.enabled = false;

                // if the routine should bnot wait, drop immediately.
                if (doNotWait) {
                    yield return new WaitForEndOfFrame();
                    InvokeOnForce(lastInteractingBodyPart, 0);
                    yield break;
                }

                // otherwise, wait for a short duration and drop.
                yield return new WaitForSeconds(0.5f);

                grabbing = false;
                justDropped = true;
                currentlyInteractingBodyPart = null;
                yield return new WaitForEndOfFrame();
                justDropped = false;

                // sendZeros to the last interacting body part & turn back xrgrabinteractable
                InvokeOnForce(lastInteractingBodyPart, 0);
                xRGrabInteractable.enabled = true;
            }
        }

        #endregion
    }
}