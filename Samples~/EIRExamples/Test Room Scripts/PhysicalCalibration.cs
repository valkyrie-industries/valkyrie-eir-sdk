using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Examples {
    /// <summary>
    /// Calibrates the EMS using ResistanceBandInteractables for an interactive calibration experience
    /// </summary>
    public class PhysicalCalibration : MonoBehaviour
    {

        public delegate void OnCalibrationUpdatedEventHandler(int[] lowerLimits, int[] upperLimits, int[] calibrationIndex);
        public static event OnCalibrationUpdatedEventHandler OnCalibrationUpdated;

        public ResistanceBandVisuals[] resistanceBands;

        //UI elements used to display the selected indexes and a button to progress
        [SerializeField]
        private GameObject leftHandIndicator, rightHandIndicator, continueButton;

        //The currently selected calibration index for both hands
        public int leftIndex { get; private set; } = -1;
        public int rightIndex { get; private set; } = -1;



        private Image[] handIndicators = new Image[2];
        private TextMeshProUGUI[] handIndicatorTexts = new TextMeshProUGUI[2];
        private bool initialised;

        private int[] lowerLimits = new int[2];
        private int[] calibrationIndex = new int[2];
        private int[] upperLimits = new int[2];

        private void Start()
        {
#if EIR_INTERACTION && EIR_HAPTICS
            //Check for needed components
            resistanceBands = GetComponentsInChildren<ResistanceBandVisuals>(true);

            if (resistanceBands == null)
                Debug.LogError("[PhysicalCalibration] No Reistance Band Calibration Interactables are found in the scene", this);
            for (int i = 0; i < resistanceBands.Length; i++)// CalibrationResistanceBandVisuals resistanceBandCalibration in resistanceBands)
            {
                float[] minMax = EIRManager.Instance.Haptics.GetLowerUpperLimitForIndex(i, 2);
                resistanceBands[i].interactable.InitialiseCalibrationLimits(minMax[0], minMax[1]);
                resistanceBands[i].InitialiseVisuals(i);
            }
#endif
        }

        private void Update()
        {

            int newLeftIndex = -1;
            int newRightIndex = -1;

            if (resistanceBands == null)
                return;
#if EIR_INTERACTION
            //Search the resistance bands. If they're interacting with a hand, calibrate that hand to the selected resistance band's index
            for (int i = 0; i < resistanceBands.Length; i++)
            {
                InteractingBodyPart hand = resistanceBands[i].interactable.currentlyInteractingBodyPart;
                if (hand != null)
                {
                    if (hand.BodyPart == BodyPart.leftHand)
                        newLeftIndex = i;
                    if (hand.BodyPart == BodyPart.rightHand)
                        newRightIndex = i;
                }
            }

            //Record the new calibration values
            CalibrateEMS(newLeftIndex, leftIndex, true);
            CalibrateEMS(newRightIndex, rightIndex, false);

            UpdateCalibrationVisuals();
#endif
        }

        private bool GetReferences()
        {
            try
            {
                if (leftHandIndicator != null && rightHandIndicator != null)
                {
                    handIndicators[0] = leftHandIndicator.GetComponent<Image>();
                    handIndicators[1] = rightHandIndicator.GetComponent<Image>();
                    handIndicatorTexts[0] = leftHandIndicator.GetComponentInChildren<TextMeshProUGUI>();
                    handIndicatorTexts[1] = rightHandIndicator.GetComponentInChildren<TextMeshProUGUI>();
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        //Change the indicators accoring to what's selected and enable the continue button if needed
        private void UpdateCalibrationVisuals()
        {

            if (!initialised)
            {
                initialised = GetReferences();
                return;
            }

            if (leftHandIndicator != null && rightHandIndicator != null)
            {
                //If the hand corresponding to the indicator has been calibrated, turn it the colour of the band and display the calibration level
                if (leftIndex != -1)
                {
                    handIndicators[0].color = ValkyrieEIRExtensionMethods.ColorBasedOnCalibrationLevel(leftIndex);
                    handIndicatorTexts[0].text = "Left Hand calibrated to: " + (leftIndex + 1);
                }

                if (rightIndex != -1)
                {
                    handIndicators[1].color = ValkyrieEIRExtensionMethods.ColorBasedOnCalibrationLevel(rightIndex);
                    handIndicatorTexts[1].text = "Right Hand calibrated to: " + (rightIndex + 1);
                }
            }

            //If we have calibrated both hands, enable the button to continue
            if (leftIndex != -1 && rightIndex != -1 && continueButton != null)
            {
                continueButton.SetActive(true);
            }
        }

        public void ResetIndices()
        {
            leftIndex = -1; rightIndex = -1;
            lowerLimits = new int[2] { 0, 0 };
            upperLimits = new int[2] { 0, 0 };
            Start();
        }

        //Send the results of the current calibration to the device
        private void CalibrateEMS(int newIndex, int oldIndex, bool isLeft)
        {
#if EIR_INTERACTION

            //Check that the index has changed and is valid
            if (newIndex != oldIndex && newIndex != -1)
            {
                if (isLeft)
                    leftIndex = newIndex;
                else
                    rightIndex = newIndex;

                BodyPart part = isLeft ? BodyPart.leftHand : BodyPart.rightHand;

                lowerLimits[(int)part] = (int)resistanceBands[newIndex].interactable.minValue;
                upperLimits[(int)part] = (int)resistanceBands[newIndex].interactable.maxValue;
                calibrationIndex[(int)part] = newIndex;
                OnCalibrationUpdated?.Invoke(lowerLimits, upperLimits, calibrationIndex);
            }
#endif
        }

    }
}