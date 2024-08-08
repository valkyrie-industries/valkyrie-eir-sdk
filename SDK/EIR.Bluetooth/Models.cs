using System;

namespace Valkyrie.EIR.Bluetooth {

    /// <summary>
    /// Defines a list of devices returned from the Bluetooth scan method.
    /// </summary>
    [Serializable]
    public class BluetoothDeviceList {
        public BluetoothDeviceInfo[] devices;
    }

    /// <summary>
    /// Defines a device returned from the Bluetooth scan method.
    /// </summary>
    [Serializable]
    public class BluetoothDeviceInfo {
        public string name;
        public string address;
        public int rssi;
    }

    /// <summary>
    /// Device connection state and battery status for both EIR bands.
    /// </summary>
    [Serializable]
    public class DeviceVitals {

        // reminder: R Master, L Slave.
        public bool LeftConnected;
        public bool RightConnected;
        public uint LeftBattery;
        public uint RightBattery;
        public uint PulseWidth;
        public uint PulseFrequency;

        public DeviceVitals(bool leftConnected, bool rightConnected, uint leftBattery, uint rightBattery, uint pulseWidth, uint pulseFrequency) {
            LeftConnected = leftConnected;
            RightConnected = rightConnected;
            LeftBattery = leftBattery;
            RightBattery = rightBattery;
            PulseWidth = pulseWidth;
            PulseFrequency = pulseFrequency;
        }

        public void Update(bool leftConnected, bool rightConnected, uint leftBattery, uint rightBattery, uint pulseWidth, uint pulseFrequency) {
            LeftConnected = leftConnected;
            RightConnected = rightConnected;
            LeftBattery = leftBattery;
            RightBattery = rightBattery;
            PulseWidth = pulseWidth;
            PulseFrequency = pulseFrequency;
        }
    }
}