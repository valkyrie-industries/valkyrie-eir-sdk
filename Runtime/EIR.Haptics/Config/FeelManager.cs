using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.EIR.Haptics {
    /// <summary>
    /// Runs set feels from FeelLibrary as well as configuring EIR to whatever that feel requires
    /// </summary>
    public class FeelManager : MonoBehaviour {
#if EIR_HAPTICS
        public static FeelManager instance;

        public static bool playingFeeling { get; private set; }

        private HapticPresetRunner presetRunnerB;
        private HapticPresetRunner presetRunnerA;

        private ConfigureEIR configureEIR;

        public delegate void FeelingStatusEvent(bool status);
        public FeelingStatusEvent onFeelingStatusChange;



        private void Awake() {
            if (instance == null)
                instance = this;
            else {
                Destroy(this);
                return;
            }
        }

        void Start() {
            configureEIR = FindObjectOfType<ConfigureEIR>();
            if (configureEIR == null)
                Debug.LogError("Can't find ConfigureEIR");
        }

        public void StopPlayingFeeling() {
            playingFeeling = false;
            EIRManager.Instance.Haptics.StopHapticPresetRunner();
            configureEIR.ConfigureToDefault();
            onFeelingStatusChange?.Invoke(false);
        }

        public void PlayFeeling(string feelID) {
            if (playingFeeling)
                return;

            FeelStruct feel = FeelLibrary.RequestFeel(feelID);

            if (string.IsNullOrEmpty(feel.name)) {
                Debug.LogError("[FeelButton] Could not find feel with ID " + feelID);
                return;
            }

            configureEIR.Configure(feel.gain, feel.frequency, feel.pulseWidth);

            presetRunnerA = EIRManager.Instance.Haptics.CreateHapticPresetRunner(DeviceRole.A, feel.presetA);
            presetRunnerB = EIRManager.Instance.Haptics.CreateHapticPresetRunner(DeviceRole.B, feel.presetB);


            playingFeeling = true;
            onFeelingStatusChange?.Invoke(true);

            StartCoroutine(WaitForPresetRunners());
        }


        private IEnumerator WaitForPresetRunners() {
            while (playingFeeling && presetRunnerB != null && presetRunnerA != null) {
                yield return new WaitForEndOfFrame();
            }

            configureEIR.ConfigureToDefault();

            onFeelingStatusChange?.Invoke(false);
            playingFeeling = false;
        }
    }

    public struct FeelStruct {
        public string name;
        public HapticPreset presetB;
        public HapticPreset presetA;
        public int gain;
        public byte frequency;
        public byte pulseWidth;
    }

    /// <summary>
    /// Retrieves feels from a list
    /// </summary>
    public static class FeelLibrary {
        public static FeelStruct RequestFeel(string id) {
            return feelList.FirstOrDefault(s => s.name == id);
        }

        // Pre-set structs
        private static List<FeelStruct> feelList = new List<FeelStruct>
        {
            new FeelStruct
            {
                name = "Spikes",
                presetB = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.1f),
                        HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.1f)),HapticPreset.LoopType.Loop),
                presetA = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.1f),
                        HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.1f)),HapticPreset.LoopType.Loop),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Sine",
                presetB = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.Loop),
                presetA = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.Loop),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Maximum",
                presetB = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.None),
                presetA = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.None),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Bounce",
                presetB = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.05f)),HapticPreset.LoopType.None),
               presetA = new HapticPreset(
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
                presetB = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateSegment(0.1f,0.6f,1),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.OneToBridge,0.3f),
                                          HapticPreset.CreateSegment(0.1f,0.3f,1),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.3f)),HapticPreset.LoopType.Loop),
                presetA = new HapticPreset(
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
                presetB = new HapticPreset(CreateArrayOfSegments(HapticPreset.CreateSegment(0.8f,0.8f,1.23f),HapticPreset.CreateSegment(0.8f,1f,0.5f)),HapticPreset.LoopType.Loop),
                presetA = new HapticPreset(CreateArrayOfSegments(HapticPreset.CreateSegment(0.8f,0.8f,0.9f),HapticPreset.CreateSegment(0.8f,1f,0.5f)),HapticPreset.LoopType.Loop),
                gain = 2,
                frequency = 200,
                pulseWidth = 90
            },
            new FeelStruct
            {
                name = "Earthquake",
                presetB = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.Loop),
                presetA = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,HapticPreset.LoopType.Loop),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = 25,
                pulseWidth = 180
            },
            new FeelStruct
            {
                name = "Shotgun",
                presetB = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,0.1f,HapticPreset.LoopType.None),
                presetA = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,0.1f,HapticPreset.LoopType.None),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = 200
            },
            new FeelStruct
            {
                name = "SineOnce",
                presetB = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.None),
                presetA = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,HapticPreset.LoopType.None),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
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

#endif
    }
}

