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
        List<BodyPart> affectedParts = new List<BodyPart>();

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
            EIRManager.Instance.Haptics.CreateHapticPresetRunner(affectedParts, HapticPreset.CreateDefaultPreset(presetType, 1, holdToTest ? HapticPreset.LoopType.Loop : HapticPreset.LoopType.None));
        }

        void EndEMSTest()
        {
            if (holdToTest)
                EIRManager.Instance.Haptics.StopHapticPresetRunner(affectedParts);
        }

#endif
    }
}


