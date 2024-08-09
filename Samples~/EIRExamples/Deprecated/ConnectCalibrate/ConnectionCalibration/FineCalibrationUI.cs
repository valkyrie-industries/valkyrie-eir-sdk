using UnityEngine;
using UnityEngine.UI;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
using Valkyrie.EIR;

public class FineCalibrationUI : MonoBehaviour
{
    [SerializeField]
    Slider[] sliders;

    private void Update()
    {
#if EIR_HAPTICS
        if (EIRManager.Instance.Haptics == null)
            return;
        sliders[0].value = HapticManager.lowerLimits[(int)BodyPart.leftHand];
        sliders[1].value = HapticManager.upperLimits[(int)BodyPart.leftHand];
        sliders[2].value = HapticManager.lowerLimits[(int)BodyPart.rightHand];
        sliders[3].value = HapticManager.upperLimits[(int)BodyPart.rightHand];
#endif
    }


    public void UpdateLeftMin(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.lowerLimits[(int)BodyPart.leftHand] = (int)value;
#endif
    }

    public void UpdateLeftMax(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.upperLimits[(int)BodyPart.leftHand] = (int)value;
#endif
    }

    public void UpdateRightMin(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.lowerLimits[(int)BodyPart.rightHand] = (int)value;
#endif
    }

    public void UpdateRightMax(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.upperLimits[(int)BodyPart.rightHand] = (int)value;
#endif
    }
}
