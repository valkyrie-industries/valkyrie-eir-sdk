using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples
{
    public class TeleportManager : MonoBehaviour
    {

        #region Private Variables

        private TeleportationAnchor[] anchors;
        private HapticGun[] guns;

        [SerializeField]
        private Button[] buttons;
        private bool[] buttonStates;
        private bool previousFiringState = false;

        private bool lastState = false;

        #endregion

        #region Unity Methods

        private void Start()
        {
            anchors = FindObjectsOfType<TeleportationAnchor>();
            guns = FindObjectsOfType<HapticGun>();
            buttonStates = new bool[buttons.Length];
            ActivateHapticButtons(false);
        }

        /// <summary>
        /// Detect the firing state of each gun, if one is true, disable the anchors. If all are not firing, enable the anchors.
        /// </summary>
        private void LateUpdate()
        {

            bool firing = false;

            if (previousFiringState != true)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttonStates[i] = buttons[i].interactable;
                }
            }

            foreach (HapticGun gun in guns)
            {
                if (gun.Firing)
                {
                    firing = true;
                    break;
                }
            }

            foreach (TeleportationAnchor anchor in anchors)
            {
                anchor.enabled = !firing;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = firing ? false : buttonStates[i];
            }

            previousFiringState = firing;
        }

        public async void ActivateHapticButtons(bool active)
        {
            await Task.Delay(100);
#if EIR_HAPTICS

            if (FindObjectOfType<FeelManager>() && lastState == true)
            {
                FindObjectOfType<FeelManager>().StopPlayingFeeling();
            }

            await Task.Delay(10);

            for (int i = 0; i < buttons.Length; i++)
            {
                Debug.Log("Setting active " + active);
                buttons[i].interactable = active;
            }

            if (FindObjectOfType<ConfigureEIR>() && lastState == true)
            {
                FindObjectOfType<ConfigureEIR>().ConfigureToDefault();
            }
#endif
            lastState = active;
        }

        #endregion

    }
}