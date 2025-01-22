
namespace Valkyrie.EIR.Bluetooth {
    public interface IEirBluetooth {

        /// <summary>
        /// Invoked when the bluetooth activity state is set to true, enabling characteristic write.
        /// </summary>
        void OnBluetoothEnable();

        /// <summary>
        /// Invoked on characteristic write.
        /// </summary>
        void OnWrite();

        /// <summary>
        /// Invoked on characteristic read when device vitals (battery, connection state) are returned from the connected EIR device.
        /// </summary>
        /// <param name="vitals"></param>
        void OnUpdateVitals(DeviceVitals vitals);

        /// <summary>
        /// Invoked when the bluetooth activity state is set to false, disabling characteristic write.
        /// </summary>
        void OnBluetoothDisable();

        /// <summary>
        /// Invoked when the output voltage value changes.
        /// </summary>
        void OnUpdateVoltages(double[] outputVoltages);

        /// <summary>
        /// Invoked when the eir bands are disconnected.
        /// </summary>
        void OnDisconnect();


        /// <summary>
        /// Invoked when the eir band battery levels drop below 20%.
        /// </summary>
        void OnLowBatteryDetected();
    }
}