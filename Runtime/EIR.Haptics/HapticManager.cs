using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Haptics
{
    /// <summary>
    /// Haptics Manager deals with calibration and EMS signals.
    /// It receives intensities and converts them into EMS-based messages (based on calibration values) and sends them to the Communication Manager.
    /// </summary>
    [Serializable]
    public class HapticManager {

        public bool stopAllHapticSignals;

        #region Events

        public delegate void CalibrationUpdatedEventHandler(int left, int right, bool dontSync);
        public static event CalibrationUpdatedEventHandler CalibrationUpdated;

        #endregion

        #region Consts

        // constants used to configure the device
        public static readonly byte CONST_FREQUENCY = 100;
        public static readonly byte CONST_PULSE_WIDTH = 100;
        public const int CALIBRATION_INDEX_LENGTH = 10;

        #endregion

        #region Properties

        /// <summary>
        ///Denotes current EMS being broadcast as a percentage of the current calibration. Updates every frame
        /// </summary>
        public float[] IndicatorSignal { get; private set; } = { 0, 0 };


        //Integers that define the user's current calibration
        public static int[] LowerLimits { get; private set; }
        public static int[] UpperLimits { get; private set; }
        public static int[] CalibrationIndex { get; private set; }

        public bool Configured { get { return configured; } }

        public ConfigSignalClass ConfigSignal {
            get {
                if (configSignal == null) {
                    configSignal = new ConfigSignalClass(0, CONST_FREQUENCY, CONST_PULSE_WIDTH);
                }
                return configSignal;
            }
        }

        public int[] MappedIntensities {
            get { return mappedIntensities; }
        }

        public HapticManager(GameObject runner) {

            Debug.Log("[Haptic Manager] Haptic Manager initialised.");
            runnerObject = runner;
            UpperLimits = new int[2] { 0, 0, };
            LowerLimits = new int[2] { 0, 0, };
            CalibrationIndex = new int[2] { 0, 0, };

            runners = new List<HapticPresetRunner>();

            hapticSignal = new HapticSignal {
                enableEms = 1,
                config = 0
            };

            HapticPresetRunner.OnHapticPresetRequest += OnHapticPresetRequest;
        }

        #endregion

        #region Private Variables

        //Values used for calculating the minimum and maximum calibration given an index
        private float calibrationRange = 50;
        private float calibrationMin = 0;
        private float calibrationMax = 255.0f;
        private float calibrationMinStep = 10.0f;
        private float calibrationMinMultiplier = 0.5f;
        private float calibrationMinOffset = 0.0f;
        private float calibrationMaxMultiplier = 2.15f;
        private float calibrationMaxOffset = 40.0f;

        private GameObject runnerObject;

        //Has the device been configured?
        private bool configured;

        private int frameCounter = 0;

        //Parts of the signal that is sent at the end of each frame
        private float[] intensities = new float[2];
        private int[] mappedIntensities = new int[2];
        private HapticSignal hapticSignal;
        private List<HapticPresetRunner> runners;

        private ConfigSignalClass configSignal;

        #endregion

        #region Public Methods

        public sbyte[] GenerateHapticSignalForSendFrequency() {
            bool send = EIRConfig.Instance.BluetoothSendFrequency == BluetoothSendFrequency.EveryFrame;

            if (!send) {
                if (frameCounter == (int)EIRConfig.Instance.BluetoothSendFrequency) {
                    send = true;
                    frameCounter = 0;
                }
                else {
                    frameCounter++;
                }
            }

            if (send) {
                HapticSignal signal = CreateHapticSignalFromIntensities(mappedIntensities);

                Reset();
                return getBytes(signal);
            }
            Reset();
            return new sbyte[0];
        }

        /// <summary>
        /// Reset the mapped intensities to 0 once the signal has been sent.
        /// </summary>
        public void Reset() {
            for (int i = 0; i < intensities.Length; i++) {
                intensities[i] = 0;
                mappedIntensities[i] = 0;
            }
        }

        public void SetUnconfigured() {
            configured = false;
        }

        public sbyte[] GenerateHapticSignal() {
            HapticSignal signal = CreateHapticSignalFromIntensities(mappedIntensities);
            return getBytes(signal);
        }


        //Configure the hardware
        public sbyte[] GenerateConfigSignal(int gain, byte frequency, byte pulseWidth) {
            Debug.Log("[Haptic Manager] Sending Config Signal");
            try {
                Debug.Log($"[Haptic Manager] Device configured with gain: {gain} frequency: {frequency} pulse width: {pulseWidth}");
                HapticSignal h = CreateConfigSignal(gain, frequency, pulseWidth);
                configured = true;
                return getBytes(h);
            }
            catch (Exception e) {
                Debug.LogError($"[Haptic Manager] Config Signal Exception {e}");
                configured = false;
                throw e;
            }
        }

        //Configure the hardware
        public sbyte[] GenerateConfigSignal() {
            Debug.Log("[Haptic Manager] Sending Config Signal");
            try {

                ConfigSignalClass c = ConfigSignal;
                Debug.Log($"[Haptic Manager] Device configured with gain: {c.Gain} frequency: {c.Frequency} pulse width: {c.PulseWidth}");
                HapticSignal h = CreateConfigSignal(c.Gain, c.Frequency, c.PulseWidth);
                configured = true;
                return getBytes(h);
            }
            catch (Exception e) {
                Debug.LogError($"[Haptic Manager] Config Signal Exception {e}");
                configured = false;
                throw e;
            }
        }

        #endregion 

        #region Calibration

        public static void SetCalibration(int[] lLimits, int[] uLimits, int[] cIndexes) {
            Debug.Log("[Calibration] Setting calibration to " + uLimits[0] + ", & " + uLimits[1]);
            LowerLimits = lLimits;
            UpperLimits = uLimits;
            CalibrationIndex = cIndexes;
        }

        //Alter the user's current calibration using the provided index
        public void ModifyCalibrationByIndex(bool isLeft, int index, bool dontSync = false) {

            float[] minMax = GetLowerUpperLimitForIndex(index, 2);

            LowerLimits[isLeft ? 1 : 0] = (int)minMax[0];
            UpperLimits[isLeft ? 1 : 0] = (int)minMax[1];

            CalibrationUpdated?.Invoke(CalibrationIndex[1], CalibrationIndex[0], dontSync);

        }

        //Calculate a minimum and maximum value for calibration using the given index
        public float[] GetLowerUpperLimitForIndex(int index, int powerLaw = 1) {
            float[] minmax;
            float min = 0;
            float max = 0;
            if (powerLaw == 1) {
                float step = (calibrationMax - calibrationRange) / CALIBRATION_INDEX_LENGTH;
                max = calibrationMin + index * step + calibrationRange;
                min = calibrationMinStep * index;
            }
            else if (powerLaw == 2) {
                min = calibrationMinOffset + calibrationMinMultiplier * index * index;
                max = calibrationMaxOffset + calibrationMaxMultiplier * index * index;
            }
            minmax = new float[] { min, max };

            return minmax;
        }

        #endregion

        #region Create & Send EMS Message

        //Adds haptic intensity to a bodypart, based on the calibration
        public void AddHapticIntensity(int bodyPart, float intensity, bool bypassCalibration = false) {

            if (EIRConfig.Instance.OutputHapticDebug) Debug.Log($"[Haptics] AddHapticIntensity for BodyPart {bodyPart} with intensity {intensity}");
            // Currently: if the bodypart is not hands, don't do anything
            if (bodyPart != 0 && bodyPart != 1) return;

            intensities[bodyPart] += intensity;

            mappedIntensities[bodyPart] = bypassCalibration ? (int)intensities[bodyPart] : (int)ClampMappedForce(bodyPart, intensities[bodyPart]);
            IndicatorSignal[bodyPart] = bypassCalibration ? intensities[bodyPart] / 255.0f : intensities[bodyPart];
        }
        #endregion

        #region Presets

        //Creates a new HapticPresetRunner instance and attaches it to the runnerObject
        public HapticPresetRunner CreateHapticPresetRunner(BodyPart affectedBodyPart, HapticPreset props, float startIntensity = 1, bool runNow = true) {
            HapticPresetRunner runner = runnerObject.AddComponent<HapticPresetRunner>();    
            runners.Add(runner);


            List<BodyPart> affectedBodyParts = new List<BodyPart> {
                affectedBodyPart
            };

            runner.SetupRunner(affectedBodyParts, props, startIntensity, runNow);

            return runner;
        }


        //Overload that instead uses a list of body parts so that a HapticPresetRunner can apply to multiple limbs
        public HapticPresetRunner CreateHapticPresetRunner(List<BodyPart> affectedBodyParts, HapticPreset props, float startIntensity = 1, bool runNow = true) {
            HapticPresetRunner runner = runnerObject.AddComponent<HapticPresetRunner>();
            runners.Add(runner);
            runner.SetupRunner(affectedBodyParts, props, startIntensity, runNow);

            return runner;
        }

        //Get all HapticPresetRunners that affect a certain limb
        public List<HapticPresetRunner> GetHapticPresetRunnerByLimb(BodyPart affectedBodyPart) {

            List<HapticPresetRunner> correctRunners = new List<HapticPresetRunner>();

            for (int i = 0; i < runners.Count; i++) {
                if (runners[i].m_affectedBodyParts.Contains(affectedBodyPart) || (int)affectedBodyPart == -1) {
                    correctRunners.Add(runners[i]);
                }
            }

            return correctRunners;
        }

        //Stops the HapticPresetRunners running on a single body part, or all of them with no argument
        public void StopHapticPresetRunner(BodyPart affectedBodyPart = (BodyPart)(-1)) {

            for (int i = runners.Count - 1; i >= 0; i--) 
            {
                if (runners[i].m_affectedBodyParts.Contains(affectedBodyPart) || (int)affectedBodyPart == -1) {
                    runners[i].Stop();
                    runners.Remove(runners[i]);
                }
            }
        }

        //Stop the HapticPresetRunners running on a list of body parts
        public void StopHapticPresetRunner(List<BodyPart> affectedBodyParts) {

            for (int i = runners.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < affectedBodyParts.Count; j++) {
                    if (runners[i].m_affectedBodyParts.Contains(affectedBodyParts[j])) {
                        runners[i].Stop();
                        runners.Remove(runners[i]);
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnHapticPresetRequest(int bodyPart, float intensity) {
            AddHapticIntensity(bodyPart, intensity);
        }

        #endregion


        #region Support Functions

        //Clamps a given fraction between the calibration limits for that limb
        private float ClampMappedForce(int part, float forceFraction) {
            // Clamp it between 0.0 and 1.0;
            if (forceFraction <= 0)
                return 0;
            if (forceFraction > 1)
                return UpperLimits[part];
            //maps amplitude depending on calibration values

            float remap = Mathf.Lerp(LowerLimits[part], UpperLimits[part], forceFraction);

            // Protect people from receiving minimum calibration value, when there is no force applied:
            if (remap == LowerLimits[part])
                remap = 0;
            return (int)remap;
        }

        private HapticSignal CreateHapticSignalFromIntensities(int[] value) {
            HapticSignal haptics = hapticSignal;
            haptics.config = 0;

            haptics.master_intensity_LSB = (byte)value[0];
            haptics.master_intensity_MSB = (byte)value[0]; // RIGHT

            haptics.slave_intensity_LSB = (byte)value[1];
            haptics.slave_intensity_MSB = (byte)value[1]; // LEFT


            hapticSignal = haptics;
            return haptics;
        }


        //Creates signal that configures the device
        private HapticSignal CreateConfigSignal(int gain, byte frequency, byte pulse_width) {
            HapticSignal _haptics = hapticSignal;
            _haptics.config = 1;

            switch (gain) {
                case 0:
                    _haptics.gain_0 = 0;
                    _haptics.gain_1 = 0;
                    break;
                case 1:
                    _haptics.gain_0 = 0;
                    _haptics.gain_1 = 1;
                    break;
                case 2:
                    _haptics.gain_0 = 1;
                    _haptics.gain_1 = 0;
                    break;
                case 3:
                    _haptics.gain_0 = 1;
                    _haptics.gain_1 = 1;
                    break;
            }
            _haptics.frequency = frequency;
            _haptics.pulse_width = pulse_width;

            return _haptics;
        }

        //Creates byte array from a formed signal
        private sbyte[] getBytes(HapticSignal str) {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            sbyte[] writeBytes = new sbyte[arr.Length];
            for (int i = 0; i < arr.Length; i++) {
                writeBytes[i] = (sbyte)arr[i];
            }
            return writeBytes;
        }
        #endregion

        #region Serializable Classes

        public class ConfigSignalClass {
            public int Gain;
            public byte Frequency;
            public byte PulseWidth;

            public ConfigSignalClass(int gain, byte frequency, byte pulseWidth) {
                Gain = gain;
                Frequency = frequency;
                PulseWidth = pulseWidth;
            }
        }

        #endregion
    }
}