using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Haptics;
using UnityEngine.EventSystems;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example implementation of quick EMS output from pointer input.
    /// </summary>
    public class EMSTestButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

        #region Serialized Variables

        [SerializeField]
        List<BodyPart> affectedParts = new List<BodyPart>();

        [SerializeField]
        HapticPreset.PresetType presetType;

        [SerializeField]
        private bool holdToTest;

        #endregion

        #region Interface Implementations

        public void OnPointerDown(PointerEventData eventData) {
#if EIR_HAPTICS
            BeginEMSOutput();
#endif
        }

        public void OnPointerUp(PointerEventData eventData) {
#if EIR_HAPTICS
            EndEMSOutput();
#endif
        }

        #endregion

        #region Private Methods

#if EIR_HAPTICS
        private void BeginEMSOutput() {
            EIRManager.Instance.Haptics.CreateHapticPresetRunner(affectedParts, HapticPreset.CreateDefaultPreset(presetType, 1, holdToTest ? HapticPreset.LoopType.Loop : HapticPreset.LoopType.None));
        }

        private void EndEMSOutput() {
            if (holdToTest)
                EIRManager.Instance.Haptics.StopHapticPresetRunner(affectedParts);
        }
#endif
        #endregion

    }
}


