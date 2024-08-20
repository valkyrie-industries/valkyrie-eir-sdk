using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class VLKBuild : IPreprocessBuildWithReport {
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        if (report.summary.platform == BuildTarget.Android) {
            MergeGradleFiles();
        }
    }

    private void MergeGradleFiles() {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string gradlePath = Path.Combine(projectPath, "Assets", "Plugins", "Android");

        // Define paths for the gradle templates
        string mainTemplatePath = Path.Combine(gradlePath, "mainTemplate.gradle");
        string customMainTemplatePath = Path.Combine(projectPath, "Packages", "com.valkyrieindustries.eirsdk", "Runtime", "Plugins", "Android", "mainTemplate.gradle");

        string propertiesTemplatePath = Path.Combine(gradlePath, "gradleTemplate.properties");
        string customPropertiesTemplatePath = Path.Combine(projectPath, "Packages", "com.valkyrieindustries.eirsdk", "Runtime", "Plugins", "Android", "gradleTemplate.properties");

        // Merge Gradle templates
        if (File.Exists(customMainTemplatePath)) {
            MergeGradleFile(customMainTemplatePath, mainTemplatePath);
        }

        // Merge Gradle properties
        if (File.Exists(customPropertiesTemplatePath)) {
            MergeGradleProperties(customPropertiesTemplatePath, propertiesTemplatePath);
        }
    }

    private void MergeGradleFile(string sourcePath, string targetPath) {
        // Read contents of both files
        string sourceContent = File.ReadAllText(sourcePath);
        string targetContent = File.ReadAllText(targetPath);

        // Merge specific blocks (dependencies, android, etc.)
        targetContent = MergeBlock(targetContent, sourceContent, "dependencies");
        targetContent = MergeBlock(targetContent, sourceContent, "android");
        // Add more blocks if needed

        // Save the merged content
        File.WriteAllText(targetPath, targetContent);
        Debug.Log($"[EIR Build] Merged {sourcePath} into {targetPath}");
    }

    private string MergeBlock(string targetContent, string sourceContent, string blockName) {
        string pattern = $@"(?s)(\b{blockName}\s*\{{)(.*?)(\}})";
        var targetMatch = Regex.Match(targetContent, pattern);
        var sourceMatch = Regex.Match(sourceContent, pattern);

        if (sourceMatch.Success) {
            string targetBlock = targetMatch.Success ? targetMatch.Groups[2].Value : "";
            string sourceBlock = sourceMatch.Groups[2].Value;

            var mergedBlock = MergeBlockContent(targetBlock, sourceBlock);

            if (targetMatch.Success) {
                // Replace the target block with the merged block
                targetContent = targetContent.Replace(targetMatch.Groups[2].Value, mergedBlock);
            }
            else {
                // Append the new block to the end if it does not exist in the target
                targetContent += $"\n{sourceMatch.Groups[1].Value}\n{mergedBlock}\n{sourceMatch.Groups[3].Value}\n";
            }
        }

        return targetContent;
    }

    private string MergeBlockContent(string targetBlock, string sourceBlock) {
        var targetLines = new HashSet<string>(NormalizeLines(targetBlock));
        var sourceLines = NormalizeLines(sourceBlock);

        foreach (var line in sourceLines) {
            if (!targetLines.Contains(line)) {
                targetBlock += "\n" + line;
                targetLines.Add(line);
            }
        }

        return targetBlock.Trim();
    }
    private IEnumerable<string> NormalizeLines(string blockContent) {
        var lines = blockContent.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++) {
            lines[i] = lines[i].Trim();
        }
        return lines;
    }

    private void MergeGradleProperties(string sourcePath, string targetPath) {
        var targetProperties = new HashSet<string>(File.ReadAllLines(targetPath));
        var sourceProperties = File.ReadAllLines(sourcePath);

        foreach (var property in sourceProperties) {
            if (!targetProperties.Contains(property.Trim())) {
                File.AppendAllText(targetPath, property.Trim() + "\n");
            }
        }

        Debug.Log($"[EIR Build] Merged {sourcePath} into {targetPath}");
    }
}
