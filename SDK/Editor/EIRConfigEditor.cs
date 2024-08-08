using UnityEditor;
using UnityEngine;

namespace Valkyrie.EIR.Utilities {

    [CustomEditor(typeof(EIRConfig))]
    public class EIRConfigEditor : Editor {

        [MenuItem("Valkyrie Tools/Generate EIR Config")]
        static void CreateConfig() {
            string assetPath = "Assets/Resources/Valkyrie Config/EIRConfig.asset";

            EIRConfig existingConfig = AssetDatabase.LoadAssetAtPath<EIRConfig>(assetPath);

            if (existingConfig != null) {
                EditorUtility.DisplayDialog("EIRConfig Already Exists", "EIRConfig asset already exists at:\n\n" + assetPath, "OK");
                Selection.activeObject = existingConfig;
                return;
            }

            EIRConfig cfg = ScriptableObject.CreateInstance<EIRConfig>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Valkyrie Config")) {
                AssetDatabase.CreateFolder("Assets/Resources", "Valkyrie Config");
            }

            AssetDatabase.CreateAsset(cfg, assetPath);

            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = cfg;
        }

        private SerializedProperty enableHapticsManager;
        private SerializedProperty enableBTEirBluetoothBridge;
        private SerializedProperty enableInteractionManager;
        private SerializedProperty bluetoothSendFrequency;
        private SerializedProperty outputHapticDebug;
        private SerializedProperty ignoreCachedDevice;
        private SerializedProperty vitalsReadFrequency;
        private SerializedProperty deviceFilter;
        private SerializedProperty autoInitialise;

        private void OnEnable() {
            enableHapticsManager = serializedObject.FindProperty("enableHapticsManager");
            enableBTEirBluetoothBridge = serializedObject.FindProperty("enableBTEirBluetoothBridge");
            enableInteractionManager = serializedObject.FindProperty("enableInteractionManager");
            bluetoothSendFrequency = serializedObject.FindProperty("bluetoothSendFrequency");
            outputHapticDebug = serializedObject.FindProperty("outputHapticDebug");
            ignoreCachedDevice = serializedObject.FindProperty("ignoreCachedDevice");
            vitalsReadFrequency = serializedObject.FindProperty("vitalsReadInterval");
            deviceFilter = serializedObject.FindProperty("deviceFilter");
            autoInitialise = serializedObject.FindProperty("autoInitialise");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(enableHapticsManager, new GUIContent("Enable Haptics Manager"));
            EditorGUILayout.PropertyField(enableBTEirBluetoothBridge, new GUIContent("Enable BT Communication Manager"));
            EditorGUILayout.PropertyField(enableInteractionManager, new GUIContent("Enable Interaction Manager"));
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(bluetoothSendFrequency, new GUIContent("Bluetooth Send Frequency"));
            EditorGUILayout.PropertyField(outputHapticDebug, new GUIContent("Output Haptic Debug"));
            EditorGUILayout.PropertyField(ignoreCachedDevice, new GUIContent("Ignore Cached Device"));
            EditorGUILayout.PropertyField(vitalsReadFrequency, new GUIContent("Vitals Read Interval (seconds)"));
            EditorGUILayout.PropertyField(deviceFilter, new GUIContent("Device Filter (returns devices with name containing this)"));
            EditorGUILayout.PropertyField(autoInitialise, new GUIContent("Auto Initialise (EIR Manager automatically initialises, otherwise call Initialise)"));

            serializedObject.ApplyModifiedProperties();

            UpdateScriptingDefines();
        }

        private void UpdateScriptingDefines() {

            BuildTargetGroup bt = EditorUserBuildSettings.selectedBuildTargetGroup;

            bool currentEnableHaptics = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(bt).Contains("EIR_HAPTICS");
            bool currentEnableComm = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(bt).Contains("EIR_COMM");
            bool currentEnableInteraction = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(bt).Contains("EIR_INTERACTION");

            if (enableHapticsManager.boolValue != currentEnableHaptics) {
                SetScriptingDefineSymbol("EIR_HAPTICS", enableHapticsManager.boolValue);
            }

            if (enableBTEirBluetoothBridge.boolValue != currentEnableComm) {
                SetScriptingDefineSymbol("EIR_COMM", enableBTEirBluetoothBridge.boolValue);
            }

            if (enableInteractionManager.boolValue != currentEnableInteraction) {
                SetScriptingDefineSymbol("EIR_INTERACTION", enableInteractionManager.boolValue);
            }

            // Force the Project window to repaint
            EditorApplication.RepaintProjectWindow();
        }


        private void SetScriptingDefineSymbol(string define, bool enable) {
            BuildTargetGroup bt = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(bt);

            // Check if the define is already present
            bool defineExists = defines.Contains(define);

            // Add or remove the define based on the enable flag
            if (enable && !defineExists) {
                // Add the define only if it doesn't exist
                defines += (string.IsNullOrEmpty(defines) ? "" : ";") + define;
            }
            else if (!enable && defineExists) {
                // Remove the define only if it exists
                defines = defines.Replace(define + ";", "").Replace(define, "");
            }

            // Set the updated defines
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(bt, defines);
        }


    }
}
