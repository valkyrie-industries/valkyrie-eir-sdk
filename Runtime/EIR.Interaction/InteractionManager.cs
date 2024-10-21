using System;

namespace Valkyrie.EIR.Interaction {

    /// <summary>
    /// The Valkyrie Interaction Manager is an interface between all the body parts, 
    /// interactable objects and can request the application of EMS/FES.
    /// </summary>
    [Serializable]
    public class InteractionManager {

        #region Events

        public delegate void HapticRequestEventHandler(BodyPart bodyPart, float force, bool bypassCalibration);
        public event HapticRequestEventHandler HapticRequest;

        #endregion

        #region Constants

        public const int UsedBodyParts = 3;
        public static int TotalBodyParts { get { return Enum.GetNames(typeof(BodyPart)).Length; } }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns all interacting body parts available for use.
        /// </summary>
        public InteractingBodyPart[] InteractingBodyParts {
            get {
                if (interactingBodyParts[0] == null) {
                    InteractingBodyPart[] ints = UnityEngine.GameObject.FindObjectsOfType<InteractingBodyPart>();
                    if (ints != null) interactingBodyParts = OrganiseBodyParts(ints);
                }

                return interactingBodyParts;
            }
        }

        #endregion

        #region Private Variables

        private InteractingBodyPart[] interactingBodyParts;
        private float[] forces;

        #endregion

        #region Private Methods

        private InteractingBodyPart[] OrganiseBodyParts(InteractingBodyPart[] unorganised) {
            InteractingBodyPart[] ints = new InteractingBodyPart[unorganised.Length];
            for (int i = 0; i < unorganised.Length; ++i) {
                ints[(int)unorganised[i].BodyPart] = unorganised[i];
            }
            return ints;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Invoked when a force application has been requested for a particular body part.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="force"></param>
        /// <param name="_bypassCalibration"></param>
        private void OnApplyForceReqested(BodyPart bodyPart, float force, bool _bypassCalibration = false) {
            if ((int)bodyPart > forces.Length - 1)
                return;
            forces[(int)bodyPart] = force;
            HapticRequest?.Invoke(bodyPart, force, _bypassCalibration);
        }

        /// <summary>
        /// Invoked when a body part is initialised, record it in the usedbodyparts array.
        /// </summary>
        /// <param name="i"></param>
        private void OnInteractingBodyPartAlive(InteractingBodyPart i) {

            interactingBodyParts[(int)i.BodyPart] = i;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an InteractionManager object
        /// </summary>
        public InteractionManager(UnityEngine.GameObject g) {
            UnityEngine.Debug.Log($"[Interaction Manager] Interaction Manager Initialised from {g.name}.");
            interactingBodyParts = new InteractingBodyPart[UsedBodyParts];
            forces = new float[UsedBodyParts - 1];
            Interactable.OnForce += OnApplyForceReqested;
            InteractingBodyPart.OnInteractingBodyPartAlive += OnInteractingBodyPartAlive;
        }

        #endregion
    }
}