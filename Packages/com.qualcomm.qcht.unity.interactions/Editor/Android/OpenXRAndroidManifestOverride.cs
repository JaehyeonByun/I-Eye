// /******************************************************************************
//  * File: OpenXRAndroidManifestOverride.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QCHT.Interactions.Android.Editor
{
    public static class OpenXRAndroidManifestOverride
    {
        [MenuItem("Qualcomm/Install Snapdragon OpenXR Manifest")]
        public static void OverrideAndroidManifest()
        {
            var appPluginDirectory = $"{Application.dataPath}/Plugins/Android/";
            var packageManifest = "Packages/com.qualcomm.qcht.unity.interactions/Editor/AndroidManifest/AndroidManifest.xml";
            packageManifest = Path.GetFullPath(packageManifest);

            try
            {
                if (!File.Exists(packageManifest))
                {
                    throw new FileNotFoundException($"File was not found : {packageManifest}");
                }

                if (!Directory.Exists(appPluginDirectory))
                {
                    Directory.CreateDirectory(appPluginDirectory);
                }

                File.Copy(packageManifest, $"{appPluginDirectory}AndroidManifest.xml", true);

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[QCHTPackageImport:OverrideAndroidManifest] {e.Message}");
            }
        }
    }
}