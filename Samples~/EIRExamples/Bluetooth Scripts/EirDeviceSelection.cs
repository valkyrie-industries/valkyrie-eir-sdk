using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valkyrie.EIR.Bluetooth;

namespace Valkyrie.EIR.Examples {

    public class EirDeviceSelection : MonoBehaviour {

#if EIR_COMM

        public GameObject buttonPrefab;
        public Transform scrollView;

        private List<Button> cachedButtons = new List<Button>();
        private List<string> macAddresses = new List<string>();

        private void OnEnable() {

            List<KeyValuePair<string, string>> devices = new List<KeyValuePair<string, string>>();
            foreach (BluetoothDeviceInfo device in EIRManager.Instance.Communication.DeviceList.devices) {
                devices.Add(new KeyValuePair<string, string>(device.address, device.name));
            }

            InstantiateButtons(devices);
        }

        public void InstantiateButtons(List<KeyValuePair<string, string>> devices) {

            Clear();

            for (int i = 0; i < devices.Count; i++) {
                GameObject buttonGO = Instantiate(buttonPrefab, scrollView);
                Button button = buttonGO.GetComponent<Button>();

                if (button != null) {
                    button.GetComponentInChildren<TextMeshProUGUI>().text = devices[i].Value;
                    button.onClick.AddListener(() => OnDeviceSelected(button));

                    // Cache the instantiated button
                    cachedButtons.Add(button);
                    macAddresses.Add(devices[i].Key);
                }
                else {
                    Debug.LogWarning("[Device Manager] ButtonPrefab does not have a Button component.");
                }
            }
        }

        private void OnDeviceSelected(Button clickedButton) {
            int index = cachedButtons.IndexOf(clickedButton);
            EIRManager.Instance.ConnectEIRDevice(macAddresses[index]);
        }

        private void Clear() {
            foreach (Button button in cachedButtons) {
                Destroy(button.gameObject);
            }
            cachedButtons.Clear();
            macAddresses = new List<string>();
        }

#endif
    }
}
