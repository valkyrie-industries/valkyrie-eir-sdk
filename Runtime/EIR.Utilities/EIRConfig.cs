using System;
using UnityEngine;

namespace Valkyrie.EIR.Utilities {

    public enum BluetoothSendFrequency { EveryFrame, EverySecond, EveryThird }

    public class EIRConfig : ScriptableObject {

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

        public bool UsingHpt { get { return enableHapticsManager; } }
        public bool UsingCom { get { return enableBTEirBluetoothBridge; } }
        public bool UsingInt { get { return enableInteractionManager; } }
        public bool IgnoreCache { get { return ignoreCachedDevice; } }
        public int VitalsReadInterval { get { return vitalsReadInterval; } }
        public string DeviceFilter { get { return deviceFilter; } }
        public bool AutoInitiaise { get { return autoInitialise; } }    

        public bool UseOVRForVibrations { get { return useOVRForVibrations; } }

        [SerializeField] private bool enableHapticsManager;
        [SerializeField] private bool enableBTEirBluetoothBridge;
        [SerializeField] private bool enableInteractionManager;
        [SerializeField] private bool outputHapticDebug;
        [SerializeField] private bool ignoreCachedDevice;
        [SerializeField] private int vitalsReadInterval;
        [SerializeField] private string deviceFilter = "Valkyrie";
        [SerializeField] private bool autoInitialise = true;
        [SerializeField] private bool useOVRForVibrations;

        public bool OutputHapticDebug { get { return outputHapticDebug; } }
        public BluetoothSendFrequency BluetoothSendFrequency { get { return bluetoothSendFrequency; } set { bluetoothSendFrequency = value; } }
        [SerializeField]
        private BluetoothSendFrequency bluetoothSendFrequency;


    }
}