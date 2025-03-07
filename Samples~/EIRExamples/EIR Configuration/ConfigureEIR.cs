using UnityEngine;
using TMPro;
using System;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Configures EIR with values it's provided or values you set or to default
    /// </summary>
    public class ConfigureEIR : MonoBehaviour {

        #region Constants

        public const int MIN_GAIN = 0;
        public const int MAX_GAIN = 3;

        #endregion


        #region Public Properties

#if EIR_HAPTICS
        public int gain { get; private set; } = MIN_GAIN;
        public byte frequency { get; private set; } = HapticManager.CONST_FREQUENCY;
        public byte pulseWidth { get; private set; } = HapticManager.CONST_PULSE_WIDTH;
#else
        public int gain { get; private set; } = MIN_GAIN;
        public byte frequency { get; private set; } = 100;
        public byte pulseWidth { get; private set; } = 100;
#endif


        #endregion

        #region Private Variables

        private bool lockSettings = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Modifies the gain by the input amount. Negative values will reduce the gain.
        /// </summary>
        /// <param name="modifier"></param>
        [Obsolete("[ConfigureEIR] Although the device can be given a gain configuration, the device does not use it. This will be removed in a future update", false)]
        public void AlterGain(int modifier) {
            gain += modifier;
            gain = Mathf.Clamp(gain, MIN_GAIN, MAX_GAIN);
            Debug.LogWarning("[ConfigureEIR] Although this can be called, the device does not recognise gain. This will be removed in a future update");
        }

        /// <summary>
        /// Modifies the frequency by the input amount. Negative values will reduce the frequency.
        /// </summary>
        /// <param name="modifier"></param>
        public void AlterFrequency(int modifier) {
#if EIR_HAPTICS
            // calculate the new frequency using an int
            int newFrequency = frequency + modifier;

            // check for overflow or underflow
            if (newFrequency > byte.MaxValue || newFrequency < HapticManager.MIN_FREQUENCY) {
                return;  // prevent overflow/underflow
            }

            frequency += (byte)modifier;
#endif
        }

        /// <summary>
        /// Modifies the pulse width by the input amount. Negative values will reduce the pulse width.
        /// </summary>
        /// <param name="modifier"></param>
        public void AlterPulseWidth(int modifier) {
#if EIR_HAPTICS
            // calculate the new frequency using an int
            int newPulseWidth = pulseWidth + modifier;

            // check for overflow or underflow
            if (newPulseWidth > byte.MaxValue || newPulseWidth < HapticManager.MIN_PULSE_WIDTH) {
                return;  // prevent overflow/underflow
            }

            pulseWidth += (byte)modifier;
#endif
        }

        /// <summary>
        /// Sets the gain, frequency and pulse width to the input values, and commands the EIR Bluetooth Bridge to send a config signal to the connected device.
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="frequency"></param>
        /// <param name="pulseWidth"></param>
        public void Configure(int gain, byte frequency, byte pulseWidth) {
            if (lockSettings) {
                ConfigureToSet();
                return;
            }
#if EIR_COMM && EIR_HAPTICS

            EIRManager.Instance.EirBluetooth.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(gain, frequency, pulseWidth));
#endif
            this.gain = gain;
            this.frequency = frequency;
            this.pulseWidth = pulseWidth;
        }

        /// <summary>
        /// Commands the EIR Bluetooth Bridge to send a config signal to the connected device with the current set values.
        /// </summary>
        public void ConfigureToSet() {
#if EIR_COMM && EIR_HAPTICS
            EIRManager.Instance.EirBluetooth.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(gain, frequency, pulseWidth));
#endif
        }

        /// <summary>
        /// Resets gain, frequency and pulse width to default, and commands the EIR Bluetooth Bridge to send a config signal to the connected device.
        /// </summary>
        public void ConfigureToDefault() {

#if EIR_HAPTICS
#if EIR_COMM
            EIRManager.Instance.EirBluetooth.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(MIN_GAIN, HapticManager.CONST_FREQUENCY, HapticManager.CONST_PULSE_WIDTH));
#endif
            gain = MIN_GAIN;
            frequency = HapticManager.CONST_FREQUENCY;
            pulseWidth = HapticManager.CONST_PULSE_WIDTH;
#endif
        }

        /// <summary>
        /// Locks gain, frequency and pulse width. Values cannot be changed in this state.
        /// Toggles locked/unlocked.
        /// </summary>
        /// <param name="text"></param>
        public void ToggleLockSettings(TextMeshProUGUI text) {
            lockSettings = !lockSettings;

            if (lockSettings) {
                text.text = "Unlock settings";
            }
            else {
                text.text = "Lock settings";
            }

        }

        #endregion
    }
}


