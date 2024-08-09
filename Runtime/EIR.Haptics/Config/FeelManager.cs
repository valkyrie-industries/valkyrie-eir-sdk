using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.EIR.Haptics{
    /// <summary>
    /// Runs set feels from FeelLibrary as well as configuring EIR to whatever that feel requires
    /// </summary>
    public class FeelManager : MonoBehaviour
    {
#if EIR_HAPTICS
        public static FeelManager instance;

        public static bool playingFeeling { get; private set; }

        private HapticPresetRunner leftPreset;
        private HapticPresetRunner rightPreset;

        private ConfigureEIR configureEIR;

        public delegate void FeelingStatusEvent(bool status);
        public FeelingStatusEvent onFeelingStatusChange;



        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Destroy(this);
                return;
            }
        }

        void Start()
        {
            configureEIR = FindObjectOfType<ConfigureEIR>();
            if (configureEIR == null)
                Debug.LogError("Can't find ConfigureEIR");
        }

        public void StopPlayingFeeling()
        {
            playingFeeling = false;
            EIRManager.Instance.Haptics.StopHapticPresetRunner();
            configureEIR.ConfigureToDefault();
            onFeelingStatusChange?.Invoke(false);
        }

        public void PlayFeeling(string feelID)
        {
            if (playingFeeling)
                return;

            FeelStruct feel = FeelLibrary.RequestFeel(feelID);

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[FeelButton] Could not find feel with ID " + feelID);
                return;
            }

            configureEIR.Configure(feel.gain, feel.frequency, feel.pulseWidth);

            leftPreset = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.leftHand, feel.leftPreset);
            rightPreset = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.rightHand, feel.rightPreset);

            playingFeeling = true;
            onFeelingStatusChange?.Invoke(true);

            StartCoroutine(WaitForPresetRunners());
        }


        private IEnumerator WaitForPresetRunners()
        {
            while (playingFeeling && leftPreset != null && rightPreset != null)
            {
                yield return new WaitForEndOfFrame();
            }

            configureEIR.ConfigureToDefault();

            onFeelingStatusChange?.Invoke(false);
            playingFeeling = false;
        }
    }

    public struct FeelStruct
    {
        public string name;
        public HapticPreset leftPreset;
        public HapticPreset rightPreset;
        public int gain;
        public byte frequency;
        public byte pulseWidth;
    }

    /// <summary>
    /// Retrieves feels from a list
    /// </summary>
    public static class FeelLibrary
    {
        public static FeelStruct RequestFeel(string id)
        {
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
                        HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.1f)),true),
                rightPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.1f),
                        HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.1f)),true),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Sine",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,true),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,true),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },
            new FeelStruct
            {
                name = "Maximum",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,false),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,false),
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
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.05f)),false),
               rightPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.1f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.05f)),false),
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
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.3f)),true),
                rightPreset = new HapticPreset(
                    CreateArrayOfSegments(HapticPreset.CreateSegment(0.1f,0.6f,1.3f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.OneToBridge,0.3f),
                                          HapticPreset.CreateSegment(0.1f,0.3f,1),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
                                          HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.fall,0.3f)),true),
                gain = 3,
                frequency = 255,
                pulseWidth = 200
            },
            new FeelStruct
            {
                name = "Fire",
                leftPreset = new HapticPreset(CreateArrayOfSegments(HapticPreset.CreateSegment(0.8f,0.8f,1.23f),HapticPreset.CreateSegment(0.8f,1f,0.5f)),true),
                rightPreset = new HapticPreset(CreateArrayOfSegments(HapticPreset.CreateSegment(0.8f,0.8f,0.9f),HapticPreset.CreateSegment(0.8f,1f,0.5f)),true),
                gain = 2,
                frequency = 200,
                pulseWidth = 90
            },
            new FeelStruct
            {
                name = "Earthquake",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,true),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,1,true),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = 25,
                pulseWidth = 180
            },
            new FeelStruct
            {
                name = "Shotgun",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,0.1f,false),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum,0.1f,false),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = 200
            },
            new FeelStruct
            {
                name = "SineOnce",
                leftPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,false),
                rightPreset = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine,3,false),
                gain = ConfigureEIR.MIN_GAIN,
                frequency = HapticManager.CONST_FREQUENCY,
                pulseWidth = HapticManager.CONST_PULSE_WIDTH
            },

        };

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1)
        {
            return new HapticSegment[] { s1 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2)
        {
            return new HapticSegment[] { s1, s2 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3)
        {
            return new HapticSegment[] { s1, s2, s3 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4)
        {
            return new HapticSegment[] { s1, s2, s3, s4 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4, HapticSegment s5)
        {
            return new HapticSegment[] { s1, s2, s3, s4, s5 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4, HapticSegment s5, HapticSegment s6)
        {
            return new HapticSegment[] { s1, s2, s3, s4, s5, s6 };
        }

        private static HapticSegment[] CreateArrayOfSegments(HapticSegment s1, HapticSegment s2, HapticSegment s3, HapticSegment s4, HapticSegment s5, HapticSegment s6, HapticSegment s7)
        {
            return new HapticSegment[] { s1, s2, s3, s4, s5, s6, s7 };
        }

#endif
    }
}

