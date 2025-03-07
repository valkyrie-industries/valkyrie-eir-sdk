using System;
using UnityEngine;

namespace Valkyrie.EIR.Utilities {

    #region Enumerations

    public enum DeploymentType { XR, Mobile }

    public enum BluetoothSendFrequency { EveryFrame, EverySecondFrame, EveryThirdFrame }

    #endregion

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

        /// <summary>
        /// Enables the Valkyrie.EIR.Haptics namespace.
        /// </summary>
        public bool UsingHaptics { get { return enableHapticsManager; } set { enableHapticsManager = value; } }
        /// <summary>
        /// Enables the Valkyrie.EIR.Communication namespace.
        /// </summary>
        public bool UsingBluetoothCommunication { get { return enableBTEirBluetoothBridge; } set { enableBTEirBluetoothBridge = value; } }
        /// <summary>
        /// Enables the Valkyrie.EIR.Interaction namespace.
        /// </summary>
        public bool UsingInteraction { get { return enableInteractionManager; } }
        /// <summary>
        /// EirBluetoothBridge.ReadDeviceVitals run interval when connected (seconds) (min value: 1).
        /// </summary>
        public int VitalsReadInterval { get { return vitalsReadInterval; } }
        /// <summary>
        /// Use 0-19 (20 levels) for EIR Calibration. If false, use 0-10.
        /// </summary>
        public bool UseDuodecimalIndex { get { return useDuodecimalIndex; } }
        /// <summary>
        /// Returns the connection timeout (milleseconds).
        /// </summary>
        public long ConnectionTimeoutMs { get { return connectionTimeoutMs; } }
        /// <summary>
        /// Automatically calls EIRManager.Initialise if selected and the component is present in the scene. If unselected, EIRManager.Initialise must be called manually.
        /// </summary>
        public bool AutoInitialise { get { return autoInitialise; } }
        /// <summary>
        /// (XR Only) Use Meta OVR plugin for controller haptic vibrations.
        /// </summary>
        public bool UseOVRForVibrations { get { return useOVRForVibrations; } }
        /// <summary>
        /// Performs more verbose logging for haptic processes.
        /// </summary>
        public bool OutputHapticDebug { get { return outputHapticDebug; } }
        /// <summary>
        /// The frequency in which EirBluetoothBridge.WriteBytesToDevice is called on a framerate dependent basis.
        /// For XR projects, it is recommended to specify BluetoothSendFrequency as EverySecondFrame or EveryThirdFrame.
        /// </summary>
        public BluetoothSendFrequency BluetoothSendFrequency { get { return bluetoothSendFrequency; } set { bluetoothSendFrequency = value; } }
        /// <summary>
        /// Switches the deployment type for the project.
        /// Important! Injects manifest changes at build time that are specific to XR if XR is selected.
        /// Selecting XR for a Mobile Deployment will result in build rejection from Google Play Store.
        /// Selecting Mobile for XR Deployment will result in 2d windowed app on XR.
        /// </summary>
        public DeploymentType DeploymentType { get { return deploymentType; } }

        #endregion

        #region Serialized Variables

        [SerializeField] private bool enableHapticsManager;
        [SerializeField] private bool enableBTEirBluetoothBridge;
        [SerializeField] private bool enableInteractionManager;

        [SerializeField] private long connectionTimeoutMs = 5000L;

        [SerializeField] private bool outputHapticDebug;
        [SerializeField] private bool ignoreCachedDevice;
        [SerializeField] private int vitalsReadInterval;
        [SerializeField] private bool autoInitialise = true;
        [SerializeField] private bool useOVRForVibrations;
        [SerializeField] private BluetoothSendFrequency bluetoothSendFrequency;
        [SerializeField] private bool useDuodecimalIndex;
        [SerializeField] private DeploymentType deploymentType;
        #endregion
    }
}