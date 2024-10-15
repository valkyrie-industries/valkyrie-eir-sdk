using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Xml;

public class VLKBuild : IPreprocessBuildWithReport {
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        if (report.summary.platform == BuildTarget.Android) {
            MergeGradleFiles();
            MergeAndroidManifest();
            RemoveDuplicateManifestEntries(); // removes duplicates after merging
        }
    }

    private void MergeGradleFiles() {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string gradlePath = Path.Combine(projectPath, "Assets", "Plugins", "Android");

        // define paths for the gradle templates
        string mainTemplatePath = Path.Combine(gradlePath, "mainTemplate.gradle");
        string customMainTemplatePath = Path.Combine(projectPath, "Packages", "com.valkyrieindustries.eirsdk", "Runtime", "Plugins", "Android", "mainTemplate.gradle");

        string propertiesTemplatePath = Path.Combine(gradlePath, "gradleTemplate.properties");
        string customPropertiesTemplatePath = Path.Combine(projectPath, "Packages", "com.valkyrieindustries.eirsdk", "Runtime", "Plugins", "Android", "gradleTemplate.properties");

        // merge Gradle templates
        if (File.Exists(customMainTemplatePath)) {
            MergeGradleFile(customMainTemplatePath, mainTemplatePath);
        }

        // merge Gradle properties
        if (File.Exists(customPropertiesTemplatePath)) {
            MergeGradleProperties(customPropertiesTemplatePath, propertiesTemplatePath);
        }
    }

    private void MergeGradleFile(string sourcePath, string targetPath) {

        // read contents of both files
        string sourceContent = File.ReadAllText(sourcePath);
        string targetContent = File.ReadAllText(targetPath);

        // merge specific blocks (dependencies, android, etc.)
        targetContent = MergeBlock(targetContent, sourceContent, "dependencies");
        targetContent = MergeBlock(targetContent, sourceContent, "android");
        // add more blocks if needed

        // save the merged content
        File.WriteAllText(targetPath, targetContent);
        Debug.Log($"[VLK Build] Merged {sourcePath} into {targetPath}");
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
                targetContent = Regex.Replace(targetContent, pattern, m => $"{m.Groups[1].Value}{mergedBlock}{m.Groups[3].Value}");
            }
            else {
                // Only append if the block is not already present
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

        Debug.Log($"[VLK Build] Merged {sourcePath} into {targetPath}");
    }

    private void MergeAndroidManifest() {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string manifestPath = Path.Combine(projectPath, "Assets", "Plugins", "Android", "AndroidManifest.xml");
        string customManifestPath = Path.Combine(projectPath, "Packages", "com.valkyrieindustries.eirsdk", "Runtime", "Plugins", "Android", "AndroidManifest.xml");

        if (File.Exists(customManifestPath)) {
            var targetDoc = new XmlDocument();
            targetDoc.Load(manifestPath);

            var sourceDoc = new XmlDocument();
            sourceDoc.Load(customManifestPath);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(targetDoc.NameTable);
            nsManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            XmlNode targetManifestNode = targetDoc.SelectSingleNode("/manifest", nsManager);
            XmlNode sourceManifestNode = sourceDoc.SelectSingleNode("/manifest", nsManager);

            MergeXmlNodes(targetDoc, targetManifestNode, sourceManifestNode, nsManager);

            targetDoc.Save(manifestPath);
            Debug.Log($"[VLK Build] Merged {customManifestPath} into {manifestPath}");
        }
    }

    private void MergeXmlNodes(XmlDocument targetDoc, XmlNode targetNode, XmlNode sourceNode, XmlNamespaceManager nsManager) {
        foreach (XmlNode sourceChildNode in sourceNode.ChildNodes) {
            if (sourceChildNode.NodeType == XmlNodeType.Element) {
                // create the XPath query for the child node considering namespaces
                string xpathQuery = $"{sourceChildNode.Name}[@android:name='{sourceChildNode.Attributes?["android:name"]?.Value}']";

                XmlNode targetChildNode = targetNode.SelectSingleNode(xpathQuery, nsManager);

                if (targetChildNode == null) {
                    // node doesn't exist in target, so import and add it
                    XmlNode newNode = targetDoc.ImportNode(sourceChildNode, true);
                    targetNode.AppendChild(newNode);
                }
                else {
                    // node exists in target, merge its children recursively
                    MergeXmlNodes(targetDoc, targetChildNode, sourceChildNode, nsManager);
                }
            }
        }
    }

    private void RemoveDuplicateManifestEntries() {
        string manifestPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), "Assets", "Plugins", "Android", "AndroidManifest.xml");

        if (!File.Exists(manifestPath)) {
            Debug.LogError("[VLK Build] AndroidManifest.xml not found at " + manifestPath);
            return;
        }

        XmlDocument manifestDoc = new XmlDocument();
        manifestDoc.Load(manifestPath);

        XmlNode manifestNode = manifestDoc.SelectSingleNode("/manifest");
        if (manifestNode == null) {
            Debug.LogError("[VLK Build] Invalid AndroidManifest.xml structure.");
            return;
        }

        // Remove duplicates from the <manifest> node
        RemoveDuplicateNodes(manifestNode, "application");
        RemoveDuplicateNodes(manifestNode, "uses-permission");
        RemoveDuplicateNodes(manifestNode, "uses-feature");

        // Remove duplicates within the <application> node
        XmlNode applicationNode = manifestNode.SelectSingleNode("application");
        if (applicationNode != null) {
            RemoveDuplicateNodes(applicationNode, "activity");
            RemoveDuplicateNodes(applicationNode, "service");
            RemoveDuplicateNodes(applicationNode, "meta-data");
        }

        // Save the modified XML
        manifestDoc.Save(manifestPath);
        Debug.Log("[VLK Build] Removed duplicate entries from AndroidManifest.xml.");
    }

    private void RemoveDuplicateNodes(XmlNode parentNode, string tagName) {
        XmlNodeList nodeList = parentNode.SelectNodes(tagName);
        if (nodeList.Count <= 1) return;

        HashSet<string> nodeSignatures = new HashSet<string>();

        for (int i = nodeList.Count - 1; i >= 0; i--) {
            XmlNode node = nodeList[i];
            string nodeSignature = GetNodeSignature(node);

            if (nodeSignatures.Contains(nodeSignature)) {
                parentNode.RemoveChild(node);
            }
            else {
                nodeSignatures.Add(nodeSignature);
            }
        }
    }

    private string GetNodeSignature(XmlNode node) {
        string signature = node.Name;

        if (node.Attributes != null) {
            foreach (XmlAttribute attr in node.Attributes) {
                signature += $"|{attr.Name}={attr.Value}";
            }
        }

        foreach (XmlNode child in node.ChildNodes) {
            signature += GetNodeSignature(child);
        }

        return signature;
    }
}
