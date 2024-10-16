using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Valkyrie.EIR.Utilities;
#if EIR_COMM
using Valkyrie.EIR.Bluetooth;
#endif
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif

namespace Valkyrie.EIR {

    /// <summary>
    /// Facade class to manage the instantiation and interaction between the three management classes required for EIR functionality.
    /// Handles cases in which one or more of the EIR managers is not required.
    /// </summary>
    public class EIRManager : MonoBehaviour {

        public static EIRManager Instance;

        #region Properties

#if EIR_HAPTICS
        public HapticManager Haptics { get { return hapticManager; } }
        private HapticManager hapticManager;
#endif
#if EIR_COMM
        public EirBluetoothBridge Communication { get { return eirBluetoothBridge; } }
        private EirBluetoothBridge eirBluetoothBridge;
#endif
#if EIR_INTERACTION
        public InteractionManager Interaction { get { return interactionManager; } }
        private InteractionManager interactionManager;
#endif

        /// <summary>
        /// Returns true when all requisite systems and permissions are initialised and granted.
        /// </summary>
        public bool Initialised { get; private set; }

        #endregion

        #region Events

        // called when the EIR manager has been disposed, to notify any component which needs to be aware of this action.
        public Action OnDisposed;

#if EIR_COMM
        // called when the Bluetooth plugin is initialised, or fails to initialise.
        public Action<bool> OnBluetoothInitialised;
        // called when the user denies permissions for bluetooth.
        public Action OnPermissionsDenied;
#endif

        #endregion

        #region Unity Methods

        private void Awake() {
            if (Instance == null) Instance = this;
            else {
                Debug.Log("[EIR Manager] Duplicate instance detected. Destroying...");
                Destroy(this);
                return;
            }
            Debug.Log("[EIR Manager] EIR Manager Starting...");

            DontDestroyOnLoad(this);
            if (EIRConfig.Instance.AutoInitiaise) Initialise();
        }


        private void Update() {
            if (!Initialised) return;
#if EIR_COMM && UNITY_ANDROID && !UNITY_EDITOR
            // read the device vitals (connection states, battery levels) of the connected EIR device.
            if (eirBluetoothBridge != null) eirBluetoothBridge.ReadDeviceVitals();
#endif
        }

        private void LateUpdate() {
            if (!Initialised) return;
#if EIR_HAPTICS && EIR_COMM && UNITY_ANDROID && !UNITY_EDITOR
            if (eirBluetoothBridge != null && eirBluetoothBridge.IsActive) {
                sbyte[] signal = hapticManager.GenerateHapticSignalForSendFrequency();
                if (signal.Length > 0) {
                    if (signal[signal.Length - 1] != 0 && EIRConfig.Instance.OutputHapticDebug) Debug.Log($"[EIR Manager] Sending signal to EMS device: {signal[signal.Length - 1]}");
                    eirBluetoothBridge.WriteBytesToDevice(signal);
                }
            }
#endif
#if EIR_HAPTICS
            hapticManager.Reset();
#endif

        }

        private void OnApplicationQuit() {
#if EIR_COMM
            if (eirBluetoothBridge != null) eirBluetoothBridge.Disconnect();
#endif
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialises the EIR Manager and any managers required by the current configuration.
        /// </summary>
        public void Initialise() {

            Debug.Log("[EIR Manager] Instantiating required modules...");
#if EIR_HAPTICS
            hapticManager = new HapticManager(gameObject);
#endif
#if EIR_INTERACTION
            interactionManager = new InteractionManager();
#if EIR_HAPTICS
            interactionManager.HapticRequest += OnHapticRequest;
            Interactable.OnHapticPresetRequested += OnHapticPresetRequested;
            Interactable.OnHapticPresetTypeRequested += OnHapticPresetTypeRequested;
#endif
#endif
#if EIR_COMM
            eirBluetoothBridge = new EirBluetoothBridge();
#endif
#if EIR_COMM && EIR_HAPTICS
            EirBluetoothBridge.OnConnectionStateChanged += OnConnectionStateChanged;
            DeviceManager.OnRequestDevices += OnRequestDevices;
            DeviceManager.OnConnectionRequest += OnConnectionRequest;
            if (gameObject.GetComponent<MainThreadDispatcher>() == null) gameObject.AddComponent<MainThreadDispatcher>();

#if UNITY_EDITOR
            // automatically flag initialised if in editor.
            Initialised = true;
            OnBluetoothInitialised?.Invoke(true);
#endif
            RequestPermissions();
#else
            Initialised = true;
#endif
        }
        #endregion

        #region Public Methods

#if EIR_COMM && EIR_HAPTICS
        /// <summary>
        /// Toggles on or off the bluetooth data write.
        /// When enabled, sends a configuration signal to the connected device.
        /// </summary>
        /// <param name="send"></param>
        public void ToggleBluetoothSend(bool send) {

            Debug.Log($"[EIR Manager] Toggling BT Communication {(send ? "On" : "Off")}");

            eirBluetoothBridge.IsActive = send;

            if (!send) {
                hapticManager.Reset();
                eirBluetoothBridge.WriteBytesToDevice(hapticManager.GenerateHapticSignal(), true);
            }
        }

        /// <summary>
        /// Requests bluetooth permisions. Will invoke permissions granted/permissions refused event.
        /// </summary>
        public void RequestPermissions() {
            BluetoothPermissions.OnPermissionsGranted += OnPermissionsResult;
            BluetoothPermissions.AskForPermissions();
        }
#endif

        /// <summary>
        /// Shuts down and cleans up the EIR Manager.
        /// </summary>
        public void Dispose() {

            Debug.Log($"[EIR Manager] EIR Manager is being disposed.");

#if EIR_INTERACTION && EIR_HAPTICS
            if (interactionManager != null) {
                interactionManager.HapticRequest -= OnHapticRequest;
                Interactable.OnHapticPresetRequested -= OnHapticPresetRequested;
                Interactable.OnHapticPresetTypeRequested -= OnHapticPresetTypeRequested;
            }
#endif

#if EIR_INTERACTION
            interactionManager = null;
#endif

#if EIR_HAPTICS
            hapticManager = null;
#endif

#if EIR_COMM
            if (eirBluetoothBridge != null) {
                Communication.Disconnect();
#if EIR_HAPTICS
                EirBluetoothBridge.OnConnectionStateChanged -= OnConnectionStateChanged;
#endif
                DeviceManager.OnRequestDevices -= OnRequestDevices;
                DeviceManager.OnConnectionRequest -= OnConnectionRequest;
                if (gameObject.GetComponent<MainThreadDispatcher>() != null) Destroy(gameObject.GetComponent<MainThreadDispatcher>());
                BluetoothPermissions.OnPermissionsGranted -= OnPermissionsResult;
            }
            eirBluetoothBridge = null;
#endif
            OnDisposed?.Invoke();
            Destroy(this);
        }

        #endregion

        #region Event Handlers

#if EIR_INTERACTION && EIR_HAPTICS
        /// <summary>
        /// Invokes when the EIR calibration is changed.
        /// </summary>
        /// <param name="lowerLimits"></param>
        /// <param name="upperLimits"></param>
        /// <param name="calibrationIndex"></param>
        private void OnCalibrationUpdated(int[] lowerLimits, int[] upperLimits, int[] calibrationIndex) {
            hapticManager.SetCalibration(lowerLimits, upperLimits, calibrationIndex);
        }

        /// <summary>
        /// Invokes when an action has requested a Haptic Preset.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="preset"></param>
        private void OnHapticPresetRequested(BodyPart bodyPart, HapticPreset preset) {
            hapticManager.CreateHapticPresetRunner(bodyPart, preset);
        }

        /// <summary>
        /// Invokes when an action has requested a Haptic Preset by type.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="preset"></param>
        private void OnHapticPresetTypeRequested(BodyPart bodyPart, HapticPreset.PresetType preset) {
            hapticManager.CreateHapticPresetRunner(bodyPart, HapticPreset.CreateDefaultPreset(preset));
        }
#endif

#if EIR_COMM && EIR_HAPTICS
        /// <summary>
        /// Invokes by the Eir Bluetooth Bridge each time the Bluetooth connection state changes.
        /// </summary>
        /// <param name="conState"></param>
        private void OnConnectionStateChanged(ConnectionStates conState) {
            if (conState == ConnectionStates.Connected) {
                eirBluetoothBridge.SendConfigSignal(hapticManager.GenerateConfigSignal());
                ToggleBluetoothSend(true);
            } else if (eirBluetoothBridge.PreviousState == ConnectionStates.Connected) {
                hapticManager.SetUnconfigured();
                ToggleBluetoothSend(false);
            }
        }
#endif

#if EIR_INTERACTION && EIR_HAPTICS
        /// <summary>
        /// Invoked on InteractionManager when a Haptic feedback pulse is requested.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="force"></param>
        /// <param name="bypassCalibration"></param>
        private void OnHapticRequest(BodyPart bodyPart, float force, bool bypassCalibration) {
            Debug.Log($"[EIR Manager] OnHapticRequest from Interaction system {(int)bodyPart} {force}");
            hapticManager.AddHapticIntensity((int)bodyPart, force, bypassCalibration);
        }
#endif

#if EIR_COMM

        /// <summary>
        /// Invoked by the EIR Bluetooth Bridge when the system is initialised. 
        /// Invokes an event to be consumed by whatever system needs to know this information.
        /// </summary>
        /// <param name="initialisd"></param>
        public void OnInitialisationComplete(bool initialisd) {
            Debug.Log($"[EIR Manager] Initialisation {(initialisd ? "Complete" : "Failed")}");
            OnBluetoothInitialised?.Invoke(initialisd);
        }

        /// <summary>
        /// Invoked on DeviceManager when it becomes active.
        /// This function will pass it the device addresses and names to generate the required UI buttons.
        /// </summary>
        /// <param name="dm"></param>
        private void OnRequestDevices(DeviceManager dm) {
            List<KeyValuePair<string, string>> devices = new List<KeyValuePair<string, string>>();
            foreach (BluetoothDeviceInfo device in eirBluetoothBridge.DeviceList.devices) {
                devices.Add(new KeyValuePair<string, string>(device.address, device.name));
            }
            dm.InstantiateButtons(devices);
        }

        /// <summary>
        /// Invoked on DeviceManager when a device is selected.
        /// Will invoke the connect routine for the input address.
        /// </summary>
        /// <param name="address"></param>
        private void OnConnectionRequest(string address) {
            _ = eirBluetoothBridge.Connect(address);
        }

        /// <summary>
        /// Invoked by the Permissions System when permissions are granted or denied.
        /// </summary>
        /// <param name="granted"></param>
        private void OnPermissionsResult(bool granted) {
            BluetoothPermissions.OnPermissionsGranted -= OnPermissionsResult;
            if (granted) {
                eirBluetoothBridge.Initialise(OnInitialisationComplete);
                Initialised = true;
            } else {
                Debug.LogWarning("[EIR Manager] Bluetooth Permissions have been denied. EIR will not function.");
                OnPermissionsDenied?.Invoke();
            }
        }
#endif
#endregion
    }
}