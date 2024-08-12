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
    public class HapticGun : KinematicGrabInteractable {
#else
    public class HapticGun : MonoBehaviour {
#endif
        bool firing = false;
        List<IXRSelectInteractor> currentInteractors = new List<IXRSelectInteractor>();
        Rigidbody rig;
#if EIR_HAPTICS
        HapticManager haptics;
#endif

        public bool Firing { get { return firing; } }

        BoxCollider col;
#if EIR_HAPTICS
        FeelManager feel;
        ConfigureEIR conf;
#endif

        private AudioSource audioSource;
        private ParticleSystem particles;

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

        public override void Start() {
            base.Start();
            rig = GetComponent<Rigidbody>();
            if (rig == null)
                Debug.Log("[HapticGun] No Rigidbody found");

            rig.maxAngularVelocity = 20;
#if EIR_HAPTICS
            haptics = EIRManager.Instance.Haptics;
            if (haptics == null)
                Debug.Log("[HapticGun] No HapticManager found");
#endif
            col = GetComponent<BoxCollider>();
            if (col == null)
                Debug.Log("[HapticGun] Box collider missing");
#if EIR_HAPTICS
            feel = FindObjectOfType<FeelManager>();
            if (feel == null)
                Debug.Log("[HapticGun] Feel manager missing");

            conf = FindObjectOfType<ConfigureEIR>();
            if (conf == null)
                Debug.Log("[HapticGun] Configure eir missing");
#endif
        }

        public override void Interacting() {
            base.Interacting();
            if (grabbing) {
                col.enabled = false;
            } else {
                col.enabled = true;
            }
        }

        new public void SelectEntered(SelectEnterEventArgs e) {
            currentInteractors.Add(e.interactorObject);
        }

        new public void SelectExited(SelectExitEventArgs e) {
            if (currentInteractors.Count < 1) {
                Debug.LogError("No current interactors. Should never reach this state");
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

        public void Deactivate(DeactivateEventArgs e) {
            if (currentInteractors.Count < 1) {
                Debug.LogError("No current interactors. Should never reach this state");
                return;
            }

            if (firing && e.interactorObject == currentInteractors[0]) {
                firing = false;
#if EIR_HAPTICS
                conf.ConfigureToDefault();
#endif
            }
        }

        public void Activate(ActivateEventArgs e) {
            if (currentInteractors.Count < 1) {
                Debug.LogError("No current interactors. Should never reach this state");
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
                    StartCoroutine(ContinousFire());
                }
            } else {
#if EIR_HAPTICS
                if (conf != null) {
                    conf.Configure((int)configuration.x, (byte)configuration.y, (byte)configuration.z);
                    ResetConfigurationWithDelay((int)(shotEMSLength * 1000));
                }
                if (string.IsNullOrEmpty(feelID) || feel == null) {
                    HapticPreset preset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity);
                    HapticPreset halfPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity * 0.75f);

                    haptics.CreateHapticPresetRunner(currentInteractors[0].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, preset);

                    if (currentInteractors.Count > 1) {
                        haptics.CreateHapticPresetRunner(currentInteractors[1].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, halfPreset);
                    }
                } else {
                    feel.PlayFeeling(feelID);
                }
#endif

                for (int i = 0; i < shotsPerFire; i++) {
                    PerformShot();
                }
            }
        }

        public async void ResetConfigurationWithDelay(int waitTime) {
            await Task.Delay(waitTime);
#if EIR_HAPTICS
            if (this != null)
                conf.ConfigureToDefault();
#endif
        }

        IEnumerator ContinousFire() {
            while (firing) {
                for (int i = 0; i < shotsPerFire; i++) {
                    PerformShot();
                }
#if EIR_HAPTICS
                HapticPreset preset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity);
                HapticPreset halfPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.value, shotEMSLength, HapticPreset.LoopType.None, intensity * 0.75f);

                haptics.CreateHapticPresetRunner(currentInteractors[0].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, preset);

                if (currentInteractors.Count > 1) {
                    haptics.CreateHapticPresetRunner(currentInteractors[1].transform.GetComponentInParent<InteractingBodyPart>().BodyPart, halfPreset);
                }
#endif
                yield return new WaitForSeconds(shotDelay);

            }

        }

        private void PerformShot() {

            Bullet bullet = Instantiate(bulletPrefab, barrelTip.position, barrelTip.rotation).GetComponent<Bullet>();

            bullet.Rigid.velocity += barrelTip.TransformDirection(Vector3.forward) * shotVelocity
                + (barrelTip.TransformDirection(Vector3.left) * Random.Range(-bulletSpreadMax, bulletSpreadMax))
                + (barrelTip.TransformDirection(Vector3.up) * Random.Range(-bulletSpreadMax, bulletSpreadMax));

            StartCoroutine(DestroyDelayed(bullet.gameObject));
            bullet.transform.SetParent(null);

            // Play audio
            if (audioSource == null) {
                audioSource = this.GetComponent<AudioSource>();

            }
            if (audioSource != null) {
                audioSource.Stop();
                audioSource.PlayOneShot(audioSource.clip);
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            //Inform bullet it is fired
            Bullet bulletScript = bullet.gameObject.GetComponent<Bullet>();
            if (bulletScript != null && bulletsDisappearOnContact)
                bulletScript.OnFired();

            // Play particles
            if (particles == null) particles = this.GetComponentInChildren<ParticleSystem>();
            if (particles != null)
                particles.Play();
        }

        IEnumerator DestroyDelayed(GameObject rb) {
            yield return new WaitForSeconds(3);
            if (rb != null && rb != null)
                Destroy(rb);
        }
    }

}


