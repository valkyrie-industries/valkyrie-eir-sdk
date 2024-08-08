using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Valkyrie.EIR.Bluetooth {

    public class DeviceManager : MonoBehaviour {

#if EIR_COMM

        public GameObject buttonPrefab;
        public Transform scrollView;
        public static Action<DeviceManager> OnRequestDevices;
        public static Action<string> OnConnectionRequest;

        private List<Button> cachedButtons = new List<Button>();
        private List<string> macAddresses = new List<string>();

        private void OnEnable() {
            OnRequestDevices?.Invoke(this);
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
            OnConnectionRequest?.Invoke(macAddresses[index]);
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
