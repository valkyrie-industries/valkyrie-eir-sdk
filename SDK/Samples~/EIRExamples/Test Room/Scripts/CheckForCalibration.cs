using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Haptics;
using Valkyrie.EIR;

public class CheckForCalibration : MonoBehaviour
{
    [SerializeField]
    GameObject conCalFloor, testRoomFloor;

    // Start is called before the first frame update
#if EIR_HAPTICS
    void Start()
    {
        if(EIRManager.Instance.Haptics != null)
        {
            if(HapticManager.UpperLimits[(int)BodyPart.leftHand] != 0 || HapticManager.UpperLimits[(int)BodyPart.rightHand] != 0)
            {
                conCalFloor.SetActive(false);
                testRoomFloor.SetActive(true);
            }
        }
    }
#endif
}
