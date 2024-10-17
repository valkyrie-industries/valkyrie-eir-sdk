using UnityEngine;
using UnityEngine.UI;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Overrides EMSDisplay example and visualises as a pie.
    /// </summary>
    public class PieDisplay : EMSDisplay {

        #region Serialized Variables

        [SerializeField]
        private Image meter;

        #endregion

        #region Unity Methods

        private void Update() {
            meter.fillAmount = signalLevels[(int)part];
        }

        #endregion
    }
}



