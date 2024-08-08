using System;
using UnityEngine;
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Haptics;

namespace Valkyrie.EIR.Interaction.Interactables {
    /// <summary>
    /// Checks if it is touched by the bodypart (the "currently interacting body part")
    /// and sends TouchInteraction() function
    /// </summary>
    public class TouchInteractable : Interactable {

        [SerializeField]
        protected float interactionDuration = 0.3f;
        protected bool touched;
        protected bool partDependent = false;

        public virtual void OnCollisionEnter(Collision collision) {
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

        public virtual void OnTriggerEnter(Collider collider) {
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

        public virtual void OnCollisionExit(Collision collision) {
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
            }
            catch (Exception ex) {
                Debug.LogError($"[TouchInteractable.OnCollisionExit] {ex.Message}");
            }
        }

        public virtual void OnTriggerExit(Collider collider) {
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


        public virtual void TouchInteraction(Vector3 velocity) {
            // empty in base.
        }

        public virtual void TouchInteraction(Vector3 velocity, BodyPart part) {
            // empty in base.
        }

        protected void OnDisable() {
            touched = false;
        }
    }
}