using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR;
using Valkyrie.EIR.Haptics;

public class HapticExample : MonoBehaviour
{
    HapticPreset preset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.riseWaitFall, 1);

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.leftHand,preset);
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.rightHand, preset);
        }
    }
}
