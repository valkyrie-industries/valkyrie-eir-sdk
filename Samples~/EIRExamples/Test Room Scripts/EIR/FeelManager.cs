using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Haptics;

namespace Valkyrie.EIR.Examples
{

    /// <summary>
    /// Runs set feels from FeelLibrary as well as configuring EIR to whatever that feel requires.
    /// </summary>
    public class FeelManager : MonoBehaviour
    {
#if EIR_HAPTICS

        #region Static Accessors

        public static FeelManager Instance;

        #endregion

        #region Public Properties

        public bool IsPlayingFeeling { get; private set; }

        #endregion

        #region Events

        public delegate void FeelingStatusEvent(bool status);
        public FeelingStatusEvent OnFeelingStatusChange;

        #endregion

        #region Private Variables

        private HapticPresetRunner leftPreset;
        private HapticPresetRunner rightPreset;

        private ConfigureEIR configureEIR;

        private int originalGain;
        private int originalPulseWidth;
        private int originalFrequency;

        private bool usingCustomConf = false;

        #endregion

        #region Unity Methods

        private void Awake() {
            if (Instance == null) Instance = this;
            else {
                Destroy(this);
                return;
            }
        }

        private void Start() {
            configureEIR = FindObjectOfType<ConfigureEIR>();
            if (configureEIR == null) Debug.LogError("[FeelManager] FeelManager failed to initialise. Unable to find ConfigureEIR component.");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays the 'feeling' which corresponds to the input ID.
        /// </summary>
        /// <param name="feelID"></param>
        public void PlayFeeling(string feelID) {
            if (IsPlayingFeeling)
                return;

            FeelStruct feel = FeelLibrary.RequestFeel(feelID);

            if (string.IsNullOrEmpty(feel.name)) {
                Debug.LogError($"[FeelManager] Could not find feel with ID {feelID}.");
                return;
            }

            originalFrequency = configureEIR.frequency;
            originalGain = configureEIR.gain;
            originalPulseWidth = configureEIR.pulseWidth;

            if(feel.gain == 0 && feel.frequency == 0 && feel.pulseWidth == 0)
            {
                //Do nothing! This feel has no configuration attached
                usingCustomConf = false;
            }
            else
            {
                configureEIR.Configure(feel.gain, feel.frequency, feel.pulseWidth);
                usingCustomConf = true;
            }

            leftPreset = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.leftHand, feel.leftPreset);
            rightPreset = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.rightHand, feel.rightPreset);

            IsPlayingFeeling = true;
            OnFeelingStatusChange?.Invoke(true);

            StartCoroutine(WaitForPresetRunners());
        }

        /// <summary>
        /// Stops the active 'feeling'.
        /// </summary>
        public void StopPlayingFeeling() {
            IsPlayingFeeling = false;
            EIRManager.Instance.Haptics.StopHapticPresetRunner();
            if(usingCustomConf)
                configureEIR.Configure(originalGain, (byte)originalFrequency, (byte)originalPulseWidth);
            OnFeelingStatusChange?.Invoke(false);
        }

        #endregion

        #region Private Methods

        private IEnumerator WaitForPresetRunners() {
            while (IsPlayingFeeling && leftPreset != null && rightPreset != null) {
                yield return new WaitForEndOfFrame();
            }

            if(usingCustomConf)
                configureEIR.Configure(originalGain, (byte)originalFrequency, (byte)originalPulseWidth);

            OnFeelingStatusChange?.Invoke(false);
            IsPlayingFeeling = false;
        }

        #endregion
    }

    #region Data Classes

    public struct FeelStruct {
        public string name;
        public HapticPreset leftPreset;
        public HapticPreset rightPreset;
        public int gain;
        public byte frequency;
        public byte pulseWidth;
    }

    /// <summary>
    /// Retrieves feels from a list.
    /// </summary>
    public static class FeelLibrary {

        /// <summary>
        /// Retrieve the feel corresponding to the input id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static FeelStruct RequestFeel(string id) {
            return feelList.FirstOrDefault(s => s.name == id);
        }

        // Pre-set structs
        private static List<FeelStruct> feelList = new List<FeelStruct>
        {
            new FeelStruct
            {
                name = "Spikes",
                leftPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.1f),
                        HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.1f)),HapticPreset.LoopType.Loop),
                rightPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.1f),
                        HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.1f)),HapticPreset.LoopType.Loop),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Sine",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.Loop),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.Loop),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Maximum",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.None),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.None),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Bounce",
                leftPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.05f)),HapticPreset.LoopType.None),
               rightPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.05f)),HapticPreset.LoopType.None),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = 200
            },
            new FeelStruct
            {
                name = "Thunder",
                leftPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateSegment(0.1f,0.6f,1),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.OneToBridge,0.3f),
                                          HapticPreset.CreateSegment(0.1f,0.3f,1),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.3f)),HapticPreset.LoopType.Loop),
                rightPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateSegment(0.1f,0.6f,1.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.OneToBridge,0.3f),
                                          HapticPreset.CreateSegment(0.1f,0.3f,1),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.3f)),HapticPreset.LoopType.Loop),
                gain = 3,
                frequency = 255,
                pulseWidth = 200
            },
            new FeelStruct
            {
                name = "Fire",
                leftPreset = new HapticPreset(CreateArrayOfSegments(HapticPreset.CreateSegment(0.8f,0.8f,1.23f),HapticPreset.CreateSegment(0.8f,1f,0.5f)),HapticPreset.LoopType.Loop),
                rightPreset = new HapticPreset(CreateArrayOfSegments(HapticPreset.CreateSegment(0.8f,0.8f,0.9f),HapticPreset.CreateSegment(0.8f,1f,0.5f)),HapticPreset.LoopType.Loop),
                gain = 2,
                frequency = 200,
                pulseWidth = 90
            },
            new FeelStruct
            {
                name = "Earthquake",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.Loop),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.Loop),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = 25,
                pulseWidth = 180
            },
            new FeelStruct
            {
                name = "Shotgun",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,0.1f,HapticPreset.LoopType.None),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,0.1f,HapticPreset.LoopType.None),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = 200
            },
            new FeelStruct
            {
                name = "SineOnce",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.None),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.None),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "MaximumLoopLeft",
                leftPreset = new HapticPreset(new HapticSegment[]{HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.4f)},HapticPreset.LoopType.LoopFinalIntensity),
                rightPreset = new HapticPreset(new HapticSegment[]{HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.4f)},HapticPreset.LoopType.LoopFinalIntensity),
            },
            new FeelStruct
            {
                name = "MaximumLoopRight",
                leftPreset = new HapticPreset(new HapticSegment[]{HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.4f)},HapticPreset.LoopType.LoopFinalIntensity),
                rightPreset = new HapticPreset(new HapticSegment[]{HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.4f)},HapticPreset.LoopType.LoopFinalIntensity),
            },
            new FeelStruct
            {
                name = "MaximumLoopBoth",
                leftPreset = new HapticPreset(new HapticSegment[]{HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.4f)},HapticPreset.LoopType.LoopFinalIntensity),
                rightPreset = new HapticPreset(new HapticSegment[]{HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.4f)},HapticPreset.LoopType.LoopFinalIntensity),
            },

        };

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1) {
            return new HapticSegment[] { s1 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2) {
            return new HapticSegment[] { s1, s2 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3) {
            return new HapticSegment[] { s1, s2, s3 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4) {
            return new HapticSegment[] { s1, s2, s3, s4 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4, HapticSegment s5) {
            return new HapticSegment[] { s1, s2, s3, s4, s5 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4, HapticSegment s5, HapticSegment s6) {
            return new HapticSegment[] { s1, s2, s3, s4, s5, s6 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4, HapticSegment s5, HapticSegment s6, HapticSegment s7) {
            return new HapticSegment[] { s1, s2, s3, s4, s5, s6, s7 };
        }
        #endregion

#endif
    }

}

