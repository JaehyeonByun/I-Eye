/******************************************************************************
 * File: AndroidGradleProjectPostProcessor.cs
 * Copyright (c) 2022-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    public class AndroidGradleProjectPostProcessor : IPostGenerateGradleAndroidProject
    {
        public class XmlAttributeContainer
        {
            public string Name;
            public string Value;
            public string Prefix;

            public XmlAttributeContainer(string name, string value)
            {
                Name = name;
                Value = value;
                Prefix = "android";
            }

            public XmlAttributeContainer(string name, string value, string prefix)
            {
                Name = name;
                Value = value;
                Prefix = prefix;
            }
        }

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (settings == null)
            {
                return;
            }

            var isOpenXRLoaderActive = settings.Manager.activeLoaders?.Any(loader => loader.GetType() == typeof(OpenXRLoader));
            var openXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var baseRuntimeFeature = openXRSettings?.GetFeature<BaseRuntimeFeature>();
            var fusionFeature = openXRSettings?.GetFeature<FusionFeature>();
            if (isOpenXRLoaderActive == true && baseRuntimeFeature != null && baseRuntimeFeature.enabled)
            {
                ModifyArchiveInBuildGradle(true, path, "SpacesServicesHelper");
                ModifyArchiveInBuildGradle(true, path, "libopenxr_loader");
                if (fusionFeature != null && fusionFeature.enabled)
                {
                    ModifyArchiveInBuildGradle(true, path, "SpacesActivities");
                    ModifyGradleProjectForHostController(true, path);
                    ModifySpacesLauncherLaunchCategory(false, path, "com.qualcomm.snapdragon.spaces.splashscreen.SplashScreenActivity");
                    ModifyUnityPlayerActivityLaunchCategory(true, path, "com.unity3d.player.UnityPlayerActivity");
                    ModifyPlayerActivityWindowFocusChangeBehaviour(false, path);
                    SetMetaData(false, path, "SKIP_LAUNCH_ON_VIEWER");
                    SetMetaData(false, path, "SKIP_PERMISSION_CHECKS");
                    SetMetaData(false, path, "LAUNCH_CONTROLLER_ON_HOST");
                    SetMetaData(baseRuntimeFeature.ShowSplashScreenOnHost, path, "SHOW_SPLASH_SCREEN_ON_HOST", "true");
                }
                else if (!baseRuntimeFeature.LaunchAppOnViewer && !baseRuntimeFeature.LaunchControllerOnHost)
                {
                    ModifyArchiveInBuildGradle(false, path, "SpacesActivities");
                    ModifyGradleProjectForHostController(false, path);
                    ModifyUnityPlayerActivityLaunchCategory(true, path, "com.unity3d.player.UnityPlayerActivity");
                    ModifyPlayerActivityWindowFocusChangeBehaviour(false, path);
                    SetMetaData(false, path, "SKIP_LAUNCH_ON_VIEWER");
                    SetMetaData(false, path, "SKIP_PERMISSION_CHECKS");
                    SetMetaData(false, path, "LAUNCH_CONTROLLER_ON_HOST");
                    SetMetaData(baseRuntimeFeature.ShowSplashScreenOnHost, path, "SHOW_SPLASH_SCREEN_ON_HOST", "true");
                }
                else
                {
                    ModifyArchiveInBuildGradle(true, path, "SpacesActivities");
                    ModifyUnityPlayerActivityLaunchCategory(false, path, "com.unity3d.player.UnityPlayerActivity");
                    SetMetaData(baseRuntimeFeature.ShowLaunchMessageOnHost, path, "SHOW_LAUNCH_MESSAGE_ON_HOST", "true");
                    SetMetaData(baseRuntimeFeature.ShowSplashScreenOnHost, path, "SHOW_SPLASH_SCREEN_ON_HOST", "true");
                    SetMetaData(!baseRuntimeFeature.LaunchAppOnViewer, path, "SKIP_LAUNCH_ON_VIEWER", "true");
                    SetMetaData(baseRuntimeFeature.LaunchControllerOnHost, path, "LAUNCH_CONTROLLER_ON_HOST", "true");
                    ModifySpacesLauncherLaunchCategory(!baseRuntimeFeature.ExportHeadless, path);
                    /* Determine if the controller archive should be replaced or overwritten by a possible same-name archive from the Assets folder. */
                    var projectArchives = new DirectoryInfo(Application.dataPath).GetFiles("*.aar");
                    var addController = !baseRuntimeFeature.UseCustomController || (baseRuntimeFeature.UseCustomController && projectArchives.Any(fileInfo => fileInfo.Name == "SpacesActivities.aar"));
                    ModifyArchiveInBuildGradle(baseRuntimeFeature.LaunchControllerOnHost && addController, path, "SpacesActivities");
                    ModifyGradleProjectForHostController(baseRuntimeFeature.LaunchControllerOnHost, path);
                    ModifyPlayerActivityWindowFocusChangeBehaviour(baseRuntimeFeature.LaunchAppOnViewer, path);
                    SetMetaData(baseRuntimeFeature.SkipPermissionChecks, path, "SKIP_PERMISSION_CHECKS", "true");
                }
            }
            else
            {
                /* Roll back every change that was made if the Base Runtime Feature is not enabled. */
                ModifyArchiveInBuildGradle(false, path, "SpacesServicesHelper");
                ModifyArchiveInBuildGradle(false, path, "SpacesActivities");
                ModifyArchiveInBuildGradle(false, path, "libopenxr_loader");
                ModifyGradleProjectForHostController(false, path);
                ModifyUnityPlayerActivityLaunchCategory(true, path, "com.unity3d.player.UnityPlayerActivity");
                ModifyPlayerActivityWindowFocusChangeBehaviour(false, path);
                SetMetaData(false, path, "SHOW_LAUNCH_MESSAGE_ON_HOST");
                SetMetaData(false, path, "SHOW_SPLASH_SCREEN_ON_HOST");
                SetMetaData(false, path, "SKIP_LAUNCH_ON_VIEWER");
                SetMetaData(false, path, "SKIP_PERMISSION_CHECKS");
				SetMetaData(false, path, "LAUNCH_CONTROLLER_ON_HOST");
            }
        }

        private void ModifyArchiveInBuildGradle(bool add, string path, string archiveName)
        {
            var gradlePropertiesPath = Path.Combine(path, "build.gradle");
            var lines = new List<string>(File.ReadAllLines(gradlePropertiesPath));
            var regex = "(?:[^A-Za-z])(" + archiveName + ")(?:[^A-Za-z],)";
            if (add)
            {
                if (!lines.Any(line => Regex.Match(line, regex).Success))
                {
                    int index = GetIndexForMatch(ref lines,
                        new[]
                        {
                            "dependencies",
                            "{"
                        });
                    ModifyBuildScript(true, "    implementation(name: '" + archiveName + "', ext:'aar')", ref lines, index + 1);
                }
            }
            else
            {
                lines.RemoveAll(line => Regex.Match(line, regex).Success);
            }

            File.WriteAllText(gradlePropertiesPath, String.Join(Environment.NewLine, lines));
        }

        private void ModifyUnityPlayerActivityLaunchCategory(bool add, string path, string activity)
        {
            path = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            var xmlDocument = ReadXmlDocument(path);
            var launcherSelector = "/manifest/application/activity[@android:name='" + activity + "']/";
            launcherSelector += "intent-filter/category[@android:name='android.intent.category.LAUNCHER']";
            var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            var launcherNode = xmlDocument.SelectSingleNode(launcherSelector, namespaceManager);
            bool overwriteFile = false;
            if (add && launcherNode == null)
            {
                launcherSelector = "/manifest/application/activity[@android:name='" + activity + "']/intent-filter";
                var intentNode = xmlDocument.SelectSingleNode(launcherSelector, namespaceManager);
                if (intentNode == null)
                {
                    return;
                }

                launcherNode = CreateXmlNode(xmlDocument, namespaceManager, "category", new XmlAttributeContainer("name", "android.intent.category.LAUNCHER"));
                intentNode.AppendChild(launcherNode);
                overwriteFile = true;
            }

            if (!add && launcherNode != null && launcherNode.ParentNode != null)
            {
                launcherNode.ParentNode.RemoveChild(launcherNode);
                overwriteFile = true;
            }

            if (overwriteFile)
            {
                WriteXmlDocument(path, xmlDocument);
            }
        }

        private void ModifySpacesLauncherLaunchCategory(bool add, string path, string activityNameFull = "com.qualcomm.snapdragon.spaces.customlauncher.SpacesLauncher")
        {
            path = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            var xmlDocument = ReadXmlDocument(path);
            var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            namespaceManager.AddNamespace("tools", "http://schemas.android.com/tools");
            bool overwriteFile = false;
            var activityNode = xmlDocument.SelectSingleNode("/manifest/application/activity[@android:name='"+activityNameFull+"']", namespaceManager);
            if (activityNode != null)
            {
                activityNode.ParentNode.RemoveChild(activityNode);
                overwriteFile = true;
            }

            if (!add)
            {
                var applicationNode = xmlDocument.SelectSingleNode("/manifest/application", namespaceManager);
                activityNode = CreateXmlNode(xmlDocument, namespaceManager, "activity", new XmlAttributeContainer("name", activityNameFull), new XmlAttributeContainer("exported", "true"));
                applicationNode.AppendChild(activityNode);
                var intentNodeForRemove = CreateXmlNode(xmlDocument, namespaceManager, "intent-filter", new XmlAttributeContainer("node", "removeAll", "tools"));
                activityNode.AppendChild(intentNodeForRemove);
                overwriteFile = true;
            }

            if (overwriteFile)
            {
                WriteXmlDocument(path, xmlDocument);
            }
        }

        private void ModifyGradleProjectForHostController(bool applyPatch, string path)
        {
            var gradlePropertiesPath = Path.Combine(path, "..", "gradle.properties");
            var lines = new List<string>(File.ReadAllLines(gradlePropertiesPath));
            ModifyBuildScript(true, "android.useAndroidX=true", ref lines);
            ModifyBuildScript(applyPatch, "android.enableJetifier=true", ref lines);
            File.WriteAllText(gradlePropertiesPath, String.Join(Environment.NewLine, lines));
            var launcherBuildGradlePath = Path.Combine(path, "..", "launcher", "build.gradle");
            lines = new List<string>(File.ReadAllLines(launcherBuildGradlePath));
            int index = GetIndexForMatch(ref lines,
                new[]
                {
                    "implementation",
                    "project",
                    "unityLibrary"
                });
            ModifyBuildScript(applyPatch, "    implementation 'com.android.support.constraint:constraint-layout:1.1.3'", ref lines, index + 1);
            ModifyBuildScript(applyPatch, "    implementation 'com.google.android.material:material:1.3.0'", ref lines, index + 1);
            ModifyBuildScript(applyPatch, "    implementation 'androidx.navigation:navigation-fragment-ktx:2.4.2'", ref lines, index + 1);
            ModifyBuildScript(applyPatch, "    implementation 'androidx.navigation:navigation-ui-ktx:2.4.2'", ref lines, index + 1);
            ModifyBuildScript(applyPatch, "    implementation 'androidx.lifecycle:lifecycle-livedata-ktx:2.4.1'", ref lines, index + 1);
            ModifyBuildScript(applyPatch, "    implementation 'androidx.lifecycle:lifecycle-viewmodel-ktx:2.4.1'", ref lines, index + 1);
            ModifyBuildScript(applyPatch, "    implementation 'androidx.databinding:viewbinding:7.2.1'", ref lines, index + 1);
            index = GetIndexForMatch(ref lines,
                new[]
                {
                    "apply",
                    "plugin",
                    "com.android.application"
                });
            ModifyBuildScript(applyPatch, "apply plugin: 'kotlin-android'", ref lines, index + 1);
            index = GetIndexForMatch(ref lines,
                new[]
                {
                    "android",
                    "{"
                });
            ModifyBuildScript(applyPatch, "    buildFeatures { viewBinding true }", ref lines, index + 1);
            File.WriteAllText(launcherBuildGradlePath, String.Join(Environment.NewLine, lines));
            var mainBuildGradlePath = Path.Combine(path, "..", "build.gradle");
            lines = new List<string>(File.ReadAllLines(mainBuildGradlePath));
#if UNITY_2022_2_OR_NEWER
            index = GetIndexForMatch(ref lines, new [] {"id", "android", "library", "version"});
            ModifyBuildScript(applyPatch,"    id 'org.jetbrains.kotlin.jvm' version '1.6.10' apply false", ref lines, index - 1);
#else
            index = GetIndexForMatch(ref lines,
                new[]
                {
                    "classpath",
                    "android",
                    "build",
                    "gradle"
                });
            ModifyBuildScript(applyPatch, "            classpath 'org.jetbrains.kotlin:kotlin-gradle-plugin:1.6.10'", ref lines, index + 1);
#endif
            File.WriteAllText(mainBuildGradlePath, String.Join(Environment.NewLine, lines));
        }

        private void ModifyPlayerActivityWindowFocusChangeBehaviour(bool add, string path)
        {
            /* Because Spaces applications launch on glasses (secondary display) we can ignore focus lost events which allow for
             * keyevents to still be processed by unity for remote OpenXR controllers to function. */
            var unityPlayerActivityPath = Path.Combine(path, "..", "unityLibrary", "src", "main", "java", "com", "unity3d", "player", "UnityPlayerActivity.java");
            var lines = new List<string>(File.ReadAllLines(unityPlayerActivityPath));
            int index = GetIndexForMatch(ref lines,
                new[]
                {
                    "super",
                    "onWindowFocusChanged",
                    "hasFocus"
                });
            ModifyBuildScript(add, "        if (!hasFocus) return;", ref lines, index);
            File.WriteAllText(unityPlayerActivityPath, String.Join(Environment.NewLine, lines));
        }

        private void SetMetaData(bool add, string path, string key, string data = "")
        {
            path = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            key = "com.qualcomm.snapdragon.spaces." + key;
            var xmlDocument = ReadXmlDocument(path);
            var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            namespaceManager.AddNamespace("tools", "http://schemas.android.com/tools");
            var metaDataNode = xmlDocument.SelectSingleNode("/manifest/application/meta-data[@android:name='" + key + "']", namespaceManager);
            if (metaDataNode != null)
            {
                metaDataNode.ParentNode.RemoveChild(metaDataNode);
            }

            if (add && data != "")
            {
                var applicationNode = xmlDocument.SelectSingleNode("/manifest/application", namespaceManager);
                metaDataNode = CreateXmlNode(xmlDocument, namespaceManager, "meta-data", new XmlAttributeContainer("name", key), new XmlAttributeContainer("value", data));
                applicationNode.AppendChild(metaDataNode);
            }

            WriteXmlDocument(path, xmlDocument);
        }

        private XmlDocument ReadXmlDocument(string path)
        {
            var xmlDocument = new XmlDocument();
            using (var reader = new XmlTextReader(path))
            {
                reader.Read();
                xmlDocument.Load(reader);
            }

            return xmlDocument;
        }

        private void WriteXmlDocument(string path, XmlDocument xmlDocument)
        {
            using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                xmlDocument.Save(writer);
            }
        }

        private void ModifyBuildScript(bool add, string newLine, ref List<string> lines, int index = -1)
        {
            if (add)
            {
                if (!lines.Any(line => line.Replace(" ", "").Contains(newLine.Replace(" ", ""))))
                {
                    lines.Insert(index == -1 ? lines.Count : index, newLine);
                }
            }
            else
            {
                lines.RemoveAll(line => line.Replace(" ", "").Contains(newLine.Replace(" ", "")));
            }
        }

        private int GetIndexForMatch(ref List<string> lines, string[] matchPhrases)
        {
            try
            {
                var index = lines.FindIndex(line => matchPhrases.All(line.Contains));
                return index;
            }
            catch
            {
                return -1;
            }
        }

        private XmlNode CreateXmlNode(XmlDocument xmlDocument, XmlNamespaceManager namespaceManager, string name, params XmlAttributeContainer[] attributes)
        {
            var node = xmlDocument.CreateNode(XmlNodeType.Element, name, namespaceManager.DefaultNamespace);
            foreach (var attribute in attributes)
            {
                var xmlAttribute = xmlDocument.CreateAttribute(attribute.Prefix, attribute.Name, namespaceManager.LookupNamespace(attribute.Prefix));
                xmlAttribute.Value = attribute.Value;
                node.Attributes.Append(xmlAttribute);
            }

            return node;
        }
    }
}
