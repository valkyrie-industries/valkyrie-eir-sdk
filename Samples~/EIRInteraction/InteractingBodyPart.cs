using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Interaction
{

    /// <summary>
    /// Body part for physical interaction with objects.
    /// Associated with the tracked device on the same gameobject
    /// Body parts are indicated by the public enum BodyPart. Currently works for hands and head, but extendable to all the others
    /// </summary>
    public class InteractingBodyPart : MonoBehaviour {

        public delegate void InteractingBodyPartAliveEventHandler(InteractingBodyPart i);
        public static InteractingBodyPartAliveEventHandler OnInteractingBodyPartAlive;

        public BodyPart BodyPart { get { return bodyPart; } }

        [SerializeField]
        private BodyPart bodyPart;

        #region Dynamic properties
        /// <summary>
        /// The position of the object.
        /// </summary>
        public Vector3 position {
            get => transform.position;
        }
        /// <summary>
        /// The velocity of the object.
        /// </summary>
        public Vector3 velocity {
            get => m_velocity;
        }
        private Vector3 m_velocity;

        /// <summary>
        /// The velocity of the object.
        /// </summary>
        public Vector3 averageVelocity {
            get => (position - lastPosition) / Time.deltaTime;
        }


        /// <summary>
        /// The accleration of the object.
        /// </summary>
        public Vector3 acceleration {
            //get => (velocity - lastVelocity) / Time.deltaTime; // Smooth it
            get {
                m_acceleration = Vector3.Lerp(m_acceleration, (velocity - lastVelocity) / Time.deltaTime, 0.5f);
                return m_acceleration;
            }
        }

        private Vector3 m_acceleration;

        /// <summary>
        /// The rotation of the object.
        /// </summary>
        public Quaternion rotation {
            get => transform.rotation;
        }

        /// <summary>
        /// The angular velocity of the object.
        /// </summary>
        public Quaternion angularVelocity {
            get => m_angularVelocity;
        }
        private Quaternion m_angularVelocity;

        public float velocitySmooth {
            get => m_velocitySmooth;
            set => m_velocitySmooth = value;
        }
        private float m_velocitySmooth;
        #endregion

        #region Interaction Properties
        /// <summary>
        /// If the body part is gripping. Available for hands
        /// </summary>
        public bool grip {
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
        private bool m_grip;

        #endregion

        private Vector3 lastPosition;
        private Vector3 lastVelocity;

        private void Awake() {
            OnInteractingBodyPartAlive?.Invoke(this);
        }


        private void LateUpdate() {
            m_velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
        }

        public void Grip(bool _grip) {
            grip = _grip;
        }
    }
}