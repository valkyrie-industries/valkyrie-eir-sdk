using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valkyrie.EIR.Examples
{
    public class PieDisplay : EMSDisplay
    {
        [SerializeField]
        public Image meter;

        private void Update()
        {
            UpdateIndicatorImages();
        }

        void UpdateIndicatorImages()
        {
            meter.fillAmount = signalLevels[(int)part];
        }

    }
}
    


