using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Valkyrie.EIR.Utilities {

    /// <summary>
    /// Post-Import process which executes upon successful import of the com.valkyrieindustries.eirsdk package.
    /// Generates and configures an EIR Config object.
    /// </summary>
    public class VLKPostImport {

        [InitializeOnLoadMethod]
        private static void SubscribeToEvent() {
            // this causes the method to be invoked after the Editor registers the new list of packages.
            Events.registeredPackages += OnRegisteredPackages;
        }

        private static void OnRegisteredPackages(PackageRegistrationEventArgs packageRegistrationEventArgs) {
            Debug.Log($"[EIR SDK Deployment] Executing post-package-import process...");
            foreach (var package in packageRegistrationEventArgs.added) {
                if (package.name == "com.valkyrieindustries.eirsdk") ExecutePostImportProcess();
            }
        }

        /// <summary>
        /// Upon package import, creates an EIRConfig asset (and any requisite directories) and sets it to the default configuration.
        /// </summary>
        private static void ExecutePostImportProcess() {

            string assetPath = "Assets/Resources/Valkyrie Config/EIRConfig.asset";

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Valkyrie Config")) {
                Debug.Log($"[EIR SDK Deployment] Creating directory...");
                if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "Valkyrie Config");

            }

            EIRConfig existingConfig = AssetDatabase.LoadAssetAtPath<EIRConfig>(assetPath);

            if (existingConfig != null) {
                EditorUtility.DisplayDialog("EIRConfig Already Exists", "EIRConfig asset already exists at:\n\n" + assetPath, "OK");
                Debug.Log($"[EIR SDK Deployment] EIRConfig asset already exists at {assetPath}");
                Selection.activeObject = existingConfig;
                return;
            }

            EIRConfig cfg = ScriptableObject.CreateInstance<EIRConfig>();

            AssetDatabase.CreateAsset(cfg, assetPath);

            cfg.BluetoothSendFrequency = BluetoothSendFrequency.EverySecondFrame;

            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Debug.Log("[EIR SDK Deployment] Setting Scripting Definitions..");

            // set both haptics and communication to on by default, interaction remains optional.
            SetScriptingDefineSymbol("EIR_HAPTICS", true);
            SetScriptingDefineSymbol("EIR_COMM", true);

            cfg.UsingHpt = true;
            cfg.UsingCom = true;

            Selection.activeObject = cfg;

        }

        /// <summary>
        /// Enables or disables a scripting define (EIR_COMM etc) dependent on which modules are required.
        /// </summary>
        /// <param name="define"></param>
        /// <param name="enable"></param>
        private static void SetScriptingDefineSymbol(string define, bool enable) {
            BuildTargetGroup bt = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(bt);

            // check if the define is already present
            bool defineExists = defines.Contains(define);

            // add or remove the define based on the enable flag
            if (enable && !defineExists) {
                // add the define only if it doesn't exist
                defines += (string.IsNullOrEmpty(defines) ? "" : ";") + define;
            }
            else if (!enable && defineExists) {
                // remove the define only if it exists
                defines = defines.Replace(define + ";", "").Replace(define, "");
            }

            // set the updated defines
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(bt, defines);
        }

    }
}