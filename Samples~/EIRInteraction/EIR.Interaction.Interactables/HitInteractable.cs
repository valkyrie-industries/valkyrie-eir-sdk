using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Utilities;
using Valkyrie.EIR.Interaction;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Interaction.Interactables
{
    /// <summary>
    /// Checks if it is hit by the hand and sends 
    /// short impulse of various envelopes to the Interaction Manager
    /// </summary>  
    public class HitInteractable : TouchInteractable
    {
        public readonly float forceCoefficient = ValkyrieEIRExtensionMethods.hitForceMultiplier;

        public enum Envelope { Square, Triangular, ExpDecay, ExpGrowth, Sin };

        public Envelope hitEnvelope = Envelope.Square;

        private bool hitIsRunning;

        public override void TouchInteraction(Vector3 velocity)
        {
            StartCoroutine(PerformHit(velocity, currentlyInteractingBodyPart.BodyPart));
        }

        private IEnumerator PerformHit(Vector3 velocity, BodyPart bodyPart)
        {
            float t = Time.realtimeSinceStartup;
            while ((Time.realtimeSinceStartup - t) < interactionDuration)
            {
                hitIsRunning = true;
                switch (hitEnvelope)
                {
                    case Envelope.Square:
                        InvokeOnForce(bodyPart, forceCoefficient * velocity.magnitude);
                        break;
                    case Envelope.Triangular:
                        float slope = 1.0f - (Time.realtimeSinceStartup - t) / interactionDuration;
                        InvokeOnForce(bodyPart, forceCoefficient * velocity.magnitude * slope);
                        break;
                    case Envelope.ExpDecay:
                        float expSlope = - 3.0f * (Time.realtimeSinceStartup - t) / interactionDuration;
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
    }
}