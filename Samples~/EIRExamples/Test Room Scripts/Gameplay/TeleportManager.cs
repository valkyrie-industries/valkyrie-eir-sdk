using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Process teleportation around the Valkyrie EIR Example Room.
    /// Also enables/disables the Haptic preset buttons when teleporting, or additionally when firing the Haptic Gun.
    /// </summary>
    public class TeleportManager : MonoBehaviour {

        #region Serialized Variables

        [SerializeField]
        private Button[] buttons;

        #endregion

        #region Private Variables

        private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor[] anchors;
        private HapticGun[] guns;

        private bool[] buttonStates;
        private bool previousFiringState = false;

        private bool lastState = false;

        #endregion

        #region Unity Methods

        private void Start() {
            anchors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>();
            guns = FindObjectsOfType<HapticGun>();
            buttonStates = new bool[buttons.Length];
            ActivateHapticButtons(false);
        }

        /// <summary>
        /// Detect the firing state of each gun, if one is true, disable the anchors. If all are not firing, enable the anchors.
        /// </summary>
        private void LateUpdate() {

            bool firing = false;

            if (previousFiringState != true) {
                for (int i = 0; i < buttons.Length; i++) {
                    buttonStates[i] = buttons[i].interactable;
                }
            }

            foreach (HapticGun gun in guns) {
                if (gun.Firing) {
                    firing = true;
                    break;
                }
            }

            foreach (UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor anchor in anchors) {
                anchor.enabled = !firing;
            }

            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].interactable = firing ? false : buttonStates[i];
            }

            previousFiringState = firing;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Activates the haptic buttons. These should be disabled during teleportation and re-enabled when the teleportation process has ended.
        /// </summary>
        /// <param name="active"></param>
        public async void ActivateHapticButtons(bool active) {
            await Task.Delay(100);
#if EIR_HAPTICS

            if (FindObjectOfType<FeelManager>() && lastState == true) {
                FindObjectOfType<FeelManager>().StopPlayingFeeling();
            }

            await Task.Delay(10);

            for (int i = 0; i < buttons.Length; i++) {
                Debug.Log($"[Teleport Manager] Setting Haptic Buttons {(active ? "active" : "inactive")}.");
                buttons[i].interactable = active;
            }

            if (FindObjectOfType<ConfigureEIR>() && lastState == true) {
                FindObjectOfType<ConfigureEIR>().ConfigureToDefault();
            }
#endif
            lastState = active;
        }

        #endregion

    }
}