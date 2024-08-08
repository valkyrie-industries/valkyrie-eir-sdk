using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR;
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Utilities;
using Valkyrie.EIR.Interaction.Interactables;

public class Bow : MonoBehaviour
{
    [SerializeField]
    Transform bow, topPoint, bottomPoint, midPoint, midStringPoint;
    [SerializeField]
    LineRenderer topLine, bottomLine;

    Transform arrowToGrab, arrowToFly;
    Arrow[] arrows;

    bool grabbing;
    bool arrowOnBow;
    GrabInteractable arrowGrabInteractable, bowGrabInteractable;
    InteractionManager interactionManager;

    public float forceMultiplier = 20;
    public float speedMultiplier = 20;

    Vector3 initialMidPoint, initialArrowPosition;
    Quaternion initialArrowRotation;

    Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        bowGrabInteractable = GetComponent<GrabInteractable>();

        initialMidPoint = midStringPoint.localPosition;

        FindAllArrows();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBowVisuals();
        //UpdateBowPhysics();
        UpdateEIRsignal();
    }

    private void FixedUpdate()
    {
        UpdateBowVisuals();
    }

    void UpdateBowVisuals()
    {
        // Update String visuas
        if (arrowOnBow)
        {
            Vector3[] topPositions = { topPoint.position, arrowToGrab.position };
            topLine.SetPositions(topPositions);

            Vector3[] bottomPositions = { bottomPoint.position, arrowToGrab.position };
            bottomLine.SetPositions(bottomPositions);
        } else
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
                if (aGrabInteractable.grabbing)
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
        if (arrowOnBow && !arrowGrabInteractable.grabbing)
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

    void UpdateEIRsignal()
    {
        if (arrowGrabInteractable == null)
            return;
        if (arrowGrabInteractable.grabbing && arrowOnBow)
        {
            direction = (transform.TransformPoint(initialMidPoint) - arrowToGrab.position);
            float force = ValkyrieEIRExtensionMethods.Map(direction.magnitude, 0, 0.5f);
            arrowGrabInteractable.ApplyForceToGrabbingArm(force);
            bool isLeft = arrowGrabInteractable.currentlyInteractingBodyPart.BodyPart == 0 ? false: true ;
            EIRManager.Instance.Interaction.XrBridge.SendVibration(isLeft, force * 0.5f, 0.01f);
        }
    }

    void FireArrow()
    {
        StartCoroutine(DelayedFire());
    }
    
    IEnumerator DelayedFire()
    {
        arrowGrabInteractable.SendZeroForce();
        arrowGrabInteractable = null;
        EIRManager.Instance.Interaction.XrBridge.SendVibration(true, 0, 0);
        EIRManager.Instance.Interaction.XrBridge.SendVibration(false, 0, 0);
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

    void FindAllArrows()
    {
        arrows = FindObjectsOfType<Arrow>();
        Debug.Log(arrows);
    }
    
}
