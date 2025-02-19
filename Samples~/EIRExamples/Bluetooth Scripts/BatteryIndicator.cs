using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        [SerializeField] private Sprite sprUnknown, sprFull, spr80, spr60, spr40, spr20, sprEmpty;

        [SerializeField] private Image leftIndicator;
        [SerializeField] private Image rightIndicator;

        [SerializeField] private TextMeshProUGUI leftText;
        [SerializeField] private TextMeshProUGUI rightText;

        [SerializeField] private Image panel;
        [SerializeField] private bool recolour;
        [SerializeField] private bool recolourImage = true;
        [SerializeField] private bool recolourText = true;
        [SerializeField] private Color standardColor = Color.white;
        [SerializeField] private Color lowBattColor = Color.red;
        [SerializeField] private Color disconnectedColor = Color.gray;

        [SerializeField] private GameObject lowBatteryNotification;

        [SerializeField] private UnityEvent onLowBattery;
        [SerializeField] private UnityEvent<DeviceVitals> onUpdateVitals;

        #endregion

        #region Private Variables

        private bool initialised;

        #endregion

        #region Unity Methods

#if EIR_COMM
        private void Update() {
            if (initialised) return;
            if (EIRManager.Instance == null) return;
            EIRManager.Instance.EirBluetooth.RegisterHandler(this);
            SetIndication(false, EIRManager.Instance.EirBluetooth.Vitals);
            SetIndication(true, EIRManager.Instance.EirBluetooth.Vitals);
            initialised = true;
        }

        private void OnDisable() {
            if (!initialised) return;
            EIRManager.Instance.EirBluetooth.UnregisterHandler(this);
            initialised = false;
        }
#endif

        #endregion

        #region Private Methods

#if EIR_COMM
        /// <summary>
        /// Sets the indicator values for each device.
        /// </summary>
        /// <param name="isLeft"></param>
        private void SetIndication(bool isLeft, DeviceVitals deviceVitals) {
            uint battery = isLeft ? deviceVitals.LeftBattery : deviceVitals.RightBattery;
            bool connected = isLeft ? deviceVitals.LeftConnected : deviceVitals.RightConnected;
            Image i = isLeft ? leftIndicator : rightIndicator;
            i.sprite = GetSprite(connected, battery);
            TextMeshProUGUI t = isLeft ? leftText : rightText;
            t.SetText(connected ? $"{battery}%" : "off");
            if (recolour) i.color = connected ? (battery > 20 ? standardColor : lowBattColor) : disconnectedColor;
            if (recolourImage) i.color = connected ? (battery > 20 ? standardColor : lowBattColor) : disconnectedColor;
            if (recolourText) t.color = connected ? (battery > 20 ? standardColor : lowBattColor) : disconnectedColor;
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
#endif

        #endregion

        #region Public Methods

        public void DismissLowBatteryNotification() {

            if (lowBatteryNotification != null) {
                lowBatteryNotification.SetActive(false);
            }
        }

        #endregion

        #region Interface Implementation

#if EIR_COMM

        public void OnBluetoothEnable() {
            // discard.
        }

        public void OnWrite() {
            // discard.
        }

        public void OnUpdateVitals(DeviceVitals vitals) {
            SetIndication(false, vitals);
            SetIndication(true, vitals);
            onUpdateVitals.Invoke(vitals);
        }

        public void OnBluetoothDisable() {
            // discard.
        }

        public void OnUpdateVoltages(double[] voltages) {
            // discard.
        }

        public void OnDisconnect() {
            SetIndication(false, new DeviceVitals(false, false, 0, 0, 0, 0));
            SetIndication(true, new DeviceVitals(false, false, 0, 0, 0, 0));
        }

        public void OnLowBatteryDetected() {

            // display the low battery notification, if one is available.
            if (lowBatteryNotification != null) {
                lowBatteryNotification.SetActive(true);
            }
            onLowBattery.Invoke();
        }
#endif

        #endregion
    }
}