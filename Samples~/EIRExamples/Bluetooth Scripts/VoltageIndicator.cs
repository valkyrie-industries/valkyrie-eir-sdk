using TMPro;
using UnityEngine;
#if EIR_COMM
using Valkyrie.EIR.Bluetooth;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Manages an output display to visualise the voltage output of an EIR device, dependent on the Communication Manager reading the required characteristic.
    /// </summary>
    public class VoltageIndicator : MonoBehaviour, IEirBluetooth {

        #region Serialized Variables

        [SerializeField] private TextMeshProUGUI textA;
        [SerializeField] private TextMeshProUGUI textB;

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
            EIRManager.Instance.EirBluetooth.RegisterHandler(this);
            SetIndication(0, 0);
            initialised = true;
        }

        private void OnDisable() {
            if (!initialised) return;
            EIRManager.Instance.EirBluetooth.UnregisterHandler(this);
            initialised = false;
            SetIndication(0, 0);
        }
#endif

        #endregion

        #region Private Methods

#if EIR_COMM

        /// <summary>
        /// Sets the indicator values for each device.
        /// </summary>
        /// <param name="isLeft"></param>
        private void SetIndication(double voltageA, double voltageB) {
            textA.text = voltageA.ToString() + " V";
            textB.text = voltageB.ToString() + " V";
        }

        /// <summary>
        /// Disables the indicator if the devices are not in use.
        /// </summary>
        private void Disable() {
            if (!isActive)
                return;
            isActive = false;

            if (textA.gameObject.activeSelf) textA.gameObject.SetActive(false);
            if (textB.gameObject.activeSelf) textB.gameObject.SetActive(false);

        }

        /// <summary>
        /// Enables the indicator if the devices are in use.
        /// </summary>
        private void Enable() {
            if (isActive)
                return;
            isActive = true;

            if (!textA.gameObject.activeSelf) textA.gameObject.SetActive(true);
            if (!textB.gameObject.activeSelf) textB.gameObject.SetActive(true);

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
            // discard
        }

        public void OnBluetoothDisable() {
            Disable();
        }

        public void OnUpdateVoltages(double[] outputVoltages) {
            Enable();
            SetIndication(outputVoltages[0], outputVoltages[1]);
        }

        public void OnDisconnect() {
            SetIndication(0, 0);
        }

        public void OnLowBatteryDetected() {
            // do nothing.
        }
#endif

        #endregion
    }
}