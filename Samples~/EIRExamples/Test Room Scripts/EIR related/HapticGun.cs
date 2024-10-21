using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
using Valkyrie.EIR.Interaction.Interactables;
#endif
using UnityEngine.XR.Interaction.Toolkit;
using System.Threading.Tasks;

namespace Valkyrie.EIR.Examples {

#if EIR_INTERACTION

    /// <summary>
    /// Example gun that applies EMS force when fired.
    /// </summary>
    public class HapticGun : KinematicGrabInteractable {
#else
    public class HapticGun : MonoBehaviour {
#endif

        #region Serialized Variables

        [Space]
        [Header("Haptic Gun")]
        [Space]
        [Header("Scene Assignments")]
        [SerializeField]
        private Transform barrelTip;
        [SerializeField]
        private Rigidbody bulletPrefab;
        [SerializeField]
        private string feelID = "";
        [Header("Shooting")]
        [SerializeField]
        private bool rapidFire = false;
        [SerializeField]
        private float bulletSpreadMax = 0;
        [SerializeField]
        private int shotsPerFire = 1;
        [SerializeField]
        private float shotDelay = 0.1f;
        [SerializeField]
        private float shotVelocity = 100;
        [Range(0, 3)]
        [SerializeField]
        float minPitch = 1;
        [Range(0, 3)]
        [SerializeField]
        float maxPitch = 1;
        [Header("EMS Output")]
        [SerializeField]
        private float intensity = 1.0f;
        [SerializeField]
        private float shotEMSLength = 0.1f;
        [SerializeField]
        private Vector3 configuration = new Vector3(0, 100, 100);
        [SerializeField]
        private bool bulletsDisappearOnContact = true;

        #endregion

        #region Private Variables

        private bool firing = false;
        private List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor> currentInteractors = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor>();
        private Rigidbody rig;
        private AudioSource audioSource;
        private ParticleSystem particles;
        private BoxCollider col;

#if EIR_HAPTICS
        private ConfigureEIR conf;
#endif

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns true if the gun is actively firing.
        /// </summary>
        public bool Firing { get { return firing; } }

        #endregion

        #region Unity Methods


#if EIR_INTERACTION
        protected override void Start() {
            base.Start();
#else
        public void Start() {
#endif
            rig = GetComponent<Rigidbody>();
            if (rig == null) Debug.LogWarning("[HapticGun] No Rigidbody found");

            rig.maxAngularVelocity = 20;
            col = GetComponent<BoxCollider>();
            if (col == null) Debug.LogWarning("[HapticGun] Box Collider missing");
#if EIR_HAPTICS
            conf = FindObjectOfType<ConfigureEIR>();
            if (conf == null) Debug.LogWarning("[HapticGun] ConfigureEir missing");
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add the newly selected interactor to the list of current interactors.
        /// </summary>
        /// <param name="e"></param>
        new public void SelectEntered(SelectEnterEventArgs e) {
            currentInteractors.Add(e.interactorObject);
        }

        /// <summary>
        /// Remove the deselected interactor from the list of current interactors.
        /// </summary>
        /// <param name="e"></param>
        new public void SelectExited(SelectExitEventArgs e) {
            if (currentInteractors.Count < 1) {
                Debug.LogError("[HapticGun] No current interactors. This should never reach this state.");
                return;
            }

            if (currentInteractors[0] == e.interactorObject && firing) {
                firing = false;
#if EIR_HAPTICS
                if (conf != null)
                    conf.ConfigureToDefault();
#endif
            }

            currentInteractors.Remove(e.interactorObject);
        }

        /// <summary>
        /// Activate the gun, and begin firing if on rapid fire, or fire one bullet if not.
        /// </summary>
        /// <param name="e"></param>
        public void Activate(ActivateEventArgs e) {
            if (currentInteractors.Count < 1) {
                Debug.LogError("[HapticGun] No current interactors. This should never reach this state");
                return;
            }

            //Don't fire if we aren't the trigger hand
            if (e.interactorObject != currentInteractors[0])
                return;

            if (rapidFire) {
                if (!firing && e.interactorObject == currentInteractors[0]) {
                    firing = true;
#if EIR_HAPTICS
                    if (conf != null)
                        conf.Configure((int)configuration.x, (byte)configuration.y, (byte)configuration.z);
#endif
                    StartCoroutine(ProcessContinuousFire());
                }
            } else {
#if EIR_HAPTICS && EIR_INTERACTION
                if (conf != null) {
                    conf.Configure((int)configuration.x, (byte)configuration.y, (byte)configuration.z);
                    ResetConfigurationWithDelay((int)(shotEMSLength * 1000));
                }
                if (string.IsNullOrEmpty(feelID) || FeelManager.Instance == null) {
                    HapticPreset preset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity);
                    HapticPreset halfPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity * 0.75f);

                    EIRManager.Instance.Haptics.CreateHapticPresetRunner(currentInteractors[0].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, preset);

                    if (currentInteractors.Count > 1) {
                        EIRManager.Instance.Haptics.CreateHapticPresetRunner(currentInteractors[1].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, halfPreset);
                    }
                } else {
                    FeelManager.Instance.PlayFeeling(feelID);
                }
#endif

                for (int i = 0; i < shotsPerFire; i++) {
                    PerformShot();
                }
            }
        }

        /// <summary>
        /// Deactivate the gun, resetting it to its default state.
        /// </summary>
        /// <param name="e"></param>
        public void Deactivate(DeactivateEventArgs e) {
            if (currentInteractors.Count < 1) {
                Debug.LogError("[HapticGun] No current interactors. This should never reach this state");
                return;
            }

            if (firing && e.interactorObject == currentInteractors[0]) {
                firing = false;
#if EIR_HAPTICS
                conf.ConfigureToDefault();
#endif
            }
        }

        /// <summary>
        /// Resets the gun to its default configuration after the input delay (ms).
        /// </summary>
        /// <param name="waitTime"></param>
        public async void ResetConfigurationWithDelay(int waitTime) {
            await Task.Delay(waitTime);
#if EIR_HAPTICS
            if (this != null)
                conf.ConfigureToDefault();
#endif
        }

        #endregion

        #region Private Methods

#if EIR_INTERACTION
        protected override void Interacting() {
            base.Interacting();
            if (grabbing) {
                col.enabled = false;
            } else {
                col.enabled = true;
            }
#else
        public void Interacting() {
#endif
        }

        /// <summary>
        /// Processes the continuous generation, configuration and firing of bullets.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProcessContinuousFire() {
            while (firing) {
                for (int i = 0; i < shotsPerFire; i++) {
                    PerformShot();
                }
#if EIR_HAPTICS && EIR_INTERACTION
                HapticPreset preset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity);
                HapticPreset halfPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity * 0.75f);

                EIRManager.Instance.Haptics.CreateHapticPresetRunner(currentInteractors[0].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, preset);

                if (currentInteractors.Count > 1) {
                    EIRManager.Instance.Haptics.CreateHapticPresetRunner(currentInteractors[1].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, halfPreset);
                }
#endif
                yield return new WaitForSeconds(shotDelay);

            }

        }

        /// <summary>
        /// Configures and fires a bullet.
        /// </summary>
        private void PerformShot() {

            Bullet bullet = Instantiate(bulletPrefab, barrelTip.position, barrelTip.rotation).GetComponent<Bullet>();

            bullet.Rigid.velocity += barrelTip.TransformDirection(Vector3.forward) * shotVelocity
                + (barrelTip.TransformDirection(Vector3.left) * Random.Range(-bulletSpreadMax, bulletSpreadMax))
                + (barrelTip.TransformDirection(Vector3.up) * Random.Range(-bulletSpreadMax, bulletSpreadMax));

            StartCoroutine(DestroyBulletAfterDelay(bullet.gameObject));
            bullet.transform.SetParent(null);

            // play audio
            if (audioSource == null) {
                audioSource = this.GetComponent<AudioSource>();

            }
            if (audioSource != null) {
                audioSource.Stop();
                audioSource.PlayOneShot(audioSource.clip);
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            // inform bullet it is fired
            Bullet bulletScript = bullet.gameObject.GetComponent<Bullet>();
            if (bulletScript != null && bulletsDisappearOnContact) bulletScript.Fire();

            // play particles
            if (particles == null) particles = this.GetComponentInChildren<ParticleSystem>();
            if (particles != null) particles.Play();
        }

        /// <summary>
        /// Destroys the bullet gameobject after 3 seconds.
        /// </summary>
        /// <param name="bulletObject"></param>
        /// <returns></returns>
        private IEnumerator DestroyBulletAfterDelay(GameObject bulletObject) {
            yield return new WaitForSeconds(3);
            if (bulletObject != null && bulletObject != null)
                Destroy(bulletObject);
        }

        #endregion
    }

}


