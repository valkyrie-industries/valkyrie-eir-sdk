using System;
using UnityEngine;

namespace Valkyrie.EIR.Interaction.Interactables {
    /// <summary>
    /// Checks if it is touched by the bodypart (the "currently interacting body part")
    /// and sends TouchInteraction() function
    /// </summary>
    public class TouchInteractable : Interactable {

        #region Serialized Variables

        [SerializeField]
        protected float interactionDuration = 0.3f;

        #endregion

        #region Private Variables

        protected bool touched;
        protected bool partDependent = false;

        #endregion

        #region Unity Methods

        protected virtual void OnCollisionEnter(Collision collision) {
            try {
                Collider collidedObject = collision.collider;
                if (collidedObject == null) {
                    Debug.LogError("[TouchInteractable.OnCollisionEnter] Collision object is null.");
                    return;
                }

                InteractingBodyPart bodyPart = collidedObject.GetComponent<InteractingBodyPart>();
                if (bodyPart == null) {
                    Debug.LogWarning("[TouchInteractable.OnCollisionEnter] No InteractingBodyPart component found on the collided object.");
                    return;
                }

                Vector3 force = collision.impulse / Time.fixedDeltaTime;
                currentlyInteractingBodyPart = bodyPart;
                TouchInteraction(force);
                touched = true;
            }
            catch (Exception ex) {
                Debug.LogError($"[TouchInteractable.OnCollisionEnter] {ex.Message}");
            }
        }

        protected virtual void OnCollisionExit(Collision collision) {
            try {
                Collider collidedObject = collision.collider;
                if (collidedObject == null) {
                    Debug.LogError("[TouchInteractable.OnCollisionExit] Collision object is null.");
                    return;
                }

                InteractingBodyPart bodyPart = collidedObject.GetComponent<InteractingBodyPart>();
                if (bodyPart != null) {
                    touched = false;
                }
            } catch (Exception ex) {
                Debug.LogError($"[TouchInteractable.OnCollisionExit] {ex.Message}");
            }
        }

        protected virtual void OnTriggerEnter(Collider collider) {
            Collider collidedObject = collider;
            InteractingBodyPart bodyPart = collidedObject.GetComponent<InteractingBodyPart>();
            if (bodyPart != null) {
                Vector3 velocity = bodyPart.velocity;
                currentlyInteractingBodyPart = bodyPart;
                if (partDependent) {
                    TouchInteraction(velocity, bodyPart.BodyPart);
                }
                else {
                    TouchInteraction(velocity);
                }

                touched = true;
            }
        }

        protected virtual void OnTriggerExit(Collider collider) {
            try {
                Collider collidedObject = collider;
                if (collidedObject == null) {
                    Debug.LogError("[TouchInteractable.OnTriggerExit] Collider object is null.");
                    return;
                }

                InteractingBodyPart bodyPart = collidedObject.GetComponent<InteractingBodyPart>();
                if (bodyPart != null) {
                    touched = false;
                }
            }
            catch (Exception ex) {
                Debug.LogError($"[TouchInteractable.OnTriggerExit] {ex.Message}");
            }
        }

        protected void OnDisable() {
            touched = false;
        }

        #endregion

        #region Private Methods

        protected virtual void TouchInteraction(Vector3 velocity) {
            // empty in base.
        }

        protected virtual void TouchInteraction(Vector3 velocity, BodyPart part) {
            // empty in base.
        }

        #endregion
    }
}