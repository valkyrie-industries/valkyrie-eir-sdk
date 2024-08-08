using System.Collections;
using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.UI;
using System;

namespace Valkyrie.EIR.Haptics
{
    public class CalibrationProcess : MonoBehaviour
    {
        /*
        public XRController[] hands;
        public Slider slider;
        private bool listeningToController, pointingAtThis;
        [SerializeField] private GameObject nextButton, previousButton, playButton;

        [SerializeField] private Text instructions0, instructions1, minMaxText, muscleText;
        [SerializeField] bool changeInstructionText = true;

        public HapticCalibration calibrationValues;

        public int muscleNum;
        public int calibrationValueNum = 0; // 0 - 1st muscle min, 1st muscle max, 2nd muscle min, 2nd muscle max

        private HapticsManager hapticsManager;

        public ActivateButton activateButton;

        private bool previousActivateButton;

        private bool stopAllPhysicalInteraction;

        // New Hardware
        public bool newHardware;
        private HapticSignalVer2 hapticSignalVer2;
        [SerializeField]
        private int frequency, pulseWidth;

        // Start is called before the first frame update
        void Start()
        {
            listeningToController = true;
            hapticsManager = FindObjectOfType<HapticsManager>();
            if (hapticsManager == null)
                Debug.LogError("No HapticsManager script found", this);

            var existingCal = FindObjectOfType<HapticCalibration>();
            if (calibrationValues == null)
                calibrationValues = existingCal;

            // New Hardware
            frequency = HapticVariables.constantFrequency;
            pulseWidth = HapticVariables.constantPusleWidth;
            hapticSignalVer2 = new HapticSignalVer2()
            {
                enableEms = 1,
                config = 0,
                gain_0 = 0,
                gain_1 = 0,
                frequency = HapticVariables.constantFrequency,
                pulse_width = HapticVariables.constantPusleWidth,
                master_intensity_LSB = 0,
                master_intensity_MSB = 0,
                slave_intensity_LSB = 0,
                slave_intensity_MSB = 0
            };
        }

        void Update()
        {

            ThumbIntoSlider(hands[0], slider);
            ThumbIntoSlider(hands[1], slider);

            bool triggerLeft = ReadButton(hands[0]);
            bool triggerRight = ReadButton(hands[1]);
            if (activateButton.active)
            {
                hapticsManager.stopAllPhysicalInteraction = true;
                SendPulse((int)slider.value);
            }

            if (activateButton.active != previousActivateButton && previousActivateButton)
            {
                StartCoroutine(SendNull());
            }

            previousActivateButton = activateButton.active;
        }

        private void ThumbIntoSlider(XRController _hand, Slider slider)
        {
            if (ReadInput(_hand).y > 0.9f)
                slider.value += 1;
            if (ReadInput(_hand).y < -0.9f)
                slider.value -= 1;
        }

        void OnDestroy()
        {
            if (listeningToController)
                SendPulse(0);
        }

        IEnumerator SendNull()
        {
            yield return new WaitForSeconds(0.1f);
            SendPulse(0);
            hapticsManager.stopAllPhysicalInteraction = false;
        }

        private void SendPulse(int value)
        {
            try
            {
                Debug.Log("Sending calibration values " + value);
                hapticsManager.FormSignal(muscleNum, value, true);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString(), this);
            }
        }

        public void ConfigureNewHardware(int gain)
        {
            hapticsManager.ConfigureEMSHardware(gain, (byte)frequency, (byte)pulseWidth);
        }

        void SendHapticsDirectlyToNewHardware(bool enable, int _value)
        {
            if (enable)
            {
                hapticSignalVer2.enableEms = 1;
                hapticSignalVer2.master_intensity_LSB = (byte)_value;
                hapticSignalVer2.master_intensity_MSB = (byte)_value; // For biphasic
            }
            else
            {
                hapticSignalVer2.enableEms = 0;
            }
            hapticsManager.SendEMSSignalConstantly(hapticSignalVer2);
        }

        public void UpdateFrequency(System.Single input)
        {
            frequency = (int)input;
        }

        public void UpdatePulseWidth(System.Single input)
        {
            pulseWidth = (int)input;
        }

        public void GoNext()
        {
            calibrationValueNum = (calibrationValueNum + 1);
            UpdateValueNum();
            UpdateVisibility();
        }

        public void GoPrevious()
        {
            calibrationValueNum = (calibrationValueNum + 3) % 4;
            UpdateValueNum();
            UpdateVisibility();
        }

        public void Restart()
        {
            calibrationValueNum = 0;
            UpdateValueNum();
            UpdateVisibility();
        }

        private void UpdateValueNum()
        {
            int calIndex = 0;
            switch (calibrationValueNum)
            {
                case 0:
                    calIndex = 0;
                    break;
                case 1:
                    calIndex = 4;
                    break;
                case 2:
                    calIndex = 1;
                    break;
                case 3:
                    calIndex = 5;
                    break;
                case 4:
                    return;
            }
            muscleNum = calIndex % 4;
            slider.GetComponent<CalibrationSlider>().index = calIndex;
            if (calibrationValues != null)
                slider.value = calibrationValues.limits[calIndex];
        }

        private void UpdateVisibility()
        {

            previousButton.SetActive(calibrationValueNum != 0);
            nextButton.SetActive(calibrationValueNum != 4);
            playButton.SetActive(calibrationValueNum == 4);

            if (calibrationValueNum == 4)
            {
                muscleText.text = "";
                minMaxText.text = "";
                if (instructions0 != null)
                    instructions0.text = "You are now calibrated!";
                if (changeInstructionText)
                    if (instructions1 != null)
                        instructions1.text = "Click Play to start the fitness experience";
                activateButton.gameObject.SetActive(false);
                slider.gameObject.SetActive(false);
                return;
            }
            activateButton.gameObject.SetActive(true);
            slider.gameObject.SetActive(true);
            muscleText.text = muscleNum == 0 ? "Right arm" : "Left arm";
            minMaxText.text = calibrationValueNum % 2 < 1 ? "Minimum" : "Maximum";
            minMaxText.color = calibrationValueNum % 2 < 1 ? Color.green : Color.red;
            if (changeInstructionText)
            {
                if (instructions0 != null)
                    instructions0.text = Instructions0();
                if (instructions1 != null)
                    instructions1.text = Instructions1(calibrationValueNum % 2 < 1);
            }

        }

        private string Instructions0()
        {
            string outstring = "";
            switch (calibrationValueNum)
            {
                case 0:
                    outstring = "Move the slider to the values around 100 / 120 then press \"ACTIVATE EMS\". Adjust the value by using Up/Down Thumbstick.";
                    break;
                case 1:
                    outstring = "Increase the value above the previous minimum value, then press \"ACTIVATE EMS\". Adjust the value by using Up/ Down Thumbstick.";
                    break;
                case 2:
                    outstring = "Move the slider to the values around 100 / 120 then press \"ACTIVATE EMS\". Adjust the value by using Up/Down Thumbstick.";
                    break;
                case 3:
                    outstring = "Increase the value above the previous minimum value, then press \"ACTIVATE EMS\". Adjust the value by using Up/ Down Thumbstick.";
                    break;
            }
            return outstring;
        }

        private string Instructions1(bool min)
        {
            if (min)
                return "Continue increasing the value and pressing \"ACTIVATE EMS\" until you start feeling the EMS. Then click Next";
            else
                return "Continue increasing the value and pressing \"ACTIVATE EMS\" until you've reached the maximum desired activation level. Then click Next";
        }

        private bool ReadButton(XRController controller)
        {
            bool input = false;
            var feature = CommonUsages.triggerButton;
            if (controller != null &&
                controller.enableInputActions &&
                controller.inputDevice.TryGetFeatureValue(feature, out var controllerInput))
            {
                input = controllerInput;
            }
            return input;
        }

        private Vector2 ReadInput(XRController controller)
        {
            var input = Vector2.zero;
            if (!listeningToController)
                return input;

            var feature = CommonUsages.primary2DAxis;
            if (controller != null &&
                controller.enableInputActions &&
                controller.inputDevice.TryGetFeatureValue(feature, out var controllerInput))
            {
                input += controllerInput;
            }

            return input;
        }
        */
    }
}