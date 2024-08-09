using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
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
            if(HapticManager.upperLimits[(int)BodyPart.leftHand] != 0 || HapticManager.upperLimits[(int)BodyPart.rightHand] != 0)
            {
                conCalFloor.SetActive(false);
                testRoomFloor.SetActive(true);
            }
        }
    }
#endif
}
