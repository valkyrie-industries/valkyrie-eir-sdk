using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif
namespace Valkyrie.EIR.Examples
{
    public class Target : MonoBehaviour
    {
        [SerializeField]
        private bool moving, movingX;

        private Vector3 initPos;
        private Quaternion initRot;
        private float amplitude, period;

        [SerializeField]
        private bool handsOnly;

        [SerializeField]
        private bool rollTarget;

        //private MobileGameControl mobileGameControl;

        [SerializeField]
        private int scoreValue = 1;

        [SerializeField]
        private bool respawnFloating = false;


        // Start is called before the first frame update
        public virtual void Start()
        {
            amplitude = Random.Range(scoreValue * 0.5f, scoreValue * 2f);
            period = Random.Range(2f, 5f);
            initPos = this.transform.position;
            initRot = this.transform.rotation;
            //mobileGameControl = FindAnyObjectByType<MobileGameControl>();
        }

        // Update is called once per frame
        void Update()
        {

            if (moving)
            {
                transform.position = new Vector3(initPos.x, initPos.y + amplitude * Mathf.Sin(Time.time * 2 * Mathf.PI / period), initPos.z);
            }
            else if (movingX)
            {
                transform.position = new Vector3(initPos.x + amplitude * Mathf.Sin(Time.time * 2 * Mathf.PI / period), initPos.y, initPos.z);
            }

            /*
            // Fly Up
            transform.position += new Vector3(0, 0.005f, 0);

            if (transform.position.y > 15)
            {
                //Destroy(gameObject);
                FindObjectOfType<BowAndArrowGame>().DestroyTarget(this);
            }
           */
        }

        public virtual void RestartObject()
        {
            // If collider was off - turn it back on
            if (this.GetComponent<Collider>())
                this.GetComponent<Collider>().enabled = true;

            //Turn back on meshrenderer
            if (GetComponent<MeshRenderer>())
                GetComponent<MeshRenderer>().enabled = true;

            // Re-initialise rotation
            this.transform.rotation = initRot;
            this.transform.position = initPos;
        }

        private void OnTriggerEnter(Collider other)
        {
#if EIR_INTERACTION
            //if (other.name == "arrowMesh")
            //{
            Debug.Log("Collider is " + other);
            if (handsOnly)
            {
                if (other.GetComponent<InteractingBodyPart>() != null)
                {
                    if (other.GetComponent<InteractingBodyPart>().BodyPart == BodyPart.leftHand || other.GetComponent<InteractingBodyPart>().BodyPart == BodyPart.rightHand)
                        ReactToCollision();
                }
            }
            else
            {
                Debug.Log("Target position " + this.transform.position);
                Debug.Log("Bullet position " + other.transform.position);
                Debug.Log("Inverse point " + this.transform.InverseTransformPoint(other.transform.position));

                ReactToCollision(this.transform.InverseTransformPoint(other.transform.position));

            }
            //}
            //if (mobileGameControl == null)
            //    mobileGameControl = FindAnyObjectByType<MobileGameControl>();
            //mobileGameControl.AddScore(scoreValue);
#else
            Debug.Log("[Target] Script requires EIR Interaction to function");
#endif
        }

        public virtual void ReactToCollision(object parameter = null)
        {
            if (rollTarget)
                StartCoroutine(Rolling(parameter));
            else
                StartCoroutine(Explosion());
        }

        IEnumerator Rolling(object parameter = null)
        {
            float angle = this.transform.eulerAngles.x;
            ParticleSystem particles = GetComponent<ParticleSystem>();
            if (particles != null)
            {
                ParticleSystem.ShapeModule shape = particles.shape;
                shape.position = (Vector3)parameter;
                GetComponent<ParticleSystem>().Play();

            }
            GetComponent<AudioSource>().Play();
            while (angle < 89)
            {
                this.transform.rotation = Quaternion.Euler(this.transform.eulerAngles.x + 1, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
                yield return new WaitForEndOfFrame();
                angle = this.transform.eulerAngles.x;
            }
            yield return new WaitForSeconds(3f);
            this.transform.rotation = Quaternion.Euler(0, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
            StopAllCoroutines();
        }


        IEnumerator Explosion()
        {
            GetComponent<ParticleSystem>().Play();
            GetComponent<MeshRenderer>().enabled = false;

            GetComponent<AudioSource>().Play();

            //GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(0.3f);

            if (respawnFloating)
            {
                if (GetComponent<MeshRenderer>())
                    GetComponent<MeshRenderer>().enabled = false;
                if (GetComponent<Collider>())
                    GetComponent<Collider>().enabled = false;

                Invoke("RestartObject", 5);
            }
            else
            {
                Destroy(this.gameObject);
            }


            //FindObjectOfType<BowAndArrowGame>().DestroyTarget(this);
        }

    }
}


    
