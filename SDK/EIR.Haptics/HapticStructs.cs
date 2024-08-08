using System.Runtime.InteropServices;

namespace Valkyrie.EIR.Haptics
{
    public struct HapticSignal
    {
        public byte enableEms;
        public byte sleep_status;
        public byte config;

        public byte gain_0;
        public byte gain_1;
        public byte frequency;
        public byte pulse_width;
        public byte master_intensity_LSB;
        public byte master_intensity_MSB;
        public byte slave_intensity_LSB;
        public byte slave_intensity_MSB;

        public HapticSignal(byte _enableEms)
        {
            enableEms = _enableEms;
            sleep_status = 0;
            config = 1;
            gain_0 = 0;
            gain_1 = 0;
            frequency = 0;
            pulse_width = 0;
            master_intensity_LSB = 0;
            master_intensity_MSB = 0;
            slave_intensity_LSB = 0;
            slave_intensity_MSB = 0;
        }
    }

    public struct Muscle
    {
        public byte muscleNum;
        public byte intensity;
        public byte frequency;
        public byte pulseWidth;

        public Muscle(byte _muscleNum, byte _intensity, byte _frequency, byte _pulseWidth)
        {
            muscleNum = _muscleNum;
            intensity = _intensity;
            frequency = _frequency;
            pulseWidth = _pulseWidth;
        }

        public Muscle(Muscle _muscle)
        {
            muscleNum = _muscle.muscleNum;
            intensity = _muscle.intensity;
            frequency = _muscle.frequency;
            pulseWidth = _muscle.pulseWidth;
        }

        #region overriden functions
        public bool Equals(Muscle other)
        {
            return muscleNum == other.muscleNum && intensity == other.intensity &&
                   frequency == other.frequency && pulseWidth == other.pulseWidth;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Muscle))
            {
                return false;
            }
            Muscle otherMuscle = (Muscle)obj;
            return Equals(otherMuscle);
        }

        public static bool operator ==(Muscle one, Muscle two)
        {
            return one.Equals(two);
        }
        public static bool operator !=(Muscle one, Muscle two)
        {
            return !one.Equals(two);
        }

        public override int GetHashCode()
        {
            var calculation = muscleNum + 256 * intensity + 256 * 256 * frequency + 256 * 256 * 256 * pulseWidth;
            return calculation.GetHashCode();
        }
        #endregion
    }

    public struct HapticSegment
    {
        public float m_time;
        public float m_point1;
        public float m_point2;

        //If enabled, this segment will try to use the previous segment's end value for point1
        public bool usePrevAsPoint1;


        //If enabled, this segment will try to use the next segment's start value for point2
        public bool useNextAsPoint2;
    }
}