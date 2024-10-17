using System.Threading.Tasks;
using UnityEngine;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Arrow projectile for EMS Bow example.
    /// </summary>
    public class Arrow : MonoBehaviour {

        #region Public Properties

        public Transform ArrowBody { get { return arrowBody; } }

        #endregion

        #region Private Variables

        private Transform arrowBody;

        private Collider col;
        private Rigidbody rb;

        private bool inFlight = false;

        private float returnAfter = 5;
        private float flyStartTime;

        private float waitBeforeCollide = 10f;

        #endregion

        #region Unity Methods

        private void Start() {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }

        private void LateUpdate() {
            if (inFlight && rb != null && arrowBody != null) {
                arrowBody.LookAt(transform.position + rb.velocity);
            }

            if (Time.time + waitBeforeCollide < Time.time && inFlight) {
                if (col != null)
                    col.enabled = true;
            }

            if (Time.time - flyStartTime > returnAfter && !rb.isKinematic && inFlight) {
                EndFlight();
            }
        }

        private void OnCollisionEnter(Collision collision) {
            Debug.Log($"[Arrow] Hit Detected. Collider: {collision.gameObject.name}");

            if (flyStartTime + waitBeforeCollide < Time.time)
                EndFlight();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loose the arrow.
        /// </summary>
        public void BeginFlight() {
            inFlight = true;
            flyStartTime = Time.time;
            col.enabled = false;
        }

        #endregion

        #region Private Methods

        private void EndFlight() {
            inFlight = false;
#if EIR_INTERACTION
            if (GetComponent<ReturnAfterGrabbing>() != null)
                GetComponent<ReturnAfterGrabbing>().ReturnToOrigin();
#endif
            gameObject.layer = 0;
            col.enabled = true;

            ResetRotation();
        }

        private async void ResetRotation() {
            await Task.Delay(300);
            arrowBody.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }

        #endregion
    }
}