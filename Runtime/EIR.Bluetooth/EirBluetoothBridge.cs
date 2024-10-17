using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Bluetooth {

    public enum ConnectionStates {
        NotConnected,
        Scanning,
        Connecting,
        Connected,
        Reconnecting,
        Found,
        NotFound,
        Selection
    }

    /// <summary>
    /// Bridge between the EIR Bluetooth Java plugin and the Unity application.
    /// </summary>
    public class EirBluetoothBridge {

        #region Android Class Definition

        private AndroidPluginCallback callbackInstance;
        private AndroidJavaClass eirBlu;

        private const string JAVA_CLASS_DEF = "com.valkyrieindustries.eirbluetooth.EirBluetooth";
        private const string JAVA_CALLBACK_DEF = "com.valkyrieindustries.eirbluetooth.EirBluetooth$EirBTCallbacks";
        private const string UNITY_ACTIVITY_DEF = "com.unity3d.player.UnityPlayer";

        #endregion

        #region Events

        private Action<bool> initialisationCallback;

        public delegate void OnConnectionStateChangedEventHandler(ConnectionStates connectionState);
        public static event OnConnectionStateChangedEventHandler OnConnectionStateChanged;

        public static Action<bool> OnDataWritten;

        #endregion

        #region Variables and Properties

        private ConnectionStates state;
        private ConnectionStates prevState;
        private bool isActive;
        private bool autoConnect;
        private BluetoothDeviceList deviceList = new BluetoothDeviceList();
        private DeviceVitals deviceVitals = new DeviceVitals(false, false, 0, 0, 0, 0);
        private string searchFilter = EIRConfig.Instance.DeviceFilter;
        private bool initialised;
        private float readInterval;
        private List<IEirBluetooth> handlers = new List<IEirBluetooth>();
        
        /// <summary>
        /// Returns the name of the current connected bluetooth device, if available.
        /// </summary>
        public static string deviceName { get; private set;}

        /// <summary>
        /// The current Bluetooth connection state.
        /// </summary>
        public ConnectionStates CurrentState { get { return state; } }

        /// <summary>
        /// The previous Bluetooth connection state.
        /// </summary>
        public ConnectionStates PreviousState { get { return prevState; } }

        /// <summary>
        /// The activity state of the Bluetooth send routine.
        /// If true, the characteristic write is actively being written to, if false no data is sent.
        /// </summary>
        public bool IsActive {
            get { return isActive; }
            set {
                bool oldValue = isActive;
                isActive = value;
                if (isActive != oldValue) {
                    foreach (IEirBluetooth handler in handlers) {
                        if (handler == null) continue;
                        if (isActive) handler.OnBluetoothEnable();
                        else handler.OnBluetoothDisable();
                    }
                }
            }
        }

        /// <summary>
        /// List of devices found by the device scan.
        /// </summary>
        public BluetoothDeviceList DeviceList { get { return deviceList; } }

        /// <summary>
        /// Returns the device vitals data. Battery status and connection status.
        /// </summary>
        public DeviceVitals Vitals { get { return deviceVitals; } }

        /// <summary>
        /// Returns true if the Bluetooth device is connected.
        /// </summary>
        public bool IsConnected { get {
                return state == ConnectionStates.Connected; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialise the BLE communication system.
        /// </summary>
        public void Initialise(Action<bool> callback) {
            if (initialised) return;
            initialisationCallback = callback;

            readInterval = EIRConfig.Instance.VitalsReadInterval;

            AndroidJavaObject activity = new AndroidJavaClass(UNITY_ACTIVITY_DEF).GetStatic<AndroidJavaObject>("currentActivity");

            if (activity == null) Debug.Log("[EIR Bluetooth] No activity");
            else Debug.Log($"[EIR Bluetooth] Activity: {activity}");

            eirBlu = new AndroidJavaClass(JAVA_CLASS_DEF);

            callbackInstance = new AndroidPluginCallback();

            callbackInstance.OnInitialisationCompleteEvent += OnInitialisationComplete;
            callbackInstance.OnDeviceFoundEvent += HandleDeviceFound;
            callbackInstance.OnConnectedEvent += OnConnected;
            callbackInstance.OnDisconnectEvent += OnDisconnected;
            callbackInstance.OnReconnectionEvent += OnReconnection;
            callbackInstance.OnReadEvent += OnRead;
            callbackInstance.OnWriteEvent += OnWrite;

            eirBlu.CallStatic("initialise", activity, callbackInstance, 3000l);
            initialised = true;
        }

        /// <summary>
        /// Simulate a connection loss event. Intended for development purposes.
        /// </summary>
        public void SimulateDisconnect() {
            eirBlu.CallStatic("simulateDeviceDisconnection");
        }

        /// <summary>
        /// Scan for nearby EIR devices.
        /// </summary>
        public void Scan() {
            autoConnect = false;
            state = ConnectionStates.Scanning;
            eirBlu.CallStatic("startScan", searchFilter);
        }

        /// <summary>
        /// Scan for nearby EIR devices, and connect if only one is found.
        /// If multiple devices are found, any UX calling this method should proceed to a device selection screen.
        /// </summary>
        /// <param name="auto"></param>
        /// <returns></returns>
        public async Task<ConnectionStates> ScanAndConnect(bool auto = true) {
            autoConnect = auto;

            TaskCompletionSource<ConnectionStates> tcs = new TaskCompletionSource<ConnectionStates>();

            OnConnectionStateChangedEventHandler handler = null;
            handler = (connectionState) => {
                if (auto) {
                    if (connectionState == ConnectionStates.Connected || connectionState == ConnectionStates.NotConnected) {
                        OnConnectionStateChanged -= handler;
                        tcs.TrySetResult(connectionState);
                    }
                }
                else {
                    if (connectionState == ConnectionStates.Found || connectionState == ConnectionStates.NotFound || connectionState == ConnectionStates.Connected || connectionState == ConnectionStates.NotConnected || connectionState == ConnectionStates.Selection) {
                        OnConnectionStateChanged -= handler;
                        tcs.TrySetResult(connectionState);
                    }
                }
            };

            OnConnectionStateChanged += handler;

            eirBlu.CallStatic("startScan", searchFilter);
            state = ConnectionStates.Scanning;

            return await tcs.Task;
        }

        /// <summary>
        /// Connect to the device at the input mac address.
        /// </summary>
        /// <param name="macAddress"></param>
        public async Task<ConnectionStates> Connect(string macAddress) {

            TaskCompletionSource<ConnectionStates> tcs = new TaskCompletionSource<ConnectionStates>();

            OnConnectionStateChangedEventHandler handler = null;
            handler = (connectionState) => {
                if (connectionState == ConnectionStates.Connected || connectionState == ConnectionStates.NotConnected) {
                    OnConnectionStateChanged -= handler;
                    tcs.TrySetResult(connectionState);
                }
            };

            OnConnectionStateChanged += handler;

            eirBlu.CallStatic("connectToDevice", macAddress);
            return await tcs.Task;
        }

        /// <summary>
        /// Terminate active connection.
        /// </summary>
        public void Disconnect() {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (eirBlu != null) eirBlu.CallStatic("disconnect");
            state = ConnectionStates.NotConnected;
#endif
        }

        /// <summary>
        /// Sends a configuration signal to the EIR bands, if the sendstate is active.
        /// </summary>
        /// <param name="data"></param>
        public void SendConfigSignal(sbyte[] data) {
            if (state == ConnectionStates.NotConnected || !isActive) return;
            eirBlu.CallStatic("writeCharacteristic", data, true);
        }

        /// <summary>
        /// Reads the device vitals dependent on the read interval frequency.
        /// This function should be called from an update loop.
        /// </summary>
        public void ReadDeviceVitals() {
            if (state == ConnectionStates.Connected) {
                readInterval -= Time.deltaTime;
                if (readInterval <= 0) {
                    readInterval = EIRConfig.Instance.VitalsReadInterval;
                    eirBlu.CallStatic("readCharacteristic");
                }
            }
        }

        /// <summary>
        /// Writes the input data to the EIR Bands' write characteristic.
        /// If critical, will attempt to send the signal again the if the java plugin does not receive the 'data received' callback.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="critical"></param>
        public void WriteBytesToDevice(sbyte[] data, bool critical = false) {
            if (state == ConnectionStates.NotConnected || !isActive) return;
            eirBlu.CallStatic("writeCharacteristic", data, critical);
        }

        /// <summary>
        /// Registers a new handler of IEirBluetooth interface.
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterHandler(IEirBluetooth handler) {
            Debug.Log("[EIR BT] Registering Handler");
            handlers.Add(handler);
        }

        /// <summary>
        /// Unregisters a handler of IEirBluetooth interface.
        /// </summary>
        /// <param name="handler"></param>
        public void UnregisterHandler(IEirBluetooth handler) {
            handlers.Remove(handler);
        }

        #endregion

        #region Event Handlers

        private void OnWrite(bool obj, bool obj2) {
            OnDataWritten?.Invoke(obj);

            foreach (IEirBluetooth handler in handlers) {
                if (handler == null) continue;
                handler.OnWrite();
            }
        }

        private void OnRead(byte[] obj) {
            Debug.Log($"[EIR Bluetooth] Read bytes: {DecodeBytesToString(obj)}");

            bool deviceRConnected = Convert.ToBoolean(obj[0]);
            bool deviceLConnected = Convert.ToBoolean(obj[2]);

            uint rBattery = Convert.ToUInt32(obj[1]);
            uint lBattery = Convert.ToUInt32(obj[3]);

            uint pulseWidth = Convert.ToUInt32(obj[4]);
            uint pulseFrequency = Convert.ToUInt32(obj[5]);

            deviceVitals.Update(deviceLConnected, deviceRConnected, lBattery, rBattery, pulseWidth, pulseFrequency);

            foreach (IEirBluetooth handler in handlers) {
                if (handler == null) continue;
                handler.OnUpdateVitals(deviceVitals);
            }

        }

        private void OnInitialisationComplete(bool success) {
            if (success) {
                Debug.Log("[EIR Bluetooth] Initialisation completed.");
                initialisationCallback(true);
            }
            else {
                throw new NotImplementedException("[EIR Bluetooth] Initialisation failure not yet implemented. In fact, even the plugin can't get here so if you're reading this, something went REALLY wrong!");
            }
        }

        private void OnConnected(string name) {
            MainThreadDispatcher.RunOnMainThread(() => {
                Debug.Log($"[EIR Bluetooth] OnConnected - Device: {name}");
                state = ConnectionStates.Connected;
                deviceName = name;
                OnConnectionStateChanged?.Invoke(state);
            });
        }

        private void OnDisconnected(bool reconnecting) {
            MainThreadDispatcher.RunOnMainThread(() => {
                Debug.Log($"[EIR Bluetooth] Disconnection event detected. {(reconnecting ? "Attempting reconnection." : "No Reconnection required.")}");
                ConnectionStates s = prevState;
                prevState = state;
                state = reconnecting ? ConnectionStates.Reconnecting : ConnectionStates.NotConnected;
                deviceName = "";
                OnConnectionStateChanged?.Invoke(state);
            });
        }

        private void OnReconnection(bool success) {
            MainThreadDispatcher.RunOnMainThread(() => {
                Debug.Log($"[EIR Bluetooth] Reconnection event detected. {(success ? "Success." : "Failed.")}");
                ConnectionStates s = prevState;
                prevState = state;
                state = success ? s : ConnectionStates.NotConnected;
                OnConnectionStateChanged?.Invoke(state);
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Deserialises the device found response from the java plugin.
        /// </summary>
        /// <param name="jsonArray"></param>
        private void HandleDeviceFound(string jsonArray) {
            jsonArray = string.Concat("{\"devices\":", jsonArray, "}");
            Debug.Log($"[EIR Bluetooth] Devices Found: {jsonArray}");

            MainThreadDispatcher.RunOnMainThread(() => {
                // deserialize the JSON array into a BluetoothDeviceList
                deviceList = JsonUtility.FromJson<BluetoothDeviceList>(jsonArray);

                if (deviceList.devices.Length == 1) {

                    Debug.Log($"[EIR Bluetooth] One device found.");


                    // Iterate through the list of devices
                    foreach (BluetoothDeviceInfo device in deviceList.devices) {
                        Debug.Log($"[EIR Bluetooth] Device Found - Name: {device.name}, Address: {device.address}, RSSI {device.rssi}");
                        if (autoConnect) {
                            state = ConnectionStates.Connecting;
                            eirBlu.CallStatic("connectToDevice", device.address);
                        }
                        else {
                            state = ConnectionStates.Found;
                        }
                    }
                }
                else if (deviceList.devices.Length > 1) {
                    Debug.Log($"[EIR Bluetooth] {deviceList.devices.Length} devices found.");
                    state = ConnectionStates.Selection;
                    OnConnectionStateChanged?.Invoke(state);

                }
                else {
                    Debug.Log($"[EIR Bluetooth] No devices returned.");

                    state = ConnectionStates.NotFound;
                }
                Debug.Log($"[EIR Bluetooth] Proceeding for State {state}.");
                OnConnectionStateChanged?.Invoke(state);
            });
        }

        /// <summary>
        /// Returns a string result from the input byte array.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        private string DecodeBytesToString(byte[] byteArray) {
            StringBuilder result = new StringBuilder();

            foreach (byte b in byteArray) {
                // Convert each byte to its hexadecimal representation
                string hex = b.ToString("X2");

                // Append the hexadecimal representation to the result string
                result.Append(hex).Append(" ");
            }

            // Remove the trailing space if needed
            if (result.Length > 0) {
                result.Length--; // Remove the last character (space)
            }

            return result.ToString();
        }

        #endregion

        #region Android Callbacks

        /// <summary>
        /// Bridge class facilitating communication between the Unity system and Android plugin's internal callbacks.
        /// This class acts as an intermediary, translating Android plugin events into Unity-compatible events.
        /// </summary>
        public class AndroidPluginCallback : AndroidJavaProxy {

            /// <summary>
            /// Initializes a new instance of the <see cref="AndroidPluginCallback"/> class.
            /// </summary>
            public AndroidPluginCallback() : base(JAVA_CALLBACK_DEF) { }

            /// <summary>
            /// Event triggered when a device is found during scanning.
            /// </summary>
            public event Action<string> OnDeviceFoundEvent;

            /// <summary>
            /// Event triggered upon successful connection to a device.
            /// </summary>
            public event Action<string> OnConnectedEvent;

            /// <summary>
            /// Event triggered when the initialization of the plugin is complete.
            /// </summary>
            public event Action<bool> OnInitialisationCompleteEvent;

            /// <summary>
            /// Event triggered upon disconnection from a device.
            /// </summary>
            public event Action<bool> OnDisconnectEvent;

            /// <summary>
            /// Event triggered upon successful reconnection to a previously disconnected device.
            /// </summary>
            public event Action<bool> OnReconnectionEvent;

            /// <summary>
            /// Event triggered upon receiving data from the connected device.
            /// </summary>
            public event Action<byte[]> OnReadEvent;

            /// <summary>
            /// Event triggered upon writing data to the connected device.
            /// </summary>
            public event Action<bool, bool> OnWriteEvent;

            /// <summary>
            /// Event triggered when low battery is detected on the connected device.
            /// </summary>
            public event Action<bool> OnLowBatteryDetectedEvent;

            /// <summary>
            /// Callback invoked when initialization of the plugin is complete.
            /// </summary>
            /// <param name="success">Indicates whether initialization was successful.</param>
            public void onInitialisationComplete(bool success) {
                OnInitialisationCompleteEvent?.Invoke(success);
            }

            /// <summary>
            /// Callback invoked when a device is found during scanning.
            /// </summary>
            /// <param name="devices">Information about the discovered device(s).</param>
            public void onDeviceFound(string devices) {
                OnDeviceFoundEvent?.Invoke(devices);
            }

            /// <summary>
            /// Callback invoked upon disconnection from a device.
            /// </summary>
            /// <param name="withReconnect">Indicates if a reconnection attempt will be made.</param>
            public void onDisconnect(bool withReconnect) {
                OnDisconnectEvent?.Invoke(withReconnect);
            }

            /// <summary>
            /// Callback invoked upon successful reconnection to a previously disconnected device.
            /// </summary>
            /// <param name="success">Indicates whether reconnection was successful.</param>
            public void onReconnection(bool success) {
                OnReconnectionEvent?.Invoke(success);
            }

            /// <summary>
            /// Callback invoked upon successful connection to a device.
            /// </summary>
            /// <param name="deviceName">Name of the connected device.</param>
            public void onConnectionEstablished(string deviceName) {
                OnConnectedEvent?.Invoke(deviceName);
            }

            /// <summary>
            /// Callback invoked upon receiving data from the connected device.
            /// </summary>
            /// <param name="data">Data received from the device.</param>
            public void onRead(sbyte[] data) {
                byte[] bytes = new byte[data.Length];
                for (int i = 0; i < data.Length; i++) {
                    bytes[i] = (byte)data[i];
                }
                OnReadEvent?.Invoke(bytes);
            }

            /// <summary>
            /// Callback invoked upon writing data to the connected device.
            /// </summary>
            /// <param name="success">Indicates whether the write operation was successful.</param>
            /// <param name="data">Data that was written to the device.</param>
            public void onWrite(bool success, bool critical) {
                OnWriteEvent?.Invoke(success, critical);
            }

            /// <summary>
            /// Callback invoked when low battery is detected on the connected device.
            /// </summary>
            /// <param name="lowBattery">Indicates whether low battery is detected.</param>
            public void onLowBatteryDetected(bool lowBattery) {
                OnLowBatteryDetectedEvent?.Invoke(lowBattery);
            }
        }

        #endregion

    }
}
