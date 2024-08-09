using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valkyrie.EIR;

namespace Valkyrie.EIR.Haptics
{
    public class ConfigureButton : MonoBehaviour
    {
        public TextMeshProUGUI output;

        public enum DisplayType
        {
            Gain,
            Frequency,
            PulseWidth
        }

        public DisplayType displayedOutput;

        private ConfigureEIR configure;

        public void Start()
        {
            configure = FindObjectOfType<ConfigureEIR>();
            UpdateOutput();
        }

        private void Update()
        {
            if(configure != null)
            {
                UpdateOutput();
            }
        }

        public void UpdateOutput()
        {
            switch (displayedOutput)
            {
                case DisplayType.Gain:
                    {
                        output.text = configure.gain.ToString();
                        break;
                    }
                case DisplayType.Frequency:
                    {
                        output.text = configure.frequency.ToString();
                        break;
                    }
                case DisplayType.PulseWidth:
                    {
                        output.text = configure.pulseWidth.ToString();
                        break;
                    }
            }

        }
    }

}

