using UnityEngine;
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Haptics;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction.Interactables {
    /// <summary>
    /// Valkyrie Gravity Grab Interactable: checks the object's mass and acceleration.
    /// Every frame it sends this force to the InteractionManager, which then exerts EMS on the required hand.
    /// </summary>
    public class KinematicGrabInteractable : GrabInteractable
    {
        public float mass;
        public Transform machineAttachmentPoint;
        public float minExtension = 0.1f;
        public EmsPhysicsMode emsPhysicsMode;
        [SerializeField]
        private float angleFraction = 0.5f;

        private float accelerationMultiplier = ValkyrieEIRExtensionMethods.accelerationMultiplier;
        private float massMultiplier = ValkyrieEIRExtensionMethods.massMultipier;

        private float height, headSize;

        private Vector3 shoulderPosition, midArmPosition, elbowPosition;

        private float angle;
        private float forceAngle;


        public override void Update()
        {
            height = 1.75f;// Fitness.PlayerSettings.meanPlayerHeight;
            headSize = height / 7.5f;

            base.Update();
        }
        public override void Interacting()
        {
            if (grabbing)
            {
                // 1. Calculate force of the object
                Vector3 force = Vector3.zero;

                switch (emsPhysicsMode)
                {
                    case EmsPhysicsMode.TensionBasedMass:
                        Vector3 tension = mass * (machineAttachmentPoint.position - transform.position).normalized;
                        force = tension;
                        break;
                    case EmsPhysicsMode.MassOnly:
                    case EmsPhysicsMode.MassAndAccelerationVector:
                    case EmsPhysicsMode.MassAndElbowAngle:
                    case EmsPhysicsMode.MassAndAccelerationScalar:
                    case EmsPhysicsMode.MassAndElbowAngleAndAccelerationScalar:
                        Vector3 gravitationalForce = new Vector3(0, - mass, 0);
                        force = gravitationalForce;
                        break;
                }
                
                // 2. Apply multipliers:
                force *= massMultiplier;

                // 3. Calculate acceleration
                Vector3 acceleration = accelerationMultiplier * currentlyInteractingBodyPart.acceleration;

                float kinematicForce = 0; 

                // 4. Combine object force and acceleration
                switch (emsPhysicsMode)
                {
                    case EmsPhysicsMode.MassOnly:
                        kinematicForce = force.magnitude;
                        break;
                    case EmsPhysicsMode.TensionBasedMass:
                        kinematicForce = (force - acceleration).magnitude;
                        // If the system is not tensed, then no force
                        if ((machineAttachmentPoint.position - transform.position).magnitude < minExtension)
                        {
                            kinematicForce *= (machineAttachmentPoint.position - transform.position).magnitude / minExtension;
                        }
                        break;
                    case EmsPhysicsMode.MassAndAccelerationVector:
                        kinematicForce = (force - acceleration).magnitude;
                        break;
                    case EmsPhysicsMode.MassAndElbowAngle:
                        kinematicForce = (force - acceleration).magnitude;
                        // Calculate the elbow position:
                        elbowPosition = ElbowPosition(currentlyInteractingBodyPart, EIRManager.Instance.Interaction.InteractingBodyParts[(int)BodyPart.head].transform, headSize);
                        angle = Vector3.Angle((currentlyInteractingBodyPart.position - elbowPosition),Vector3.down); // Angle between the force and the forearm
                        forceAngle = Mathf.Clamp(Mathf.Sin(angle * Mathf.PI / 180.0f), 0, 1);
                        kinematicForce = kinematicForce * (1.0f - angleFraction) + kinematicForce * forceAngle * angleFraction;
                        break;
                    case EmsPhysicsMode.MassAndAccelerationScalar:
                        kinematicForce = force.magnitude + acceleration.magnitude;
                        break;
                    case EmsPhysicsMode.MassAndElbowAngleAndAccelerationScalar:
                        kinematicForce = force.magnitude + acceleration.magnitude;
                        // Calculate the elbow position:
                        elbowPosition = ElbowPosition(currentlyInteractingBodyPart, EIRManager.Instance.Interaction.InteractingBodyParts[(int)BodyPart.head].transform, headSize);
                        angle = Vector3.Angle((currentlyInteractingBodyPart.position - elbowPosition), Vector3.down); // Angle between the force and the forearm
                        forceAngle = Mathf.Clamp(Mathf.Sin(angle * Mathf.PI / 180.0f) + angle * 0.002f, 0, 1);
                        kinematicForce = kinematicForce * (1.0f - angleFraction) + kinematicForce * forceAngle * angleFraction;
                        break;
                }

                InvokeOnForce(currentlyInteractingBodyPart.BodyPart, kinematicForce);
            }
        }

        private Vector3 ElbowPosition(InteractingBodyPart hand, Transform _head, float _headSize)
        {

            Vector3 handPosition = hand.transform.position;
            int isLeft = hand.BodyPart == BodyPart.leftHand ? 1 : -1; // which side of the neck the shoulder is
            shoulderPosition = _head.position + _headSize * _head.TransformDirection(Vector3.down) + _headSize * 0.75f * isLeft * _head.TransformDirection(Vector3.left);
            Vector3 shoulderToHandDirection = handPosition - shoulderPosition;
            float shoulderToHandDistance = Vector3.Distance(handPosition, shoulderPosition);
            midArmPosition = (shoulderPosition + handPosition) / 2;
            float offset = Mathf.Sqrt(Mathf.Clamp(Mathf.Pow((_headSize * 1.5f), 2) - Mathf.Pow((shoulderToHandDistance * 0.5f), 2), 0, _headSize * 1.5f)); // Pythagoras
            Vector3 offsetDirection = Vector3.Cross(Vector3.Cross(shoulderToHandDirection, Vector3.down), shoulderToHandDirection);
            Vector3 _elbowPosition = midArmPosition + offsetDirection.normalized * offset;

            return _elbowPosition;
        }
    }
}