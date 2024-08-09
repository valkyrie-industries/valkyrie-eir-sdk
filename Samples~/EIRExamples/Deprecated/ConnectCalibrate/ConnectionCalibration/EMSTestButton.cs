using System.Collections.Generic;
using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
using UnityEngine.EventSystems;

namespace Valkyrie.EIR.Examples {
    public class EMSTestButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        List<BodyPart> affectedParts = new List<BodyPart>();
#if EIR_HAPTICS
        [SerializeField]
        HapticPreset.PresetType presetType;
#endif

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
            EIRManager.Instance.Haptics.CreateHapticPresetRunner(affectedParts, HapticPreset.CreateDefaultPreset(presetType, 1, holdToTest ? true : false));
        }

        void EndEMSTest()
        {
            if (holdToTest)
                EIRManager.Instance.Haptics.StopHapticPresetRunner(affectedParts);
        }

#endif
    }
}


