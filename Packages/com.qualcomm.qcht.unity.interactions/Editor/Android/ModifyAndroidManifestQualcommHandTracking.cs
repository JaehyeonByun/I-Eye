// /******************************************************************************
//  * File: ModifyAndroidManifestQualcommHandTracking.cs
//  * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using QCHT.Interactions.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace QCHT.Interactions.Android.Editor
{
    public class ModifyAndroidManifestQualcommHandTracking : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;

        public override Type featureType => typeof(HandTrackingFeature);

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
            // Ignored
        }
        
        protected override void OnPostprocessBuildExt(BuildReport report)
        {
            // Ignored
        }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            var manifestPath = $"{path}/src/main/AndroidManifest.xml";
            var androidManifest = new XmlAndroidManifest(manifestPath);

            try
            {
                androidManifest.Load();
            }
            catch (Exception)
            {
                Debug.LogError($"[ModifyAndroidManifestQualcommHandTracking:OnPostGenerateGradleAndroidProjectExt] " +
                               $"Failed to load Android manifest {manifestPath}");
                return;
            }

            // Gets Open XR profile string of every enabled OpenXRInteraction feature and checks if it contains "controller" in path. 
            // The profile constant seems to be named "profile" in every OpenXRInteraction features inherited classes.
            // It is not a OpenXRInteraction field though, so we try to get it by reflection.
            // It allows to check if a controller profile has been enabled in Open XR build settings
            // In case of a controller profile enabled, it will assume that the hand tracking is not required and can support both interactions (hand tracking + motion controllers)
            var required = true;
            var features = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            foreach (var interactionFeature in features.GetFeatures<OpenXRInteractionFeature>())
            {
                if (!interactionFeature.enabled)
                {
                    continue;
                }

                var type = interactionFeature.GetType();
                const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                var fieldInfo = type.GetField("profile", bindingFlags);
                if (fieldInfo == null)
                {
                    continue;
                }

                if (fieldInfo.GetValue(interactionFeature) is string profile && profile.Contains("controller"))
                {
                    required = false;
                }
            }

            androidManifest.SetNodeAndAttributes("/manifest", "uses-feature", "qualcomm.software.handtracking",
                new Dictionary<string, string>
                {
                    { "required", required ? "true" : "false" }
                });

            try
            {
                androidManifest.Save();
            }
            catch (Exception)
            {
                Debug.LogError($"[ModifyAndroidManifestQualcommHandTracking:OnPostGenerateGradleAndroidProjectExt] " +
                               $"Failed to save Android manifest {manifestPath}");
                return;
            }
        }
        
        private sealed class XmlAndroidManifest : XmlDocument
        {
            private const string kAndroidNamespaceUri = "http://schemas.android.com/apk/res/android";

            private readonly string _path;

            public XmlAndroidManifest(string path)
            {
                _path = path;
            }

            public void Load()
            {
                using (var textReader = new XmlTextReader(_path))
                {
                    textReader.Read();
                    Load(textReader);
                }
            }

            public void Save()
            {
                using (var textWriter = new XmlTextWriter(_path, new UTF8Encoding(false)))
                {
                    textWriter.Formatting = Formatting.Indented;
                    Save(textWriter);
                }
            }

            public void SetNodeAndAttributes(string parent, string tag, string name, Dictionary<string, string> attributes)
            {
                var parentNode = SelectSingleNode(parent) as XmlElement;
                if (parentNode == null)
                {
                    Debug.Log(
                        $"[ModifyAndroidManifestQualcommHandTracking:SetAttribute] Failed to get xml parent node = {parent}");
                    return;
                }

                var nodes = parentNode.SelectNodes(tag);
                XmlElement element = null;

                // Look existing element
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        var attr = (XmlAttribute)node.Attributes?.GetNamedItem("name", kAndroidNamespaceUri);
                        if (attr == null || !attr.Value.Equals(name))
                        {
                            continue;
                        }

                        element = (XmlElement)node;
                        break;
                    }
                }

                // Create one if does not exist
                if (element == null)
                {
                    element = CreateElement(tag);
                    element.SetAttribute("name", kAndroidNamespaceUri, name);
                    parentNode.AppendChild(element);
                }

                // Update attributes
                foreach (var attr in attributes)
                {
                    var xmlAttribute = (XmlAttribute)element.Attributes.GetNamedItem(attr.Key, kAndroidNamespaceUri);
                    if (xmlAttribute != null)
                    {
                        xmlAttribute.Value = attr.Value;
                    }
                    else
                    {
                        element.SetAttribute(attr.Key, kAndroidNamespaceUri, attr.Value);
                    }
                }
            }
        }
    }
}