using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Interaction.Interactables;
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Examples {
    public class BowAndArrow : MonoBehaviour
    {
        [SerializeField]
        Transform bow, grabPoint, topPoint, bottomPoint, midPoint, arrow;
        [SerializeField]
        LineRenderer topLine, bottomLine;

        bool grabbing;
        GrabInteractable arrowGrabInteractable;
        InteractionManager interactionManager;

        public float forceMultiplier = 20;
        public float speedMultiplier = 20;

        Vector3 initialMidPoint, initialArrowPosition;
        Quaternion initialArrowRotation;

        Vector3 direction;
        bool arrowFlying;
        // Start is called before the first frame update
        void Start()
        {
            arrowGrabInteractable = grabPoint.GetComponent<GrabInteractable>();
            if (arrowGrabInteractable == null)
                Debug.LogError("No XR Grab Interactable is found on the grab point!");

            initialMidPoint = grabPoint.localPosition;
            initialArrowPosition = grabPoint.InverseTransformPoint(arrow.position);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateBowVisuals();
            UpdateBowPhysics();
            UpdateEIRsignal();
            GetArrow();
            FireArrow();
        }

        private void FixedUpdate()
        {
            UpdateBowVisuals();
        }

        void UpdateBowVisuals()
        {
            // Update String
            Vector3[] topPositions = { topPoint.position, grabPoint.position };
            topLine.SetPositions(topPositions);

            Vector3[] bottomPositions = { bottomPoint.position, grabPoint.position };
            bottomLine.SetPositions(bottomPositions);

            // Update Arrow pointing
            if (!arrowFlying)
            {
                Quaternion pointingDirection = Quaternion.LookRotation(midPoint.position - grabPoint.position);
                arrow.rotation = pointingDirection;
            }
        }

        void UpdateBowPhysics()
        {
            if (!arrowGrabInteractable.grabbing)
            {
                Vector3 displacement = (grabPoint.localPosition - initialMidPoint);
                if (displacement.magnitude > 0.02f)
                {
                    grabPoint.localPosition -= Time.deltaTime * forceMultiplier * displacement;
                }
            }
        }

        void UpdateEIRsignal()
        {
            if (arrowGrabInteractable.grabbing)
            {
                direction = (transform.TransformPoint(initialMidPoint) - grabPoint.position);
                float force = ValkyrieEIRExtensionMethods.Map(direction.magnitude, 0, 0.5f);
                arrowGrabInteractable.ApplyForceToGrabbingArm(force);
                bool isLeft = arrowGrabInteractable.currentlyInteractingBodyPart.BodyPart == 0 ? false : true;
                EIRManager.Instance.Interaction.XrBridge.SendVibration(isLeft, force * 0.5f, 0.01f);
            }
        }

        void GetArrow()
        {
            if (arrowGrabInteractable.justGrabbed)
            {
                StopAllCoroutines();
                ReturnArrow();
            }
        }

        void FireArrow()
        {
            if (arrowGrabInteractable.justDropped)
            {
                Rigidbody arrowRB = arrow.GetComponent<Rigidbody>();
                arrow.SetParent(null);
                arrowRB.useGravity = true;
                arrowRB.isKinematic = false;
                Vector3 flyDirection = (midPoint.position - grabPoint.position);
                arrowRB.velocity = flyDirection * speedMultiplier;
                StartCoroutine(ReturnArrowCoroutine());
                arrowGrabInteractable.SendZeroForce();
                EIRManager.Instance.Interaction.XrBridge.SendVibration(true, 0, 0);
                EIRManager.Instance.Interaction.XrBridge.SendVibration(false, 0, 0);
                arrowFlying = true;
            }
        }

        IEnumerator ReturnArrowCoroutine()
        {
            yield return new WaitForSeconds(4);
            ReturnArrow();
        }

        void ReturnArrow()
        {
            Rigidbody arrowRB = arrow.GetComponent<Rigidbody>();
            arrow.SetParent(grabPoint);
            arrowRB.useGravity = false;
            arrowRB.isKinematic = true;
            arrow.position = grabPoint.TransformPoint(initialArrowPosition);
            arrow.localRotation = Quaternion.identity;
            arrowFlying = false;
        }

    }
}


