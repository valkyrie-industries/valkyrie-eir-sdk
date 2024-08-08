using System.Collections;
using UnityEngine;
using Valkyrie.EIR.Interaction;
using Unity.XR.CoreUtils;
using TMPro;

namespace Valkyrie.EIR.Examples
{
    /// <summary>

    /// </summary>  
    public class SkiTarget : MonoBehaviour
    {
        /*
        public bool[] engaging, justEngaged, justDisengaged;
        #if EIR_INTERACTION

        public InteractionManager interactionManager;


        private InteractingBodyPart[] skiStickSpikeHands;
#endif
        private Vector3[] justEngagedPositions;
        private SkiStickSpike[] skiSpikes;
        private Transform head;
        private float[] justEngagedHeadHeights;

        [SerializeField]
        float EMSmultiplier = 100f;

        [SerializeField]
        float velocityMultiplier = 0.75f;

        // Path objects: path & rig


        [SerializeField]
        private SkierRig skierRig;
        private Vector3 skierRigVelocity;

        public float friction; // less than 1.0f

        [SerializeField]
        TextMeshProUGUI text;

        XROrigin xrOrigin;

        private float dx, dy, dz, v;
        private string eng;
#if EIR_INTERACTION

        private void Start()
        {
            interactionManager = EIRManager.Instance.Interaction;
            xrOrigin = FindObjectOfType<XROrigin>();
            skierRig = xrOrigin.GetComponent<SkierRig>();
            head = xrOrigin.Camera.transform;
            if (skierRig == null)
                Debug.LogWarning("No Skier Rig found!");
            text.text = "";
            engaging = new bool[2];
            justEngaged = new bool[2];
            justDisengaged = new bool[2];
            skiStickSpikeHands = new InteractingBodyPart[2];
            justEngagedPositions = new Vector3[2];
            skiSpikes = new SkiStickSpike[2];
            justEngagedHeadHeights = new float[2];

        }

        private void Update()
        {
            Engaging();

            for (int i = 0; i < 2; i++)
            {
                // Remember the place, when the spike hit the snow
                if (justEngaged[i] && skiStickSpikeHands[i] != null)
                {
                    justEngagedPositions[i] = skiStickSpikeHands[i].transform.localPosition;
                    justEngagedHeadHeights[i] = head.transform.localPosition.y;
                }
                // If you lost the ski sticks, stop engaging
                if (skiStickSpikeHands[i] != null)
                    if (skiSpikes[i].skiStick.currentlyInteractingBodyPart == null)
                    {
                        engaging[i] = false;
                        interactionManager.ApplyForce(skiStickSpikeHands[i].BodyPart, 0);
                    }
            }
        }

        private void Engaging()
        {
            for (int i = 0; i < 2; i++)
            {
                if (engaging[i])
                {
                    // 1. Calculate force of the object
                    Vector3 force = Vector3.zero;

                    if (skierRig != null && !justEngaged[i])
                    {
                        force = (skiStickSpikeHands[i].transform.localPosition - justEngagedPositions[i]) * EMSmultiplier;
                        Vector3 displacement = (skiStickSpikeHands[i].transform.localPosition - justEngagedPositions[i]);
                        float headDisplacement = Mathf.Clamp((justEngagedHeadHeights[i] - head.transform.localPosition.y), 0, 2.0f);
                        //Debug.Log("Head displacement " + headDisplacement);
                        //Vector3 projection = Vector3.Project(displacement * velocityMultiplier, this.transform.forward);
                        float projection = Vector3.Dot(-displacement * velocityMultiplier, Vector3.forward) * (1 + headDisplacement);
                        projection = Mathf.Clamp(projection, 0, 1000);
                        skierRig.AddProjectionToVelocity(projection, friction);
                    }
                    interactionManager.ApplyForce(skiStickSpikeHands[i].BodyPart, force.magnitude);
                }
            }
            
        }
#endif


        void OnTriggerEnter(Collider collider)
        {
            #if EIR_INTERACTION

            if (collider.GetComponent<SkiStickSpike>() != null)
            {
                int isLeft = collider.GetComponent<SkiStickSpike>().skiStick.currentlyInteractingBodyPart.BodyPart == BodyPart.leftHand ? 0 : 1;
                skiSpikes[isLeft] = collider.GetComponent<SkiStickSpike>();
                skiStickSpikeHands[isLeft] = skiSpikes[isLeft].skiStick.currentlyInteractingBodyPart;
                engaging[isLeft] = true;
                justEngaged[isLeft] = true;
                
                StartCoroutine(JustEngaged(isLeft));
            }
#endif
        }

        void OnTriggerExit(Collider collider)
        {
            #if EIR_INTERACTION

            if (collider.GetComponent<SkiStickSpike>() != null)
            {
                int isLeft = collider.GetComponent<SkiStickSpike>().skiStick.currentlyInteractingBodyPart.BodyPart == BodyPart.leftHand ? 0 : 1;
                SendZeroForce(isLeft);
                skiSpikes[isLeft] = null;
                skiStickSpikeHands[isLeft] = null;
                engaging[isLeft] = false;
                justDisengaged[isLeft] = true;
                StartCoroutine(JustDisengaged(isLeft));
            }
#endif
        }

        private IEnumerator JustEngaged(int isLeft)
        {
            yield return new WaitForEndOfFrame();
            justEngaged[isLeft] = false;
        }

        private IEnumerator JustDisengaged(int isLeft)
        {
            yield return new WaitForEndOfFrame();
            justDisengaged[isLeft] = false;
        }

        public virtual void SendZeroForce(int i)
        {
            #if EIR_INTERACTION

            if (skiStickSpikeHands[i] != null)
                interactionManager.ApplyForce(skiStickSpikeHands[i].BodyPart, 0);
#endif
        }

        private void OnDestroy()
        {
            //SendZeroForce();
        }
        */

    }
}