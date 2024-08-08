using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Valkyrie.EIR.Interaction {

    /// <summary>
    /// Returns a grab interactable back to its home location.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ReturnAfterGrabbing : MonoBehaviour {

        [SerializeField]
        private GameObject homeSpot;
        [SerializeField]
        private bool notRigidBodyControlled;
        [SerializeField]
        private float allowedDistance = 0.5f;

        private XRGrabInteractable xRGrabInteractable;
        private Rigidbody rig;
        private ParticleSystem part;

        private Vector3 homePosition;
        private Quaternion homeRotation;

        private int initLayer;
        private bool grabbed = false;
        private bool flaggedForReturn;
        private bool checkPosition;
        private bool initialIsKinematic;
        private bool initialUseGravity;

        public delegate void EventHandler(float distanceFromHome);
        public static event EventHandler ReturnedToOrigin;

        private void OnEnable() {
            checkPosition = true;
            StartCoroutine(CheckAndMovePosition());
        }

        private void Start() {

            SetReferences();

            xRGrabInteractable.selectExited.AddListener(SelectExited);
            xRGrabInteractable.selectEntered.AddListener(SelectEntered);

            initialIsKinematic = rig.isKinematic;
            initialUseGravity = rig.useGravity;
            initLayer = this.gameObject.layer;
            if (homeSpot == null) {
                homePosition = transform.position;
                homeRotation = transform.rotation;
            }
        }

        public UnityEvent OnReturn;

        private void OnDisable() {
            checkPosition = false;
        }

        /// <summary>
        /// Set the grab interactable rigidbody kinematic.
        /// </summary>
        /// <param name="isKinematic"></param>
        public void SetInitialIsKinematic(bool isKinematic) {
            initialIsKinematic = isKinematic;
        }

        /// <summary>
        /// Set the grab interactable rigdbody to use gravity.
        /// </summary>
        /// <param name="useGravity"></param>
        public void SetInitialUseGravity(bool useGravity) {
            initialUseGravity = useGravity;
        }

        /// <summary>
        /// Return the grab interactable back to its origin position.
        /// </summary>
        public void ReturnToOrigin() {
            //If it is grabbed, we need to cancel the return instantly
            if (grabbed) {
                flaggedForReturn = false;
                return;
            }

            float distanceFromHome = Vector3.Distance(homeSpot == null ? homePosition : homeSpot.transform.position, Vector3.zero);
            ReturnedToOrigin?.Invoke(distanceFromHome);

            gameObject.layer = initLayer;

            if (notRigidBodyControlled) {
                rig.transform.SetPositionAndRotation(homeSpot == null ? homePosition : homeSpot.transform.position, homeSpot == null ? homeRotation : homeSpot.transform.rotation);
            }
            else {
                rig.MovePosition(homeSpot == null ? homePosition : homeSpot.transform.position);
                rig.MoveRotation(homeSpot == null ? homeRotation : homeSpot.transform.rotation);
            }

            transform.position = homeSpot == null ? homePosition : homeSpot.transform.position;

            // if there is a particle system attached, play it now.
            if (part != null) part.Play();

            rig.velocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
            rig.isKinematic = initialIsKinematic;
            rig.useGravity = initialUseGravity;

            StartCoroutine(PlayParticlesAfterTeleport());

            // if it's not grabbed we need to cancel the return after we move it. If we don't do this after moving, it gets flagged twice
            flaggedForReturn = false;

            OnReturn?.Invoke();
        }

        /// <summary>
        /// check whether the gameobject has the requisite components, and throw an exception if not.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        private void SetReferences() {
            if (GetComponent<XRGrabInteractable>() == null) {
                throw new System.NullReferenceException($"[EIR.Interaction.ReturnAfterGrabbing] No XRGrabInteractable found on object {gameObject.name}. Please attach an XRGrabInteractable.");
            }
            else xRGrabInteractable = GetComponent<XRGrabInteractable>();
            if (GetComponent<Rigidbody>() == null) {
                throw new System.NullReferenceException($"[EIR.Interaction.ReturnAfterGrabbing] No RigidBody found on object {gameObject.name}. Please attach a RigidBody.");
            }
            else rig = GetComponent<Rigidbody>();
            if (gameObject.GetComponent<ParticleSystem>()) {
                part = gameObject.gameObject.GetComponent<ParticleSystem>();
            }
        }

        private void SelectExited(SelectExitEventArgs args) {
            grabbed = false;
        }

        private void SelectEntered(SelectEnterEventArgs args) {
            if (args.interactorObject.transform.GetComponentInParent<InteractingBodyPart>() != null) {
                grabbed = true;
            }
        }

        private async void ReturnToOriginAsync() {
            await Task.Delay(2000);
            ReturnToOrigin();
        }

        private IEnumerator PlayParticlesAfterTeleport() {
            yield return new WaitForEndOfFrame();

            if (part != null) part.Play();
        }

        private IEnumerator CheckAndMovePosition() {
            do {
                yield return new WaitForEndOfFrame();


                if (Vector3.Distance(transform.position, homeSpot == null ? homePosition : homeSpot.transform.position) > allowedDistance && !flaggedForReturn) {
                    flaggedForReturn = true;
                    ReturnToOriginAsync();
                }
                if (!checkPosition) break;
            }
            while (true);
        }
    }
}