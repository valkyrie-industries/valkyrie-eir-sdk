using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

#if EIR_COMM
using Valkyrie.EIR.Bluetooth;
#endif

namespace Valkyrie.EIR.Examples {
    /// <summary>
    /// Connects to the Valkyrie EIR in the background. 
    /// Intended to be placed on a button with a text child.
    /// Checks the status of connection
    /// </summary>
    public class SimpleConnection : MonoBehaviour {
        [Header("Optional UI")]
        [SerializeField]
        private GameObject ConnectionUI;
        [SerializeField]
        private GameObject afterConnectionStageUI;
        [SerializeField]
        private GameObject LoadingIcon;
        [SerializeField]
        private GameObject connectionIndicator;

        // Optional button to move to the after-connection stage
        [SerializeField]
        private Button afterConnectionStageButton;

        // If we want to change the text of the UI (connected, connecting, disconnected)
        [SerializeField]
        private bool doTextChanges = false;

        // If true, we take the first valid device instead of triggering the selection event
        [SerializeField]
        private bool takeFirstConnection = false;

        //The event that fires if we find multiple valid EIR devices while scanning and takeFirstConnection is NOT true
        public UnityEvent OnSelectionState;

        private bool selectionStateEventFired = false;

        private bool connectionEstablished;

        private static bool attemptingConnection = false;

        [SerializeField]
        private bool connectAutomatically;

        private void Start() {
#if EIR_COMM
            StartCoroutine(ConnectNextFrame());
#endif
        }

#if EIR_COMM
        private IEnumerator ConnectNextFrame() {
            yield return new WaitForEndOfFrame();
            if (!EIRManager.Instance.Initialised) {
                throw new System.Exception("[SimpleConnection] EIR Manager is not initialised. Eir Config should be auto-initialising for this example script, check this is toggled.");
            } else {
                if (EIRManager.Instance.Communication != null) {
                    CheckConnectionState(EIRManager.Instance.Communication.CurrentState);
                    if (connectAutomatically) Connect(true);
                }
            }
        }
#endif


        private void Update() {
            if (!EIRManager.Instance.Initialised) return;
            //if (connectionEstablished)
            //TODO: Make it do it once in a while (every second)
#if EIR_COMM
            if (EIRManager.Instance.Communication == null) return;
            CheckConnectionState(EIRManager.Instance.Communication.CurrentState);
#endif
        }

        public void ConnectToDevice() {
            Debug.Log("[SimpleConnection] Attempting connection...");
#if EIR_COMM

            if (EIRManager.Instance.Communication.IsConnected) {
                Debug.Log("[SimpleConnection] Device is already connected");
                return;
            }

            if (!connectionEstablished && !attemptingConnection)
                ConnectionAsync();
#endif
        }

        public void Connect(bool connect) {
#if EIR_COMM
            if (connect) {
                Debug.Log("[SimpleConnection] Attempting connection via autoconnection");
                if (EIRManager.Instance.Communication.IsConnected) {
                    Debug.Log("[SimpleConnection] Device is already connected");
                    return;
                }

                if (!attemptingConnection)
                    ConnectionAsync();
            }
            else {
                EIRManager.Instance.Communication.IsActive = false;
                if (!EIRManager.Instance.Communication.IsConnected) {
                    Debug.Log("[SimpleConnection] Device is already disconnected");
                    return;
                }
                Debug.Log("[SimpleConnection] Disconnecting device");
                EIRManager.Instance.Communication.Disconnect();
            }
#endif
        }

        public void Connect() {
            Connect(!connectionEstablished);
        }

        private async void ConnectionAsync() {
#if EIR_COMM
            attemptingConnection = true;

            if (this.GetComponent<Button>() != null)
                GetComponent<Button>().interactable = false;

            if (LoadingIcon != null)
                LoadingIcon.SetActive(true);

            ChangeText("Connecting...");

            ConnectionStates state = await EIRManager.Instance.Communication.ScanAndConnect();
            CheckConnectionState(state);
            attemptingConnection = false;
#endif
        }
#if EIR_COMM

        private IEnumerator ConnectionCoroutine() {
            attemptingConnection = true;
            LoadingIcon.SetActive(true);
            gameObject.GetComponent<Image>().enabled = false;
            GetComponentInChildren<TMP_Text>().text = "Connecting...";

            if (doTextChanges) {
                GetComponentInChildren<TextMeshProUGUI>().text = "Connecting...";
            }

            EIRManager.Instance.Communication.ScanAndConnect(); // autoconnection needs to be refactored, this is actaully an awaitable function
            Debug.Log("[SimpleConnection] AutoConnection is scanning");

            while (EIRManager.Instance.Communication.CurrentState == ConnectionStates.Scanning) {
                yield return null;
            }

            if (EIRManager.Instance.Communication.CurrentState == ConnectionStates.NotFound || EIRManager.Instance.Communication.CurrentState == ConnectionStates.NotConnected) {
                attemptingConnection = false;
                yield break;
            }

            connectionEstablished = true;
            attemptingConnection = false;


        }

        private void CheckConnectionState(ConnectionStates state) {
            if (EIRManager.Instance == null || EIRManager.Instance.Communication == null)
                return;

            if (EIRManager.Instance.Communication.CurrentState != ConnectionStates.Selection) {
                selectionStateEventFired = false;
            }

            switch (EIRManager.Instance.Communication.CurrentState) {
                case ConnectionStates.Selection: {
                        if (takeFirstConnection) {
                            _ = EIRManager.Instance.Communication.Connect(EIRManager.Instance.Communication.DeviceList.devices[0].address);
                        }
                        else if (!selectionStateEventFired) {
                            OnSelectionState.Invoke();
                            selectionStateEventFired = true;
                        }

                        break;

                    }
                case ConnectionStates.Connected: {

                        if (this.GetComponent<Button>() != null)
                            GetComponent<Button>().interactable = true;

                        if (afterConnectionStageButton != null)
                            afterConnectionStageButton.interactable = true;

                        if (LoadingIcon != null)
                            LoadingIcon.SetActive(false);

                        ChangeText("Disconnect");
                        attemptingConnection = false;
                        connectionEstablished = true;
                        MoveToNextUIStage();
                        break;
                    }

                case ConnectionStates.NotConnected:
                case ConnectionStates.NotFound: {
                        // Make it interactable
                        if (this.GetComponent<Button>() != null)
                            GetComponent<Button>().interactable = true;

                        if (LoadingIcon != null)
                            LoadingIcon.SetActive(false);

                        ChangeText("Connect");
                        connectionEstablished = false;
                        attemptingConnection = false;

                        if (afterConnectionStageButton != null)
                            afterConnectionStageButton.interactable = false;

                        break;
                    }
                case ConnectionStates.Connecting:
                case ConnectionStates.Scanning: {
                        // Make it non-interactable
                        if (this.GetComponent<Button>() != null)
                            GetComponent<Button>().interactable = false;


                        ChangeText("Connecting...");
                        break;
                    }
            }

            ChangeIndicator(EIRManager.Instance.Communication.CurrentState);
        }

        private void ChangeText(string text) {
            if (doTextChanges) {
                if (GetComponentInChildren<Text>())
                    GetComponentInChildren<Text>().text = text;
                if (GetComponentInChildren<TMP_Text>())
                    GetComponentInChildren<TMP_Text>().text = text;
            }
        }

        private void ChangeIndicator(ConnectionStates state) {
            if (connectionIndicator != null) {
                if (connectionIndicator.GetComponent<Image>() != null) {
                    switch (state) {
                        case ConnectionStates.Connected:
                            connectionIndicator.GetComponent<Image>().color = Color.green;
                            break;
                        case ConnectionStates.NotConnected:
                            connectionIndicator.GetComponent<Image>().color = Color.red;
                            break;
                        case ConnectionStates.Scanning:
                        case ConnectionStates.Connecting:
                            connectionIndicator.GetComponent<Image>().color = Color.yellow;
                            break;
                        default:
                            connectionIndicator.GetComponent<Image>().color = Color.grey;
                            break;
                    }
                }

            }
        }

        private void MoveToNextUIStage() {
            if (ConnectionUI != null)
                ConnectionUI.SetActive(false);
            if (afterConnectionStageUI != null)
                afterConnectionStageUI.SetActive(true);
        }
#endif
    }
}