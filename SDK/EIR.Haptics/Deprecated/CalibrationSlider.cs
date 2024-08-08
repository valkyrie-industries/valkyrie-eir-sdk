using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Valkyrie.EIR.Haptics
{
    public class CalibrationSlider : MonoBehaviour
    {
        /*
        public CalibrationProcess calibrationProcess;
        private Slider currentSlider;
        public int index;

        void Start()
        {
            currentSlider = GetComponent<Slider>();
            currentSlider.onValueChanged.AddListener(delegate { UpdateEMSCalibration(); });
            if (calibrationProcess.calibrationValues != null)
                currentSlider.value = calibrationProcess.calibrationValues.limits[index];
        }

        public void UpdateEMSCalibration()
        {
            UpdateUIValue();
            if (calibrationProcess.calibrationValues != null)
                calibrationProcess.calibrationValues.limits[index] = (int)currentSlider.value;
        }

        public void UpdateUIValue()
        {
            if (GetComponentInChildren<TextMeshProUGUI>() != null)
            {
                GetComponentInChildren<TextMeshProUGUI>().text = currentSlider.value.ToString();
            }
        }
        */

    }
}

