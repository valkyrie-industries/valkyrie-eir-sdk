
namespace Valkyrie.EIR.Bluetooth {
    public interface IEirBluetooth {

        void OnBluetoothEnable();

        void OnWrite();

        void OnUpdateVitals(DeviceVitals vitals);

        void OnBluetoothDisable();
    }
}