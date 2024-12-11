using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valkyrie.EIR.Bluetooth;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Example implementation of an EIR Device Selection UX.
    /// </summary>
    public class EirDeviceSelection : MonoBehaviour {

#if EIR_COMM

        #region Serialized Variables

        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private Transform scrollView;

        #endregion

        #region Private Variables

        private List<Button> cachedButtons = new List<Button>();
        private List<string> macAddresses = new List<string>();

        #endregion

        #region Unity Methods

        private void OnEnable() {

            List<KeyValuePair<string, string>> devices = new List<KeyValuePair<string, string>>();
            foreach (BluetoothDeviceInfo device in EIRManager.Instance.EirBluetooth.DeviceList.devices) {
                devices.Add(new KeyValuePair<string, string>(device.address, device.name));
            }

            InstantiateButtons(devices);
        }

        private void Update() {
            if (!EIRManager.Instance.Initialised) return;
#if EIR_COMM
            if (EIRManager.Instance.EirBluetooth == null) return;
            CheckConnectionState(EIRManager.Instance.EirBluetooth.CurrentState);
#endif
        }

        #endregion

        #region Private Methods

        private void InstantiateButtons(List<KeyValuePair<string, string>> devices) {

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

        private void CheckConnectionState(ConnectionStates state) {
            if (EIRManager.Instance == null || EIRManager.Instance.EirBluetooth == null)
                return;

            if (EIRManager.Instance.EirBluetooth.CurrentState == ConnectionStates.Denied) {
                Debug.LogWarning("[Device Manager] Device Connection Denied.");
                OnEnable();
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

        #endregion

#endif
    }
}
