using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Interaction.Interactables;
#endif
using Valkyrie.EIR.Utilities;


namespace Valkyrie.EIR.Examples
{
    public class Bow : MonoBehaviour
    {

#if EIR_INTERACTION

        [SerializeField]
        private Transform bow, topPoint, bottomPoint, midPoint, midStringPoint;
        [SerializeField]
        private LineRenderer topLine, bottomLine;

        private Transform arrowToGrab, arrowToFly;
        private Arrow[] arrows;

        private bool grabbing;
        private bool arrowOnBow;

        private GrabInteractable arrowGrabInteractable, bowGrabInteractable;
        private InteractionManager interactionManager;

        public float forceMultiplier = 20;
        public float speedMultiplier = 20;

        private Vector3 initialMidPoint, initialArrowPosition;
        private Quaternion initialArrowRotation;

        private Vector3 direction;

        private void Start()
        {
            bowGrabInteractable = GetComponent<GrabInteractable>();

            initialMidPoint = midStringPoint.localPosition;

            FindAllArrows();
        }

        private void Update()
        {
            UpdateBowVisuals();
            //UpdateBowPhysics();
            UpdateEIRsignal();
        }

        private void FixedUpdate()
        {
            UpdateBowVisuals();
        }

        private void UpdateBowVisuals()
        {
            // Update String visuas
            if (arrowOnBow)
            {
                Vector3[] topPositions = { topPoint.position, arrowToGrab.position };
                topLine.SetPositions(topPositions);

                Vector3[] bottomPositions = { bottomPoint.position, arrowToGrab.position };
                bottomLine.SetPositions(bottomPositions);
            }
            else
            {
                Vector3[] topPositions = { topPoint.position, midStringPoint.position };
                topLine.SetPositions(topPositions);

                Vector3[] bottomPositions = { bottomPoint.position, midStringPoint.position };
                bottomLine.SetPositions(bottomPositions);
            }

            // Check if the arrow is close and is grabbed
            if (!arrowOnBow)
            {
                foreach (Arrow a in arrows)
                {
                    GrabInteractable aGrabInteractable = a.GetComponent<GrabInteractable>();
                    if (aGrabInteractable.IsGrabbing)
                    {
                        if (Vector3.Distance(a.transform.position, midStringPoint.position) < 0.1f)
                        {
                            arrowToGrab = a.transform;
                            arrowToFly = a.arrowBody;
                            arrowOnBow = true;
                            arrowGrabInteractable = aGrabInteractable;
                        }
                    }

                }
            }

            // Check if arrow is released
            if (arrowOnBow && !arrowGrabInteractable.IsGrabbing)
            {
                arrowOnBow = false;
                FireArrow();
            }

            // Update Arrow pointing

            if (arrowOnBow)
            {
                Quaternion pointingDirection = Quaternion.LookRotation(midPoint.position - arrowToGrab.position);
                arrowToFly.rotation = pointingDirection;
            }
        }

        private void UpdateEIRsignal()
        {
            if (arrowGrabInteractable == null)
                return;
            if (arrowGrabInteractable.IsGrabbing && arrowOnBow)
            {
                direction = (transform.TransformPoint(initialMidPoint) - arrowToGrab.position);
                float force = ValkyrieEIRExtensionMethods.Map(direction.magnitude, 0, 0.5f);
                arrowGrabInteractable.ApplyForceToGrabbingArm(force);
                bool isLeft = arrowGrabInteractable.currentlyInteractingBodyPart.BodyPart == 0 ? false : true;
            }
        }

        private void FireArrow()
        {
            Debug.Log("[Bow] Arrow loosed...");
            StartCoroutine(DelayedFire());
        }

        private IEnumerator DelayedFire()
        {
            arrowGrabInteractable.SendZeroForce();
            arrowGrabInteractable = null;

            Transform flyingArrow = arrowToFly;
            yield return new WaitForEndOfFrame(); // This is so that the XR Interaction toolkit would return to it its original values for Rigidbody, and then we override it
            Rigidbody arrowRB = arrowToGrab.GetComponent<Rigidbody>();
            arrowRB.useGravity = true;
            arrowRB.isKinematic = false;
            Vector3 flyDirection = (transform.TransformPoint(initialMidPoint) - arrowToGrab.position);
            arrowToGrab.SetParent(null);
            arrowRB.velocity = flyDirection * speedMultiplier;
            arrowRB.gameObject.layer = 6;
            arrowRB.angularVelocity = Vector3.zero;
            //arrowRB.position += arrowRB.velocity;
            arrowRB.gameObject.GetComponent<Arrow>().BeginFlight();
        }

        private void FindAllArrows()
        {
            arrows = FindObjectsOfType<Arrow>();
            Debug.Log($"[Bow] Found {arrows} arrows");
        }
#endif
    }
}