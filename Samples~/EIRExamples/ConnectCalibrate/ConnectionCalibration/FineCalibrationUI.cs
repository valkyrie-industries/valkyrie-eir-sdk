using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valkyrie.EIR.Haptics;
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
#endif
        sliders[0].value = HapticManager.LowerLimits[(int)BodyPart.leftHand];
        sliders[1].value = HapticManager.UpperLimits[(int)BodyPart.leftHand];
        sliders[2].value = HapticManager.LowerLimits[(int)BodyPart.rightHand];
        sliders[3].value = HapticManager.UpperLimits[(int)BodyPart.rightHand];
    }


    public void UpdateLeftMin(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.LowerLimits[(int)BodyPart.leftHand] = (int)value;
#endif
    }

    public void UpdateLeftMax(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.UpperLimits[(int)BodyPart.leftHand] = (int)value;
#endif
    }

    public void UpdateRightMin(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.LowerLimits[(int)BodyPart.rightHand] = (int)value;
#endif
    }

    public void UpdateRightMax(System.Single value)
    {
#if EIR_HAPTICS
        HapticManager.UpperLimits[(int)BodyPart.rightHand] = (int)value;
#endif
    }
}
