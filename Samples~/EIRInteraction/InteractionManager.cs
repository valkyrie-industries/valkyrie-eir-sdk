using System;
using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Interaction
{
    /// <summary>
    /// Valkyrie Interaction Manager is an interface between all the body parts, 
    /// interactable objects and EMS force creator.
    /// </summary>
    [Serializable]
    public class InteractionManager {


        public delegate void HapticRequestEventHandler(BodyPart bodyPart, float force, bool bypassCalibration);
        public event HapticRequestEventHandler HapticRequest;

        // Global Access to all the body parts that take part in the exercise
        private InteractingBodyPart[] interactingBodyParts;

        //This is the number of bodyparts we use
        public static readonly int usedBodyParts = 3;

        public VLKXRBridge XrBridge { get { return xrBridge; } }
        private VLKXRBridge xrBridge;

        //This is the number of bodyparts in total
        public static int TotalBodyParts { get { return Enum.GetNames(typeof(BodyPart)).Length; } }

        private float[] forces;

        public InteractionManager(bool enableXRBridge = true) {
            UnityEngine.Debug.Log("[Interaction] Interaction Manager Intiialised");
            interactingBodyParts = new InteractingBodyPart[usedBodyParts];
            forces = new float[usedBodyParts - 1];
            Interactable.OnForce += ApplyForce;
            InteractingBodyPart.OnInteractingBodyPartAlive += OnInteractingBodyPartAlive;
            if (enableXRBridge) xrBridge = new VLKXRBridge();
        }

        public InteractingBodyPart[] InteractingBodyParts {
            get { if (interactingBodyParts[0] == null) { InteractingBodyPart[] ints = GameObject.FindObjectsOfType<InteractingBodyPart>(); if (ints != null) interactingBodyParts = OrganiseBodyParts(ints); } return interactingBodyParts; }
        }

        private InteractingBodyPart[] OrganiseBodyParts(InteractingBodyPart[] unorganised) {
            InteractingBodyPart[] ints = new InteractingBodyPart[unorganised.Length];
            for (int i = 0; i < unorganised.Length; ++i) {
                ints[(int)unorganised[i].BodyPart] = unorganised[i];
            }
            return ints;
        }

        /// <summary>
        /// When a body part is initialised, record it in the usedbodyparts array.
        /// </summary>
        /// <param name="i"></param>
        private void OnInteractingBodyPartAlive(InteractingBodyPart i) {

            interactingBodyParts[(int)i.BodyPart] = i;
        }

        /// <summary>
        /// Todo: move this to the ApplyForce event handler once SkiTarget has been addressed.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="force"></param>
        /// <param name="_bypassCalibration"></param>
        public void ApplyForce(BodyPart bodyPart, float force, bool _bypassCalibration = false) {
            if ((int)bodyPart > forces.Length - 1)
                return;
            forces[(int)bodyPart] = force;
            HapticRequest?.Invoke(bodyPart, force, _bypassCalibration);
        }
    }
}