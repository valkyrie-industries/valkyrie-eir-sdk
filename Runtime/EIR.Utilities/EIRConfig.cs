using System;
using UnityEngine;

namespace Valkyrie.EIR.Utilities {

    public enum BluetoothSendFrequency { EveryFrame, EverySecond, EveryThird }

    /// <summary>
    /// Configuration scriptable object to facilitate quick modification of EIR configration values.
    /// </summary>
    public class EIRConfig : ScriptableObject {

        #region Static Accessor

        private static EIRConfig instance;                        // Singleton instance. Only one EIR Config is permitted to exist.

        public static EIRConfig Instance {                        // Get this instance, and load of not already loaded.
            get {
                if (instance != null) return instance;
                {
                    EIRConfig loadedConfig = Resources.Load("Valkyrie Config/EIRConfig", typeof(EIRConfig)) as EIRConfig;

                    if (loadedConfig == null) {
                        throw new Exception("[Valkyrie.EIR SDK] Failed to load EIRConfig from resources. Please generate an EIRConfig with Valkyrie Tools.");
                    }

                    instance = loadedConfig;
                    return instance;
                }
            }
        }

        #endregion

        #region Public Properties

        public bool UsingHpt { get { return enableHapticsManager; } set { enableHapticsManager = value; } }
        public bool UsingCom { get { return enableBTEirBluetoothBridge; } set { enableBTEirBluetoothBridge = value; } }
        public bool UsingInt { get { return enableInteractionManager; } }
        public bool IgnoreCache { get { return ignoreCachedDevice; } }
        public int VitalsReadInterval { get { return vitalsReadInterval; } }
        public string DeviceFilter { get { return deviceFilter; } }
        public bool AutoInitiaise { get { return autoInitialise; } }    

        public bool UseOVRForVibrations { get { return useOVRForVibrations; } }
        public bool OutputHapticDebug { get { return outputHapticDebug; } }
        public BluetoothSendFrequency BluetoothSendFrequency { get { return bluetoothSendFrequency; } set { bluetoothSendFrequency = value; } }

        #endregion

        #region Serialized Variables

        [SerializeField] private bool enableHapticsManager;
        [SerializeField] private bool enableBTEirBluetoothBridge;
        [SerializeField] private bool enableInteractionManager;

        [SerializeField] private bool outputHapticDebug;
        [SerializeField] private bool ignoreCachedDevice;
        [SerializeField] private int vitalsReadInterval;
        [SerializeField] private string deviceFilter = "Valkyrie";
        [SerializeField] private bool autoInitialise = true;
        [SerializeField] private bool useOVRForVibrations;
        [SerializeField] private BluetoothSendFrequency bluetoothSendFrequency;

        #endregion
    }
}