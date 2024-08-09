using UnityEngine;
using TMPro;

namespace Valkyrie.EIR.Haptics{
    /// <summary>
    /// Configures EIR with values it's provided or values you set or to default
    /// </summary>
    public class ConfigureEIR : MonoBehaviour {
        public int gain { get; private set; } = MIN_GAIN;
        public byte frequency { get; private set; } = HapticManager.CONST_FREQUENCY;
        public byte pulseWidth { get; private set; } = HapticManager.CONST_PULSE_WIDTH;

        public const int MIN_GAIN = 0;
        public const int MAX_GAIN = 3;

        bool lockSettings = false;

        public void AlterGain(int modifier) {
            gain += modifier;
            gain = Mathf.Clamp(gain, MIN_GAIN, MAX_GAIN);
        }

        public void AlterFrequency(int modifier) {
            frequency += (byte)modifier;
        }

        public void AlterPulseWidth(int modifier) {
            pulseWidth += (byte)modifier;
        }

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

        public void ConfigureToSet() {
#if EIR_COMM && EIR_HAPTICS
            EIRManager.Instance.Communication.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(gain, frequency, pulseWidth));
#endif
        }

        public void ConfigureToDefault() {
#if EIR_COMM && EIR_HAPTICS
            EIRManager.Instance.Communication.SendConfigSignal(EIRManager.Instance.Haptics.GenerateConfigSignal(MIN_GAIN, HapticManager.CONST_FREQUENCY, HapticManager.CONST_PULSE_WIDTH));
#endif
            this.gain = MIN_GAIN;
            this.frequency = HapticManager.CONST_FREQUENCY;
            this.pulseWidth = HapticManager.CONST_PULSE_WIDTH;
        }

        public void ToggleLockSettings(TextMeshProUGUI text) {
            lockSettings = !lockSettings;

            if (lockSettings) {
                text.text = "Unlock settings";
            }
            else {
                text.text = "Lock settings";
            }

        }
    }
}


