using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace Valkyrie.EIR.Bluetooth {

    public class BluetoothPermissions {

        public static System.Action<bool> OnPermissionsGranted;

        private const string DontAskKey = "PERMISSIONS_DENIED_DONTASK";

        public static void AskForPermissions() {
#if UNITY_ANDROID


            Debug.Log("[EIR Manager] Requesting Permissions...");
            List<string> permissions = new List<string>();

            if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")) {
                permissions.Add("android.permission.BLUETOOTH_SCAN");
                Debug.Log("[EIR Manager] Permission: BLUETOOTH_SCAN");
            } else {
                if (PlayerPrefs.HasKey(DontAskKey)) PlayerPrefs.DeleteKey(DontAskKey);
            }
            if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT")) {
                permissions.Add("android.permission.BLUETOOTH_CONNECT");
                Debug.Log("[EIR Manager] Permission: BLUETOOTH_CONNECT");
            } else {
                if (PlayerPrefs.HasKey(DontAskKey)) PlayerPrefs.DeleteKey(DontAskKey);
            }

            if (PlayerPrefs.HasKey(DontAskKey)) {
                Debug.Log("[EIR Manager] Permissions previously denied with don't ask, cannot proceed.");
                OnPermissionsGranted?.Invoke(false);
                return;
            }

            if (permissions.Count > 0) {
                Debug.Log($"[EIR Manager] Requesting {permissions.Count} permissions");
                PermissionCallbacks callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;

                Permission.RequestUserPermissions(permissions.ToArray(), callbacks);
            } else {
                Debug.Log("[EIR Manager] Permissions previously granted, proceeding.");
                OnPermissionsGranted?.Invoke(true);
            }
        }

        private static void PermissionCallbacks_PermissionDenied(string msg) {
            Debug.Log("[EIR Manager] Permissions denied.");
            OnPermissionsGranted?.Invoke(false);
        }

        private static void PermissionCallbacks_PermissionGranted(string msg) {
            Debug.Log("[EIR Manager] Permissions granted.");
            OnPermissionsGranted?.Invoke(true);
        }

        private static void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string msg) {
            Debug.Log("[EIR Manager] Permissions denied, fully.");
            PlayerPrefs.SetInt(DontAskKey, 1);
            OnPermissionsGranted?.Invoke(false);
#else
            Debug.Log("[EIR Manager] Erroneous call to BluetoothPermissions on invalid platform. Falling back to successful");
            OnPermissionsGranted?.Invoke(true);
#endif
        }
    }

}
