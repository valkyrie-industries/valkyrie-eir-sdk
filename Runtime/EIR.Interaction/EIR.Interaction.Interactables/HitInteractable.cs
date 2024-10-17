using System.Collections;
using UnityEngine;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction.Interactables {

    /// <summary>
    /// Checks if it is hit by the hand and sends 
    /// short impulse of various envelopes to the Interaction Manager
    /// </summary>  
    public class HitInteractable : TouchInteractable {

        #region Enums
        public enum Envelope {
            Square,
            Triangular,
            ExpDecay,
            ExpGrowth,
            Sin
        };


        #endregion

        #region Constants

        public readonly float forceCoefficient = ValkyrieEIRExtensionMethods.HitForceMultiplier;

        #endregion

        #region Serialized Variables

        [SerializeField]
        private Envelope hitEnvelope = Envelope.Square;

        #endregion

        #region Private Variables

        private bool hitIsRunning;

        #endregion

        #region Private Methods

        protected override void TouchInteraction(Vector3 velocity) {
            StartCoroutine(PerformHit(velocity, currentlyInteractingBodyPart.BodyPart));
        }

        private IEnumerator PerformHit(Vector3 velocity, BodyPart bodyPart) {
            float t = Time.realtimeSinceStartup;
            while ((Time.realtimeSinceStartup - t) < interactionDuration) {
                hitIsRunning = true;
                switch (hitEnvelope) {
                    case Envelope.Square:
                        InvokeOnForce(bodyPart, forceCoefficient * velocity.magnitude);
                        break;
                    case Envelope.Triangular:
                        float slope = 1.0f - (Time.realtimeSinceStartup - t) / interactionDuration;
                        InvokeOnForce(bodyPart, forceCoefficient * velocity.magnitude * slope);
                        break;
                    case Envelope.ExpDecay:
                        float expSlope = -3.0f * (Time.realtimeSinceStartup - t) / interactionDuration;
                        InvokeOnForce(bodyPart, forceCoefficient * velocity.magnitude * Mathf.Exp(expSlope));
                        break;
                }
                yield return null;
            }
            hitIsRunning = false;
            InvokeOnForce(bodyPart, 0);
            yield return new WaitForSeconds(0.1f);
            InvokeOnForce(bodyPart, 0);
            if (currentlyInteractingBodyPart == null)
                yield break;
            if (bodyPart == currentlyInteractingBodyPart.BodyPart && !hitIsRunning)
                currentlyInteractingBodyPart = null;
        }

        #endregion
    }
}