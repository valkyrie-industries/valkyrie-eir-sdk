using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Valkyrie.EIR.Interaction {

    /// <summary>
    /// Body part for physical interaction with objects.
    /// Associated with the tracked device on the same gameobject
    /// Body parts are indicated by the public enum BodyPart. Currently works for hands and head, but extendable to all the others
    /// </summary>
    public class InteractingBodyPart : MonoBehaviour {

        #region Delegates

        public delegate void InteractingBodyPartAliveEventHandler(InteractingBodyPart i);

        #endregion

        #region Events

        public static InteractingBodyPartAliveEventHandler OnInteractingBodyPartAlive;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the BodyPart associated with this InteractingBodyPart.
        /// </summary>
        public BodyPart BodyPart { get { return bodyPart; } }

        /// <summary>
        /// Returns the position of the object.
        /// </summary>
        public Vector3 position {
            get => transform.position;
        }
        /// <summary>
        /// Returns the velocity of the object.
        /// </summary>
        public Vector3 velocity {
            get => m_velocity;
        }

        /// <summary>
        /// Returns the average velocity of the object.
        /// </summary>
        public Vector3 averageVelocity {
            get => (position - lastPosition) / Time.deltaTime;
        }

        /// <summary>
        /// Returns the accleration of the object.
        /// </summary>
        public Vector3 acceleration {
            //get => (velocity - lastVelocity) / Time.deltaTime; // Smooth it
            get {
                m_acceleration = Vector3.Lerp(m_acceleration, (velocity - lastVelocity) / Time.deltaTime, 0.5f);
                return m_acceleration;
            }
        }

        /// <summary>
        /// Returns the rotation of the object, expressed as a Quaternion.
        /// </summary>
        public Quaternion rotation {
            get => transform.rotation;
        }

        /// <summary>
        /// Returns the angular velocity of the object.
        /// </summary>
        public Quaternion angularVelocity {
            get => m_angularVelocity;
        }

        /// <summary>
        /// Returns the smoothed velocity of the object.
        /// </summary>
        public float velocitySmooth {
            get => m_velocitySmooth;
            set => m_velocitySmooth = value;
        }

        /// <summary>
        /// If the body part is gripping. Available for hands
        /// </summary>
        public bool Grip {
            get {
                m_grip = false;
                var controller = GetComponent<XRController>();
                if (controller != null) {
                    controller.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripTarget);
                    m_grip = gripTarget > 0.95f;
                }
                return m_grip;
            }
            set {
                if (BodyPart == BodyPart.leftHand || BodyPart == BodyPart.leftHand)
                    m_grip = value;
                else
                    m_grip = false;

            }
        }

        #endregion

        #region Serialized Variables

        [SerializeField]
        private BodyPart bodyPart;

        #endregion

        #region Private Variables

        private Vector3 m_velocity;
        private Vector3 m_acceleration;
        private Quaternion m_angularVelocity;
        private float m_velocitySmooth;
        private bool m_grip;
        private Vector3 lastPosition;
        private Vector3 lastVelocity;

        #endregion

        #region Unity Methods

        private void Awake() {
            OnInteractingBodyPartAlive?.Invoke(this);
        }


        private void LateUpdate() {
            m_velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
        }

        #endregion
    }
}