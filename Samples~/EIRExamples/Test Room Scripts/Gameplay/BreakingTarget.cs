using System.Collections;
using UnityEngine;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example breakable target object which generates a haptic force when destroyed.
    /// </summary>
    public class BreakingTarget : Target {

        #region Serialized Variables

        [SerializeField]
        private MeshRenderer meshRenderer;
        [SerializeField]
        private bool restartAfterBreaking;
        [SerializeField]
        private bool doEMSHaptics = false;

        #endregion

        #region Private Variables

        private Rigidbody[] parts;
        private Vector3[] initPosArray;
        private Quaternion[] initRotArray;
        private IEnumerator coroutine;

        #endregion

        #region Unity Methods

        public override void Start() {
            base.Start();
            if (meshRenderer == null)
                Debug.LogWarning("[Breaking Target] No meshrenderer found on this object", this);

            parts = GetComponentsInChildren<Rigidbody>(true);
            initPosArray = new Vector3[parts.Length];
            initRotArray = new Quaternion[parts.Length];
            for (int i = 1; i < parts.Length; i++) {
                initPosArray[i] = parts[i].transform.localPosition;
                initRotArray[i] = parts[i].transform.localRotation;
            }
        }

        #endregion

        #region Public Methods

        public override void ReactToCollision(object parameter = null) {
            Material mat = meshRenderer.material;
            Debug.Log("[Breaking Target] Reacting to collision, exploding!");
            if (GetComponent<AudioSource>() != null)
                GetComponent<AudioSource>().Play();
            meshRenderer.enabled = false;
            this.GetComponent<Collider>().enabled = false;
            for (int i = 1; i < parts.Length; i++) {
                parts[i].gameObject.SetActive(true);
                parts[i].transform.SetParent(null);
                parts[i].AddForce(Vector3.forward * 2, ForceMode.Impulse);
            }

#if EIR_HAPTICS
            // if required, immediately apply haptic intensity to both hands.
            if (doEMSHaptics) {
                if (EIRManager.Instance.Haptics != null) {
                    EIRManager.Instance.Haptics.AddHapticIntensity((int)BodyPart.leftHand, 1, false);
                    EIRManager.Instance.Haptics.AddHapticIntensity((int)BodyPart.rightHand, 1, false);
                }
            }
#endif

            // give the coroutine to an object we know will be present so that it always deactivates (even without other spawns)
            StartCoroutine(coroutine = DeactivateAfterDelay(3));
        }

        /// <summary>
        /// Reset the target to its initial state.
        /// </summary>
        public override void RestartObject() {
            base.RestartObject();

            // for exploding objects, turn the meshrenderers back before deactivating the gameobjects
            MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in renderers)
                mr.enabled = true;

            // Reset any other properties or components of the object if needed
            if (parts == null)
                return;

            for (int i = 1; i < parts.Length; i++) {
                parts[i].transform.SetParent(this.transform);
                parts[i].transform.localPosition = initPosArray[i];
                parts[i].transform.localRotation = initRotArray[i];
                parts[i].gameObject.SetActive(false);
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator DeactivateAfterDelay(float delay) {
            float timeDelay = 0;
            while (timeDelay < delay) {
                yield return new WaitForEndOfFrame();
                timeDelay += Time.deltaTime;
            }
            Deactivate();
        }

        private void Deactivate() {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            // for exploding objects, turn the meshrenderers back before deactivating the gameobjects
            MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in renderers)
                mr.enabled = true;
            StopAllCoroutines(); // If anything runs on this behaviour - stop it

            //base.Deactivate();
            // Reset any other properties or components of the object if needed
            for (int i = 1; i < parts.Length; i++) {
                if (gameObject.activeInHierarchy)
                    parts[i].transform.SetParent(this.transform);
                parts[i].transform.localPosition = initPosArray[i];
                parts[i].transform.localRotation = initRotArray[i];
                parts[i].gameObject.SetActive(false);
            }

            if (restartAfterBreaking) {
                meshRenderer.enabled = true;
                if (this.GetComponent<Collider>())
                    this.GetComponent<Collider>().enabled = true;
            } else {
                if (gameObject.activeInHierarchy)
                    this.gameObject.SetActive(false);
            }

        }

        private IEnumerator RestartCoroutine() {
            yield return new WaitForSeconds(5);
            RestartObject();
        }

        #endregion
    }
}


