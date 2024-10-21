using UnityEngine;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Bullet projectile for HapticGun example.
    /// </summary>
    public class Bullet : MonoBehaviour {

        #region Public Properties

        /// <summary>
        /// Returns the bullet's rigidbody component.
        /// </summary>
        public Rigidbody Rigid { get { return rb; } }

        #endregion

        #region Serialized Variables

        [SerializeField]
        private bool outputDebugLog = false;
        [SerializeField]
        private Rigidbody rb;

        #endregion

        #region Private Variables

        private bool fired = false;

        private float fireTime = 0;
        private float colliderWait = 0.3f;

        private Collider col;

        #endregion

        #region Unity Methods

        public void Update() {
            if (col != null && fireTime + colliderWait < Time.time) {
                col.enabled = true;
            }
        }

        public void OnCollisionEnter(Collision collision) {
            if (outputDebugLog) Debug.Log($"[Bullet] Collision detected with {collision.gameObject.name}");
            if (fired && fireTime + colliderWait < Time.time)
                Destroy(gameObject);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Marks the bullet as fired.
        /// </summary>
        public void Fire() {
            fireTime = Time.time;
            fired = true;
            col = GetComponent<Collider>();
            col.enabled = false;
        }

        #endregion

    }
}


