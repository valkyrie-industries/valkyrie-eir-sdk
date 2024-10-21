using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example target object which generates a haptic force when struck.
    /// </summary>
    public class Target : MonoBehaviour {

        #region Serialized Variables

        [SerializeField]
        private bool moving, movingX;
        [SerializeField]
        private bool handsOnly;
        [SerializeField]
        private bool rollTarget;
        [SerializeField]
        private int scoreValue = 1;
        [SerializeField]
        private bool respawnFloating = false;
        [SerializeField]
        private bool outputDebugLog;

        #endregion

        #region Private Variables

        private Vector3 initPos;
        private Quaternion initRot;
        private float amplitude, period;

        #endregion

        #region Unity Methods

        public virtual void Start() {
            amplitude = Random.Range(scoreValue * 0.5f, scoreValue * 2f);
            period = Random.Range(2f, 5f);
            initPos = transform.position;
            initRot = transform.rotation;
        }

        private void Update() {

            if (moving) {
                transform.position = new Vector3(initPos.x, initPos.y + amplitude * Mathf.Sin(Time.time * 2 * Mathf.PI / period), initPos.z);
            } else if (movingX) {
                transform.position = new Vector3(initPos.x + amplitude * Mathf.Sin(Time.time * 2 * Mathf.PI / period), initPos.y, initPos.z);
            }
        }

        private void OnTriggerEnter(Collider other) {
#if EIR_INTERACTION
            Debug.Log($"[Target] OnTriggerEnter with: {other}");
            if (handsOnly) {
                if (other.GetComponent<InteractingBodyPart>() != null) {
                    if (other.GetComponent<InteractingBodyPart>().BodyPart == BodyPart.leftHand || other.GetComponent<InteractingBodyPart>().BodyPart == BodyPart.rightHand)
                        ReactToCollision();
                }
            } else {
                if (outputDebugLog) {
                    Debug.Log($"[Target] Target position: {transform.position}. Bullet position: {other.transform.position}. Inverse point: {transform.InverseTransformPoint(other.transform.position)}");
                }
                ReactToCollision(transform.InverseTransformPoint(other.transform.position));

            }
#else
            Debug.Log("[Target] Script requires EIR Interaction to function");
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the target to its original configuration, position and orientation.
        /// </summary>
        public virtual void RestartObject() {
            // If collider was off - turn it back on
            if (GetComponent<Collider>()) GetComponent<Collider>().enabled = true;

            //Turn back on meshrenderer
            if (GetComponent<MeshRenderer>())
                GetComponent<MeshRenderer>().enabled = true;

            // Re-initialise rotation
            transform.rotation = initRot;
            transform.position = initPos;
        }

        /// <summary>
        /// Process the target's response mechanism, either make it roll or make it explode.
        /// </summary>
        /// <param name="parameter"></param>
        public virtual void ReactToCollision(object parameter = null) {
            if (rollTarget) StartCoroutine(RollTarget(parameter));
            else ExplodeTarget();
        }

        #endregion

        #region Private Methods

        private IEnumerator RollTarget(object parameter = null) {
            float angle = transform.eulerAngles.x;
            ParticleSystem particles = GetComponent<ParticleSystem>();
            if (particles != null) {
                ParticleSystem.ShapeModule shape = particles.shape;
                shape.position = (Vector3)parameter;
                GetComponent<ParticleSystem>().Play();

            }
            GetComponent<AudioSource>().Play();
            while (angle < 89) {
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x + 1, transform.eulerAngles.y, transform.eulerAngles.z);
                yield return new WaitForEndOfFrame();
                angle = transform.eulerAngles.x;
            }
            yield return new WaitForSeconds(3f);
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
            StopAllCoroutines();
        }


        private async void ExplodeTarget() {
            GetComponent<ParticleSystem>().Play();
            GetComponent<MeshRenderer>().enabled = false;

            GetComponent<AudioSource>().Play();

            await Task.Delay(300);

            if (respawnFloating) {
                if (GetComponent<MeshRenderer>())
                    GetComponent<MeshRenderer>().enabled = false;
                if (GetComponent<Collider>())
                    GetComponent<Collider>().enabled = false;

                await Task.Delay(5000);
                RestartObject();
            } else {
                Destroy(gameObject);
            }
        }

        #endregion

    }
}



