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
    public class HapticManager
    {

        public bool stopAllHapticSignals;

        #region Events

        public delegate void CalibrationUpdatedEventHandler(int left, int right);
        public static event CalibrationUpdatedEventHandler CalibrationUpdated;

        #endregion

        #region Consts

        public const byte MIN_FREQUENCY = 10;
        public const byte MIN_PULSE_WIDTH = 30;
        public const byte CONST_FREQUENCY = 100;
        public const byte CONST_PULSE_WIDTH = 100;
        public int CALIBRATION_INDEX_LENGTH { get { if (EIRConfig.Instance.UseDuodecimalIndex) return 19; else return 10; } }

        #endregion

        #region Properties

        /// <summary>
        ///Denotes current EMS being broadcast as a percentage of the current calibration. Updates every frame
        /// </summary>
        public float[] IndicatorSignal { get; private set; } = { 0, 0 };

        /// <summary>
        /// The lower limits (EIR device) set by the current calibration.
        /// </summary>
        public int[] LowerLimits { get; private set; }
        /// <summary>
        /// The upper limits (EIR device) set by the current calibration.
        /// </summary>
        public int[] UpperLimits { get; private set; }
        /// <summary>
        /// The raw values (0-10) set by the current calibration.
        /// </summary>
        public int[] CalibrationIndex { get; private set; }

        /// <summary>
        /// Returns true if the HapticManager is configured.
        /// </summary>
        public bool Configured { get { return configured; } }

        /// <summary>
        /// Returns the current config signal.
        /// </summary>
        public ConfigSignalClass ConfigSignal
        {
            get
            {
                if (configSignal == null)
                {
                    configSignal = new ConfigSignalClass(0, CONST_FREQUENCY, CONST_PULSE_WIDTH);
                }
                return configSignal;
            }
        }

        /// <summary>
        /// Returns the current mapped intensity values (per device).
        /// </summary>
        public int[] MappedIntensities
        {
            get { return mappedIntensities; }
        }

        private float CalibrationMinMultiplier { get { return EIRConfig.Instance.UseDuodecimalIndex ? 0.15f : 0.5f; } }
        private float CalibrationMinOffset { get { return EIRConfig.Instance.UseDuodecimalIndex ? 0.0f : 0.0f; } }
        private float CalibrationMaxMultiplier { get { return EIRConfig.Instance.UseDuodecimalIndex ? 0.6235f : 2.15f; } }
        private float CalibrationMaxOffset { get { return EIRConfig.Instance.UseDuodecimalIndex ? 30f : 40f; } }


        #endregion

        #region Private Variables

        // for calculating the minimum and maximum calibration given an index
        private float calibrationRange = 50;
        private float calibrationMin = 0;
        private float calibrationMax = 255.0f;
        private float calibrationMinStep = 10.0f;

        private GameObject runnerObject;

        private bool configured;

        private int frameCounter = 0;

        // parts of the signal that is sent at the end of each frame
        private float[] intensities = new float[2];
        private int[] mappedIntensities = new int[2];
        private HapticSignal hapticSignal;
        private List<HapticPresetRunner> runners;

        private ConfigSignalClass configSignal;

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates a haptic signal if on a valid send frame. This is dependent on the BluetoothSendFrequency set via EIRConfig.
        /// </summary>
        /// <returns></returns>
        public sbyte[] GenerateHapticSignalForSendFrequency()
        {
            bool send = EIRConfig.Instance.BluetoothSendFrequency == BluetoothSendFrequency.EveryFrame;

            if (!send)
            {
                if (frameCounter == (int)EIRConfig.Instance.BluetoothSendFrequency)
                {
                    send = true;
                    frameCounter = 0;
                }
                else
                {
                    frameCounter++;
                }
            }

            if (send)
            {
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
        public void Reset()
        {
            for (int i = 0; i < intensities.Length; i++)
            {
                intensities[i] = 0;
                mappedIntensities[i] = 0;
            }
        }

        /// <summary>
        /// Flags the connected EIR device as unconfigured.
        /// </summary>
        public void SetUnconfigured()
        {
            configured = false;
        }

        /// <summary>
        /// Generates a haptic signal from the currently mapped intensities.
        /// </summary>
        /// <returns></returns>
        public sbyte[] GenerateHapticSignal()
        {
            HapticSignal signal = CreateHapticSignalFromIntensities(mappedIntensities);
            return getBytes(signal);
        }


        /// <summary>
        /// Generates a config signal with the input gain, frequency and pulse width vales.
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="frequency"></param>
        /// <param name="pulseWidth"></param>
        /// <returns></returns>
        public sbyte[] GenerateConfigSignal(int gain, byte frequency, byte pulseWidth)
        {
            Debug.Log("[Haptic Manager] Generating Config Signal");
            try
            {
                Debug.Log($"[Haptic Manager] Config Signal generated with gain: {gain} frequency: {frequency} pulse width: {pulseWidth}");
                HapticSignal h = CreateConfigSignal(gain, frequency, pulseWidth);
                configured = true;
                return getBytes(h);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Haptic Manager] Config Signal Exception {e}");
                configured = false;
                throw e;
            }
        }

        /// <summary>
        /// Generates a config signal with the current gain, frequency and pulse width values.
        /// </summary>
        /// <returns></returns>
        public sbyte[] GenerateConfigSignal()
        {
            Debug.Log("[Haptic Manager] Generating Config Signal");
            try
            {

                ConfigSignalClass c = ConfigSignal;
                Debug.Log($"[Haptic Manager] Config Signal generated with gain: {c.Gain} frequency: {c.Frequency} pulse width: {c.PulseWidth}");
                HapticSignal h = CreateConfigSignal(c.Gain, c.Frequency, c.PulseWidth);
                configured = true;
                return getBytes(h);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Haptic Manager] Config Signal Exception {e}");
                configured = false;
                throw e;
            }
        }

        #endregion 

        #region Calibration

        /// <summary>
        /// Sets the current calibration to the input values.
        /// </summary>
        /// <param name="lLimits"></param>
        /// <param name="uLimits"></param>
        /// <param name="cIndexes"></param>
        public void SetCalibration(int[] lLimits, int[] uLimits, int[] cIndexes)
        {
            Debug.Log("[Calibration] Setting calibration to " + uLimits[0] + ", & " + uLimits[1]);
            LowerLimits = lLimits;
            UpperLimits = uLimits;
            CalibrationIndex = cIndexes;
        }

        /// <summary>
        /// Alter the current calibration for the input device to the input value.
        /// </summary>
        /// <param name="isLeft"></param>
        /// <param name="index"></param>
        public void ModifyCalibrationByIndex(bool isLeft, int index)
        {
            float[] minMax = GetLowerUpperLimitForIndex(index);

            int deviceIndex = isLeft ? 1 : 0;

            LowerLimits[deviceIndex] = Mathf.RoundToInt(minMax[0]);
            UpperLimits[deviceIndex] = Mathf.RoundToInt(minMax[1]);

            CalibrationIndex[deviceIndex] = index;

            CalibrationUpdated?.Invoke(CalibrationIndex[1], CalibrationIndex[0]);
        }

        #endregion

        #region Create & Send EMS Message

        /// <summary>
        /// Adds haptic intensity to the input body part, adjusted for calibration. Bypass calibration will ignore the current calibration values, and should only be used during calibration processes.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="intensity"></param>
        /// <param name="bypassCalibration"></param>
        public void AddHapticIntensity(int bodyPart, float intensity, bool bypassCalibration = false)
        {

            if (EIRConfig.Instance.OutputHapticDebug) Debug.Log($"[Haptic Manager] Adding Haptic Intensity for BodyPart {bodyPart} with intensity {intensity}");

            if (bodyPart != 0 && bodyPart != 1)
            {
                Debug.LogError("[HapticManager] Cannot send intensity to parts other than hands");
                return;
            }

            intensities[bodyPart] += intensity;

            mappedIntensities[bodyPart] = bypassCalibration ? (int)intensities[bodyPart] : (int)ClampMappedForce(bodyPart, intensities[bodyPart]);
            IndicatorSignal[bodyPart] = bypassCalibration ? intensities[bodyPart] / 255.0f : intensities[bodyPart];
        }
        #endregion

        #region Presets

        /// <summary>
        /// Creates a new HapticPresetRunner instance that can be applied to a sigle body part and attaches it to the runnerObject.
        /// </summary>
        /// <param name="affectedBodyPart"></param>
        /// <param name="props"></param>
        /// <param name="intensityMultiplier"></param>
        /// <param name="beginActive"></param>
        /// <param name="keepAliveBetweenScenes"></param>
        /// <returns></returns>
        public HapticPresetRunner CreateHapticPresetRunner(BodyPart affectedBodyPart, HapticPreset props, float intensityMultiplier = 1, bool beginActive = true, bool keepAliveBetweenScenes = false)
        {
            HapticPresetRunner runner = runnerObject.AddComponent<HapticPresetRunner>();
            runners.Add(runner);


            List<BodyPart> affectedBodyParts = new List<BodyPart> {
                affectedBodyPart
            };

            runner.SetupRunner(affectedBodyParts, props, intensityMultiplier, beginActive, keepAliveBetweenScenes);

            return runner;
        }


        /// <summary>
        /// Creates a new HapticPresetRunner instance that can be applied to multiple body parts and attaches it to the runnerObject.
        /// </summary>
        /// <param name="affectedBodyParts"></param>
        /// <param name="props"></param>
        /// <param name="intensityMultiplier"></param>
        /// <param name="beginActive"></param>
        /// <param name="keepAliveBetweenScenes"></param>
        /// <returns></returns>
        public HapticPresetRunner CreateHapticPresetRunner(List<BodyPart> affectedBodyParts, HapticPreset props, float intensityMultiplier = 1, bool beginActive = true, bool keepAliveBetweenScenes = false)
        {
            HapticPresetRunner runner = runnerObject.AddComponent<HapticPresetRunner>();
            runners.Add(runner);
            runner.SetupRunner(affectedBodyParts, props, intensityMultiplier, beginActive, keepAliveBetweenScenes);

            return runner;
        }

        /// <summary>
        /// Returns all haptic preset runners that affect the input body part.
        /// </summary>
        /// <param name="affectedBodyPart"></param>
        /// <returns></returns>
        public List<HapticPresetRunner> GetHapticPresetRunnerByLimb(BodyPart affectedBodyPart)
        {

            List<HapticPresetRunner> correctRunners = new List<HapticPresetRunner>();

            for (int i = 0; i < runners.Count; i++)
            {
                if (runners[i].m_affectedBodyParts.Contains(affectedBodyPart) || (int)affectedBodyPart == -1)
                {
                    correctRunners.Add(runners[i]);
                }
            }

            return correctRunners;
        }

        /// <summary>
        /// Stops all active haptic preset runners, or if a body part is input, only those which affect that body part.
        /// </summary>
        /// <param name="affectedBodyPart"></param>
        public void StopHapticPresetRunner(BodyPart affectedBodyPart = (BodyPart)(-1))
        {

            for (int i = runners.Count - 1; i >= 0; i--)
            {
                if (runners[i].m_affectedBodyParts.Contains(affectedBodyPart) || (int)affectedBodyPart == -1)
                {
                    runners[i].Stop();
                    runners.Remove(runners[i]);
                }
            }
        }

        /// <summary>
        /// Stops all haptic preset runners affecting the input list of body parts.
        /// </summary>
        /// <param name="affectedBodyParts"></param>
        public void StopHapticPresetRunner(List<BodyPart> affectedBodyParts)
        {

            for (int i = runners.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < affectedBodyParts.Count; j++)
                {
                    if (runners[i].m_affectedBodyParts.Contains(affectedBodyParts[j]))
                    {
                        runners[i].Stop();
                        runners.Remove(runners[i]);
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnHapticPresetRequest(int bodyPart, float intensity)
        {
            AddHapticIntensity(bodyPart, intensity);
        }

        #endregion

        #region Support Functions

        /// <summary>
        /// Clamps a given fraction between the calibration limits for the input bodypart.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="forceFraction"></param>
        /// <returns></returns>
        private float ClampMappedForce(int part, float forceFraction)
        {
            // clamp it between 0.0 and 1.0;
            if (forceFraction <= 0)
                return 0;
            if (forceFraction > 1)
                return UpperLimits[part];
            // maps amplitude depending on calibration values

            float remap = Mathf.Lerp(LowerLimits[part], UpperLimits[part], forceFraction);

            // protect people from receiving minimum calibration value, when there is no force applied:
            if (remap == LowerLimits[part])
                remap = 0;
            return (int)remap;
        }

        /// <summary>
        /// Generates a haptic signal from the input mapped intensities.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private HapticSignal CreateHapticSignalFromIntensities(int[] value)
        {
            HapticSignal haptics = hapticSignal;
            haptics.config = 0;

            haptics.master_intensity_LSB = (byte)value[0];
            haptics.master_intensity_MSB = (byte)value[0]; // device: right

            haptics.slave_intensity_LSB = (byte)value[1];
            haptics.slave_intensity_MSB = (byte)value[1]; // device: left


            hapticSignal = haptics;
            return haptics;
        }

        /// <summary>
        /// Calculates a minimum and maximum value for calibration using the power of two for a given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="powerLaw"></param>
        /// <returns></returns>
        private float[] GetLowerUpperLimitForIndex(int index)
        {
            return new float[] { CalibrationMinOffset + CalibrationMinMultiplier * index * index, CalibrationMaxOffset + CalibrationMaxMultiplier * index * index };
        }

        /// <summary>
        /// Generates a HapticSignal for configuration purposes.
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="frequency"></param>
        /// <param name="pulse_width"></param>
        /// <returns></returns>
        private HapticSignal CreateConfigSignal(int gain, byte frequency, byte pulse_width)
        {
            HapticSignal _haptics = hapticSignal;
            _haptics.config = 1;

            switch (gain)
            {
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

        /// <summary>
        /// Generates an array of signed bytes from the input HapticSignal object.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private sbyte[] getBytes(HapticSignal str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            sbyte[] writeBytes = new sbyte[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                writeBytes[i] = (sbyte)arr[i];
            }
            return writeBytes;
        }
        #endregion

        #region constructors

        /// <summary>
        /// Generates a new Haptic Manager and sets the gameobject on which to run the Haptic Presets.
        /// </summary>
        /// <param name="runner"></param>
        public HapticManager(GameObject runner)
        {

            Debug.Log($"[Haptic Manager] Haptic Manager initialised from {runner.name}.");
            runnerObject = runner;
            UpperLimits = new int[2] { 0, 0, };
            LowerLimits = new int[2] { 0, 0, };
            CalibrationIndex = new int[2] { 0, 0, };

            runners = new List<HapticPresetRunner>();

            hapticSignal = new HapticSignal
            {
                enableEms = 1,
                config = 0
            };

            HapticPresetRunner.OnHapticPresetRequest += OnHapticPresetRequest;
        }

        #endregion

        #region Serializable Classes

        public class ConfigSignalClass
        {
            public int Gain;
            public byte Frequency;
            public byte PulseWidth;

            public ConfigSignalClass(int gain, byte frequency, byte pulseWidth)
            {
                Gain = gain;
                Frequency = frequency;
                PulseWidth = pulseWidth;
            }
        }

        #endregion
    }
}