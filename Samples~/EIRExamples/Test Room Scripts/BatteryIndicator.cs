using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valkyrie.EIR.Bluetooth;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Manages an output display to visualise the connection and battery state of an EIR device, dependent on the Communication Manager reading the required characteristic.
    /// </summary>
    public class BatteryIndicator : MonoBehaviour, IEirBluetooth {

        #region Serialized Variables

        [SerializeField] private Sprite[] sprites;

        [SerializeField] private Image leftIndicator;
        [SerializeField] private Image rightIndicator;

        [SerializeField] private TextMeshProUGUI leftText;
        [SerializeField] private TextMeshProUGUI rightText;

        [SerializeField] private Image panel;

        #endregion

        #region Private Variables

        private bool isActive;

        #endregion

        #region Unity Methods

        private void OnEnable() {
            EIRManager.Instance.Communication.RegisterHandler(this);
            SetIndication(false, EIRManager.Instance.Communication.Vitals);
            SetIndication(true, EIRManager.Instance.Communication.Vitals);
        }

        private void OnDisable() {
            EIRManager.Instance.Communication.UnregisterHandler(this);
        }

        #endregion

        #region Private Methods

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
            if (!connected) return sprites[5];
            if (charge > 80) return sprites[4];
            if (charge > 60) return sprites[3];
            if (charge > 40) return sprites[2];
            if (charge > 20) return sprites[1];
            else return sprites[0];
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
        #endregion

        #region Interface Implementation

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

        #endregion
    }
}