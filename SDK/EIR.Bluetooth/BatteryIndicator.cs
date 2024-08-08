using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valkyrie.EIR.Bluetooth;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Manages an output display to visualise the connection and battery state of an EIR device, dependent on the Communication Manager reading the required characteristic.
    /// </summary>
    public class BatteryIndicator : MonoBehaviour {

        #region Events

        public static Action<BatteryIndicator> OnBatteryIndicatorAwake;

        #endregion

        #region Serialized Variables

        [SerializeField] private Sprite[] sprites;

        [SerializeField] private Image leftIndicator;
        [SerializeField] private Image rightIndicator;

        [SerializeField] private TextMeshProUGUI leftText;
        [SerializeField] private TextMeshProUGUI rightText;

        [SerializeField] private Image panel;

        [SerializeField] private bool forceOn;
        [SerializeField] private uint devBatteryStatus;
        #endregion

        #region Private Variables

        private EirBluetoothBridge eirBluetoothBridge;
        private bool disabled, enabled;

        private bool initialised;

        #endregion

        #region Unity Methods

        private void Awake() {
            OnBatteryIndicatorAwake?.Invoke(this);
        }

        private void Update() {
#if EIR_COMM
            if (!initialised) return;

            if (!eirBluetoothBridge.IsActive && !forceOn) {
                Disable();
            }
            else {
                Enable();

                SetIndication(false);
                SetIndication(true);
            }
#endif
        }

        #endregion

        #region Public Methpds
#if EIR_COMM
        public void Initialise(EirBluetoothBridge eir) {
            eirBluetoothBridge = eir;
        }
#endif
#endregion

        #region Private Methods

        /// <summary>
        /// Sets the indicator values for each device.
        /// </summary>
        /// <param name="isRight"></param>
        private void SetIndication(bool isRight) {
            uint battery = forceOn ? devBatteryStatus : (isRight ? eirBluetoothBridge.Vitals.RightBattery : eirBluetoothBridge.Vitals.LeftBattery);
            bool connected = forceOn ?  true : (isRight ? eirBluetoothBridge.Vitals.RightConnected : eirBluetoothBridge.Vitals.LeftConnected);
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
            if (disabled)
                return;
            disabled = true;
            enabled = false;
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
            if (enabled)
                return;
            enabled = true;
            disabled = false;
            if (!leftIndicator.gameObject.activeSelf) leftIndicator.gameObject.SetActive(true);
            if (!rightIndicator.gameObject.activeSelf) rightIndicator.gameObject.SetActive(true);

            if (!leftText.gameObject.activeSelf) leftText.gameObject.SetActive(true);
            if (!rightText.gameObject.activeSelf) rightText.gameObject.SetActive(true);

            if (!panel.gameObject.activeSelf) panel.gameObject.SetActive(true);
            
        }

        #endregion
    }
}