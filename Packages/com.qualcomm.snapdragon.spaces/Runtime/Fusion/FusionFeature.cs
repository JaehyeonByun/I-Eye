/******************************************************************************
 * File: FusionFeature.cs
 * Copyright (c) 2021-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = FeatureName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Qualcomm",
        Desc = "Enables full simultaneous use of the mobile touchscreen and AR glasses on supported Snapdragon Spaces Development Kits",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = true,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    public partial class FusionFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Dual Render Fusion (Experimental)";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.fusion";
        public const string FeatureExtensions = "XR_UNITY_android_present";


        [Tooltip("If enabled, runs validation checks on the open Scene for required components to enable dual-rendering capabilities (recommended for setting up a Scene with dual render capabilities.)\n\nIf disabled, no validation checks will be run on the open Scene (recommended to prevent build errors if the open Scene does not need to be equipped with dual render capabilities.")]
        public bool ValidateOpenScene = true;

#if UNITY_EDITOR
        [Tooltip("If enabled, when entering Play Mode the editor will behave as if there are two valid screens: Display 1 will be the Dual Render Fusion host view (phone screen), and Display 2 will be the AR viewer (headset screens)."
            + "\n\nIf disabled, when entering Play Mode the editor will behave as if the connected device can only display XR content (as an MR/VR all-in-one device).")]
        public bool SimulateFusionDevice = true;
#endif

        protected override void OnEnable()
        {
            base.OnEnable();

#if UNITY_ANDROID && !UNITY_EDITOR
            FeatureUseCheckUtility.ImposeFeatureChecks_OpenXrNotRunning += FeatureUseCheckUtilityFusion.FusionChecksForOpenXRNotRunning;
#endif
        }
    }
}
