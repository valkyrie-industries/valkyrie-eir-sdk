using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.EIR.Haptics
{
    /// <summary>
    /// A series of segments and segments which dictate what intensities a HapticPresetRunner should output while running
    /// </summary>
    public class HapticPreset
    {
        #region Enums
        public enum PresetType
        {
            minimum,
            maximum,
            sine,
            riseFall,
            fallRise,
            riseWaitFall,
            fallWaitRise,
            spike,
            value,
            slideUp,
            slideDown
        }

        public enum SegmentType
        {
            minimum,
            maximum,
            rise,
            fall,
            bridge,
            bridgeToZero,
            zeroToBridge,
            bridgeToOne,
            OneToBridge
        }

        public enum LoopType 
        { 
            None,
            Loop,
            LoopFinalIntensity
        }
        #endregion

        //Segments to play during this preset
        public HapticSegment[] m_segments;

        //If true, causes the HapticPresetRunner to loop
        public LoopType m_loopType;

        //How much time all of the segments in this preset will take to execute once
        public float totalSegmentTime
        {
            get
            {
                float totalTime = 0;
                for (int i = 0; i < m_segments.Length; i++)
                {
                    totalTime += m_segments[i].m_time;
                }
                return totalTime;
            }
            private set { }
        }

        public HapticPreset(HapticSegment[] segments, LoopType loop)
        {
            m_segments = segments;
            m_loopType = loop;
        }

        public HapticPreset() { }


        //Creates a preset of one of the default types
        public static HapticPreset CreateDefaultPreset(PresetType type, float length = 1, LoopType loopType = LoopType.None, float value = 0)
        {
            HapticPreset props = new HapticPreset();
            HapticSegment[] segments = new HapticSegment[0];


            switch (type)
            {
                //Create one segment that spans the whole length at 0
                case PresetType.minimum:
                    segments = new HapticSegment[1];
                    segments[0] = CreateDefaultSegment(SegmentType.minimum, length);
                    break;
                //Create one segment that spans the whole length at 1
                case PresetType.maximum:
                    segments = new HapticSegment[1];
                    segments[0] = CreateDefaultSegment(SegmentType.maximum, length);
                    break;
                //Create a sine wave
                case PresetType.sine:
                    segments = new HapticSegment[5];
                    segments[0] = CreateSegment(0, 0.2059f, length * 0.15f);
                    segments[1] = CreateSegment(0.2059f, 0.9378f, length * 0.27f);
                    segments[2] = CreateSegment(0.9378f, 0.9386f, length * 0.16f);
                    segments[3] = CreateSegment(0.9386f, 0.2072f, length * 0.27f);
                    segments[4] = CreateSegment(0.2072f, 0, length * 0.15f);
                    break;
                case PresetType.riseFall:
                    segments = new HapticSegment[2];
                    segments[0] = CreateDefaultSegment(SegmentType.rise, length / 2);
                    segments[1] = CreateDefaultSegment(SegmentType.fall, length / 2);
                    break;
                case PresetType.fallRise:
                    segments = new HapticSegment[2];
                    segments[0] = CreateDefaultSegment(SegmentType.fall, length / 2);
                    segments[1] = CreateDefaultSegment(SegmentType.rise, length / 2);
                    break;
                case PresetType.riseWaitFall:
                    segments = new HapticSegment[3];
                    segments[0] = CreateDefaultSegment(SegmentType.rise, length / 3);
                    segments[1] = CreateDefaultSegment(SegmentType.bridge, length / 3);
                    segments[2] = CreateDefaultSegment(SegmentType.fall, length / 3);
                    break;
                case PresetType.fallWaitRise:
                    segments = new HapticSegment[3];
                    segments[0] = CreateDefaultSegment(SegmentType.fall, length / 3);
                    segments[1] = CreateDefaultSegment(SegmentType.bridge, length / 3);
                    segments[2] = CreateDefaultSegment(SegmentType.rise, length / 3);
                    break;
                case PresetType.spike:
                    segments = new HapticSegment[2];
                    segments[0] = CreateDefaultSegment(SegmentType.maximum, 0.1f);
                    segments[1] = CreateDefaultSegment(SegmentType.minimum, length - 0.1f);
                    break;
                case PresetType.value:
                    segments = new HapticSegment[1];
                    segments[0] = CreateSegment(value, value, length);
                    break;
                case PresetType.slideUp:
                    segments = new HapticSegment[2];
                    segments[0] = CreateDefaultSegment(SegmentType.minimum, 0.1f);
                    segments[1] = CreateDefaultSegment(SegmentType.bridgeToOne, length - 0.1f);
                    break;
                case PresetType.slideDown:
                    segments = new HapticSegment[2];
                    segments[0] = CreateSegment(value, value, 0.1f);
                    segments[1] = CreateDefaultSegment(SegmentType.bridgeToZero, length - 0.1f);
                    break;
            }

            props.m_segments = segments;
            props.m_loopType = loopType;

            return props;
        }

        //Creates a segment of one of the default types
        public static HapticSegment CreateDefaultSegment(SegmentType type, float length)
        {
            HapticSegment segment = new HapticSegment();

            switch (type)
            {
                case SegmentType.minimum:
                    segment.m_point1 = 0;
                    segment.m_point2 = 0;
                    break;
                case SegmentType.maximum:
                    segment.m_point1 = 1;
                    segment.m_point2 = 1;
                    break;
                case SegmentType.rise:
                    segment.m_point1 = 0;
                    segment.m_point2 = 1;
                    break;
                case SegmentType.fall:
                    segment.m_point1 = 1;
                    segment.m_point2 = 0;
                    break;
                case SegmentType.bridge:
                    segment.usePrevAsPoint1 = true;
                    segment.useNextAsPoint2 = true;
                    break;
                case SegmentType.bridgeToZero:
                    segment.usePrevAsPoint1 = true;
                    segment.m_point2 = 0;
                    break;
                case SegmentType.zeroToBridge:
                    segment.m_point1 = 0;
                    segment.useNextAsPoint2 = true;
                    break;
                case SegmentType.bridgeToOne:
                    segment.usePrevAsPoint1 = true;
                    segment.m_point2 = 1;
                    break;
                case SegmentType.OneToBridge:
                    segment.m_point1 = 1;
                    segment.useNextAsPoint2 = true;
                    break;
            }

            segment.m_time = length;

            return segment;
        }

        public static HapticSegment CreateSegment(float point1, float point2, float length)
        {
            HapticSegment segment = new HapticSegment();
            segment.m_point1 = point1;
            segment.m_point2 = point2;
            segment.m_time = length;
            return segment;
        }


    }

}
