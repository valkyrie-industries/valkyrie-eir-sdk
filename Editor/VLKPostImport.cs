using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Valkyrie.EIR.Utilities;

public class VLKPostImport {

    [InitializeOnLoadMethod]
    private static void SubscribeToEvent() {
        // This causes the method to be invoked after the Editor registers the new list of packages.
        Events.registeredPackages += OnRegisteredPackages;
    }

    private static void OnRegisteredPackages(PackageRegistrationEventArgs packageRegistrationEventArgs) {
        foreach (var package in packageRegistrationEventArgs.added) {
            if (package.name == "com.valkyrieindustries.eirsdk") ExecutePostImportProcess();
        }
    }

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

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Debug.Log("[EIR SDK Deployment] Setting Scripting Definitions..");

        SetScriptingDefineSymbol("EIR_HAPTICS", true);


        SetScriptingDefineSymbol("EIR_COMM", true);

        cfg.UsingHpt = true;
        cfg.UsingCom = true;

        Selection.activeObject = cfg;

    }
    private static void SetScriptingDefineSymbol(string define, bool enable) {
        BuildTargetGroup bt = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(bt);

        // Check if the define is already present
        bool defineExists = defines.Contains(define);

        // Add or remove the define based on the enable flag
        if (enable && !defineExists) {
            // Add the define only if it doesn't exist
            defines += (string.IsNullOrEmpty(defines) ? "" : ";") + define;
        } else if (!enable && defineExists) {
            // Remove the define only if it exists
            defines = defines.Replace(define + ";", "").Replace(define, "");
        }

        // Set the updated defines
        UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(bt, defines);
    }

}
