using UnityEngine;
using TMPro;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example of an EIR configuration button for setting Gain, Frequency and Pulse Width.
    /// </summary>
    public class ConfigureButton : MonoBehaviour {

        #region Enums

        public enum DisplayType {
            Gain,
            Frequency,
            PulseWidth
        }

        #endregion

        #region Serialized Variables

        [SerializeField]
        private TextMeshProUGUI output;
        [SerializeField]
        private DisplayType displayedOutput;

        #endregion

        #region Private Variables

        private ConfigureEIR configure;

        #endregion

        #region Unity Methods

        public void Start() {
            configure = FindObjectOfType<ConfigureEIR>();
            UpdateOutput();
        }

        private void Update() {
            if (configure != null) {
                UpdateOutput();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the output values for gain, frequency and pulse width.
        /// </summary>
        public void UpdateOutput() {
            switch (displayedOutput) {
                case DisplayType.Gain: {
                        output.text = configure.gain.ToString();
                        break;
                    }
                case DisplayType.Frequency: {
                        output.text = configure.frequency.ToString();
                        break;
                    }
                case DisplayType.PulseWidth: {
                        output.text = configure.pulseWidth.ToString();
                        break;
                    }
            }
        }

        #endregion
    }

}

