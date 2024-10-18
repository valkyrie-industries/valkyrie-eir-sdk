using UnityEngine;
using TMPro;
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
        public void AlterGain(int modifier) {
            gain += modifier;
            gain = Mathf.Clamp(gain, MIN_GAIN, MAX_GAIN);
        }

        /// <summary>
        /// Modifies the frequency by the input amount. Negative values will reduce the gain.
        /// </summary>
        /// <param name="modifier"></param>
        public void AlterFrequency(int modifier) {
            frequency += (byte)modifier;
        }

        /// <summary>
        /// Modifies the pulse width by the input amount. Negative values will reduce the gain.
        /// </summary>
        /// <param name="modifier"></param>
        public void AlterPulseWidth(int modifier) {
            pulseWidth += (byte)modifier;
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

            EIRManager.Instance.Communication.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(gain, frequency, pulseWidth));
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
            EIRManager.Instance.Communication.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(gain, frequency, pulseWidth));
#endif
        }

        /// <summary>
        /// Resets gain, frequency and pulse width to default, and commands the EIR Bluetooth Bridge to send a config signal to the connected device.
        /// </summary>
        public void ConfigureToDefault() {

#if EIR_HAPTICS
#if EIR_COMM
            EIRManager.Instance.Communication.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(MIN_GAIN, HapticManager.CONST_FREQUENCY, HapticManager.CONST_PULSE_WIDTH));
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
            } else {
                text.text = "Lock settings";
            }

        }

        #endregion
    }
}


