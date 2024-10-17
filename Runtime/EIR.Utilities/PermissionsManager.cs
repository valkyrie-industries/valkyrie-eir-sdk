using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;

namespace Valkyrie.EIR.Utilities {

    /// <summary>
    /// The permissions manager tracks the permission states of the three bluetooth permissions required, and requests permission if not given.
    /// </summary>
    public static class PermissionsManager {

        #region Public Properties

        /// <summary>
        /// Returns true if all three required permissions have been granted.
        /// </summary>
        public static bool PermissionsGranted {
            get {
#if UNITY_EDITOR
                return false;
#else
                return
               Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
               && Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT");
#endif
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Prints a debug log for the BLUETOOTH_SCAN and BLUETOOTH_CONNECT's permission status.
        /// </summary>
        public static void LogPermissions() {
            Debug.Log($"[Permissions] Active permissions: " +
                $"BLUETOOTH_SCAN granted {Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")} " +
                $"BLUETOOTH_CONNECT granted {Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT")}");
        }

        /// <summary>
        /// Checks all three required permissions and triggers a user prompt if permission is not already granted.
        /// </summary>
        public static void RequestPermissions() {

            List<string> permissions = new List<string>();

            if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")) {
                Debug.Log("[Permissions] Initialising request for BLUETOOTH_SCAN");
                permissions.Add("android.permission.BLUETOOTH_SCAN");
            }

            if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT")) {
                Debug.Log("[Permissions] Initialising request for BLUETOOTH_CONNECT");
                permissions.Add("android.permission.BLUETOOTH_CONNECT");
            }

            if (permissions.Count > 0) { Permission.RequestUserPermissions(permissions.ToArray()); }
        }

        #endregion
    }
}
#endif