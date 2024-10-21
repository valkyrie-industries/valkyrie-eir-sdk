using UnityEngine;
using UnityEngine.UI;
#if EIR_COMM
using Valkyrie.EIR.Bluetooth;
#endif
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example menu implementation for Connection and Calibration.
    /// </summary>
    public class MenuExample : MonoBehaviour {

        #region Public Methods (EMS)

        /// <summary>
        /// Increment calibration for either the left or right EIR device.
        /// </summary>
        /// <param name="isLeft"></param>
        public void Increment(bool isLeft) {
            UpdateCalibration(isLeft, true);
        }

        /// <summary>
        /// Decrement calibration for either the left or right EIR device.
        /// </summary>
        /// <param name="isLeft"></param>
        public void Decrement(bool isLeft) {
            UpdateCalibration(isLeft, false);
        }

        /// <summary>
        /// Enable or disable the write characteristic.
        /// </summary>
        /// <param name="activate"></param>
        public void ToggleEMS(bool activate) {
#if EIR_COMM && EIR_HAPTICS
            EIRManager.Instance.ToggleBluetoothSend(!EIRManager.Instance.Communication.IsActive);
#endif
        }

        #endregion

        #region Public Methods (Connectivity)

        /// <summary>
        /// Connect or disconnect to a bluetooth device and update the input text object with the connection status.
        /// </summary>
        /// <param name="text"></param>
        public void Connect(Text text) {
#if EIR_COMM
            if (!EIRManager.Instance.Communication.IsConnected) {
                text.text = "Connecting";
                ConnectAsync(text);
            } else {
                EIRManager.Instance.Communication.Disconnect();
                text.text = "Not Connected";
            }
#endif
        }

        #endregion

        #region Private Methods

        private void UpdateCalibration(bool isLeft, bool increase) {
#if EIR_HAPTICS
            if (EIRManager.Instance.Haptics == null) {
                Debug.Log($"[Menu Example] No HapticManager available.");
                return;
            }
            int handIndex = isLeft ? 1 : 0;

            int currentIndex = EIRManager.Instance.Haptics.CalibrationIndex[handIndex];
            if (increase && currentIndex < HapticManager.CALIBRATION_INDEX_LENGTH) {
                currentIndex += 1;
            } else if (!increase && currentIndex > 0) {
                currentIndex -= 1;
            }

            Debug.Log($"[Calibration] [Menu Example] Right: {EIRManager.Instance.Haptics.CalibrationIndex[0]}. Left: {EIRManager.Instance.Haptics.CalibrationIndex[1]}.");

            EIRManager.Instance.Haptics.CalibrationIndex[handIndex] = currentIndex;
            EIRManager.Instance.Haptics.ModifyCalibrationByIndex(isLeft, currentIndex);
#endif
        }

#if EIR_COMM
        private async void ConnectAsync(Text text) {
            text.text = (await EIRManager.Instance.Communication.ScanAndConnect() == ConnectionStates.Connected ? "Disconnect" : "Connect");
        }
#endif
        #endregion
    }

}
