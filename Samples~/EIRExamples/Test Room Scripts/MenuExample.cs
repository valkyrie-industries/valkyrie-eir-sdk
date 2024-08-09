using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

#if EIR_COMM
using Valkyrie.EIR.Bluetooth;
#endif
using UnityEngine.UI;


#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif

namespace Valkyrie.EIR.Examples
{
    public class MenuExample : MonoBehaviour
    {

#if EIR_HAPTICS
        private HapticManager haptic;
#endif
      
        void Start()
        {
#if EIR_HAPTICS
            if (haptic == null) haptic = EIRManager.Instance.Haptics;
#endif
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        #region EMS

        public void Increment(bool isLeft)
        {
            UpdateCalibration(isLeft, true);
        }

        public void Decrement(bool isLeft)
        {
            UpdateCalibration(isLeft, false);
        }

        public void ToggleEMS(bool activate)
        {
#if EIR_COMM
            EIRManager.Instance.ToggleBluetoothSend(!EIRManager.Instance.Communication.IsActive);
#endif
        }

        private void UpdateCalibration(bool isLeft, bool increase)
        {
#if EIR_HAPTICS
            if (haptic == null)
            {
                Debug.Log($"[Menu Example] No HapticManager in scene.");
                return;
            }
            int handIndex = isLeft ? 1 : 0;

            int currentIndex = HapticManager.calibrationIndex[handIndex];
            if (increase && currentIndex < HapticManager.CALIBRATION_INDEX_LENGTH)
            {
                currentIndex += 1;
            }
            else if (!increase && currentIndex > 0)
            {
                currentIndex -= 1;
            }

            Debug.Log("[Calibration][Menu Example] Right: " + HapticManager.calibrationIndex[0] + " Left: " + HapticManager.calibrationIndex[1]);

            HapticManager.calibrationIndex[handIndex] = currentIndex;
            haptic.ModifyCalibrationByIndex(isLeft, currentIndex);
#endif
        }

        public void Connect(Text text)// (TextMeshProUGUI text)
        {
#if EIR_COMM
            if (!EIRManager.Instance.Communication.IsConnected)
            {
                //text.SetText("Connecting");
                text.text = "Connecting";
                ConnectAsync(text);
            }
            else {
                EIRManager.Instance.Communication.Disconnect();
                text.text = "Not Connected"; //SetText("Not Connected");
            }
#endif
        }
#if EIR_COMM
        private async void ConnectAsync(Text text)// (TextMeshProUGUI text)
        {
            //text.SetText(await EIRManager.Instance.Communication.ScanAndConnect() == ConnectionStates.Connected ? "Disconnect" : "Connect");
            text.text = (await EIRManager.Instance.Communication.ScanAndConnect() == ConnectionStates.Connected ? "Disconnect" : "Connect");
        }
#endif
#endregion
    }

}
