using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valkyrie.EIR.Haptics;
using UnityEngine.EventSystems;

namespace Valkyrie.EIR.Examples {
    public class EMSTestButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        List<DeviceRole> affectedRoles = new List<DeviceRole>();

        [SerializeField]
        HapticPreset.PresetType presetType;

        [SerializeField]
        private bool holdToTest;

        public void OnPointerDown(PointerEventData eventData)
        {
#if EIR_HAPTICS
            TestEMS();
#endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
#if EIR_HAPTICS
            EndEMSTest();
#endif
        }

#if EIR_HAPTICS
        void TestEMS()
        {
            EIRManager.Instance.Haptics.CreateHapticPresetRunner(affectedRoles, HapticPreset.CreateDefaultPreset(presetType, 1, holdToTest ? HapticPreset.LoopType.Loop : HapticPreset.LoopType.None));
        }

        void EndEMSTest()
        {
            if (holdToTest)
                EIRManager.Instance.Haptics.StopHapticPresetRunner(affectedRoles);
        }

#endif
    }
}


