using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif
namespace Valkyrie.EIR.Examples {
    public class Target : MonoBehaviour {
        [SerializeField]
        private bool moving, movingX;

        private Vector3 initPos;
        private Quaternion initRot;
        private float amplitude, period;

        [SerializeField]
        private bool handsOnly;

        [SerializeField]
        private bool rollTarget;

        [SerializeField]
        private int scoreValue = 1;

        [SerializeField]
        private bool respawnFloating = false;


        public virtual void Start() {
            amplitude = Random.Range(scoreValue * 0.5f, scoreValue * 2f);
            period = Random.Range(2f, 5f);
            initPos = this.transform.position;
            initRot = this.transform.rotation;
        }

        private void Update() {

            if (moving) {
                transform.position = new Vector3(initPos.x, initPos.y + amplitude * Mathf.Sin(Time.time * 2 * Mathf.PI / period), initPos.z);
            }
            else if (movingX) {
                transform.position = new Vector3(initPos.x + amplitude * Mathf.Sin(Time.time * 2 * Mathf.PI / period), initPos.y, initPos.z);
            }
        }

        public virtual void RestartObject() {
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

        private void OnTriggerEnter(Collider other) {
#if EIR_INTERACTION
            Debug.Log($"[Target] OnTriggerEnter with: {other}");
            if (handsOnly) {
                if (other.GetComponent<InteractingBodyPart>() != null) {
                    if (other.GetComponent<InteractingBodyPart>().BodyPart == BodyPart.leftHand || other.GetComponent<InteractingBodyPart>().BodyPart == BodyPart.rightHand)
                        ReactToCollision();
                }
            }
            else {
                Debug.Log($"[Target] Target position: {this.transform.position}");
                Debug.Log($"[Target] Bullet position: {other.transform.position}");
                Debug.Log($"[Target] Inverse point: {this.transform.InverseTransformPoint(other.transform.position)}");

                ReactToCollision(this.transform.InverseTransformPoint(other.transform.position));

            }
#else
            Debug.Log("[Target] Script requires EIR Interaction to function");
#endif
        }

        public virtual void ReactToCollision(object parameter = null) {
            if (rollTarget)
                StartCoroutine(Rolling(parameter));
            else
                StartCoroutine(Explosion());
        }

        private IEnumerator Rolling(object parameter = null) {
            float angle = this.transform.eulerAngles.x;
            ParticleSystem particles = GetComponent<ParticleSystem>();
            if (particles != null) {
                ParticleSystem.ShapeModule shape = particles.shape;
                shape.position = (Vector3)parameter;
                GetComponent<ParticleSystem>().Play();

            }
            GetComponent<AudioSource>().Play();
            while (angle < 89) {
                this.transform.rotation = Quaternion.Euler(this.transform.eulerAngles.x + 1, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
                yield return new WaitForEndOfFrame();
                angle = this.transform.eulerAngles.x;
            }
            yield return new WaitForSeconds(3f);
            this.transform.rotation = Quaternion.Euler(0, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
            StopAllCoroutines();
        }


        private IEnumerator Explosion() {
            GetComponent<ParticleSystem>().Play();
            GetComponent<MeshRenderer>().enabled = false;

            GetComponent<AudioSource>().Play();

            yield return new WaitForSeconds(0.3f);

            if (respawnFloating) {
                if (GetComponent<MeshRenderer>())
                    GetComponent<MeshRenderer>().enabled = false;
                if (GetComponent<Collider>())
                    GetComponent<Collider>().enabled = false;

                Invoke("RestartObject", 5);
            }
            else {
                Destroy(this.gameObject);
            }


        }

    }
}



