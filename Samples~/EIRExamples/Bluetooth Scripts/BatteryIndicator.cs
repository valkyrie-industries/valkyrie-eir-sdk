using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if EIR_COMM
using Valkyrie.EIR.Bluetooth;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Manages an output display to visualise the connection and battery state of an EIR device, dependent on the Communication Manager reading the required characteristic.
    /// </summary>
#if EIR_COMM
    public class BatteryIndicator : MonoBehaviour, IEirBluetooth {
#else
    public class BatteryIndicator : MonoBehaviour {
#endif

        #region Serialized Variables

        [SerializeField] private Sprite sprUnknown,sprFull,spr80,spr60,spr40,spr20,sprEmpty;

        [SerializeField] private Image leftIndicator;
        [SerializeField] private Image rightIndicator;

        [SerializeField] private TextMeshProUGUI leftText;
        [SerializeField] private TextMeshProUGUI rightText;

        [SerializeField] private Image panel;

        #endregion

        #region Private Variables

        private bool isActive;
        private bool initialised;

        #endregion

        #region Unity Methods

#if EIR_COMM
        private void Update() {
            if (initialised) return;
            if (EIRManager.Instance == null) return;
            EIRManager.Instance.Communication.RegisterHandler(this);
            SetIndication(false, EIRManager.Instance.Communication.Vitals);
            SetIndication(true, EIRManager.Instance.Communication.Vitals);
            initialised = true;
        }

        private void OnDisable() {
            if (!initialised) return;
            EIRManager.Instance.Communication.UnregisterHandler(this);
            initialised = false;
        }
#endif

        #endregion

        #region Private Methods

#if EIR_COMM
        /// <summary>
        /// Sets the indicator values for each device.
        /// </summary>
        /// <param name="isRight"></param>
        private void SetIndication(bool isRight, DeviceVitals deviceVitals) {
            uint battery = isRight ? deviceVitals.RightBattery : deviceVitals.LeftBattery;
            bool connected = isRight ? deviceVitals.RightConnected : deviceVitals.LeftConnected;
            Image i = isRight ? rightIndicator : leftIndicator;
            i.sprite = GetSprite(connected, battery);
            TextMeshProUGUI t = isRight ? rightText : leftText;
            t.SetText(connected ? $"{battery}%" : "off");
            i.color = connected ? (battery > 20 ? Color.white : Color.red) : Color.gray;
        }

        /// <summary>
        /// Returns the required sprite dependent on the device's battery state and connection state.
        /// </summary>
        /// <param name="connected"></param>
        /// <param name="charge"></param>
        /// <returns></returns>
        private Sprite GetSprite(bool connected, uint charge) {
            if (!connected) return sprUnknown;
            if (charge > 90) return sprFull;
            if (charge > 80) return spr80;
            if (charge > 60) return spr60;
            if (charge > 40) return spr40;
            if (charge > 15) return spr20;
            else return sprEmpty;
        }

        /// <summary>
        /// Disables the indicator if the devices are not in use.
        /// </summary>
        private void Disable() {
            if (!isActive)
                return;
            isActive = false;

            if (leftIndicator.gameObject.activeSelf) leftIndicator.gameObject.SetActive(false);
            if (rightIndicator.gameObject.activeSelf) rightIndicator.gameObject.SetActive(false);

            if (leftText.gameObject.activeSelf) leftText.gameObject.SetActive(false);
            if (rightText.gameObject.activeSelf) rightText.gameObject.SetActive(false);

            if (panel.gameObject.activeSelf) panel.gameObject.SetActive(false);

        }

        /// <summary>
        /// Enables the indicator if the devices are in use.
        /// </summary>
        private void Enable() {
            if (isActive)
                return;
            isActive = true;

            if (!leftIndicator.gameObject.activeSelf) leftIndicator.gameObject.SetActive(true);
            if (!rightIndicator.gameObject.activeSelf) rightIndicator.gameObject.SetActive(true);

            if (!leftText.gameObject.activeSelf) leftText.gameObject.SetActive(true);
            if (!rightText.gameObject.activeSelf) rightText.gameObject.SetActive(true);

            if (!panel.gameObject.activeSelf) panel.gameObject.SetActive(true);

        }
#endif

        #endregion

        #region Interface Implementation

#if EIR_COMM

        public void OnBluetoothEnable() {
            Enable();
        }

        public void OnWrite() {
            // discard.
        }

        public void OnUpdateVitals(DeviceVitals vitals) {
            Enable();
            SetIndication(false, vitals);
            SetIndication(true, vitals);
        }

        public void OnBluetoothDisable() {
            Disable();
        }

#endif

        #endregion
    }
}