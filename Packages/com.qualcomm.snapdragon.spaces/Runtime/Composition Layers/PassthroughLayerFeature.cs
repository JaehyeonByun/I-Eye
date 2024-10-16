/******************************************************************************
 * File: PassthroughLayerFeature.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    #if UNITY_EDITOR
    [OpenXRFeature(UiName = FeatureName,
            BuildTargetGroups = new[]
            {
                BuildTargetGroup.Android
            },
            Company = "Qualcomm",
            Desc = "Enables Passthrough layer feature on Snapdragon Spaces enabled devices",
            DocumentationLink = "",
            OpenxrExtensionStrings = FeatureExtensions,
            Version = "0.23.0",
            Required = false,
            Category = FeatureCategory.Feature,
            FeatureId = FeatureID,
            Hidden = true /*TODO: (LE) unhide this when api is done*/)]
    #endif
    internal sealed class PassthroughLayerFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Passthrough Layer (Experimental)";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.passthroughlayer";
        public const string FeatureExtensions = "XR_FB_passthrough XR_FB_composition_layer_alpha_blend";

        protected override bool IsRequiringBaseRuntimeFeature => true;

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            base.OnInstanceCreate(xrInstance);
            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any())
            {
                Debug.Log(FeatureName + " is missing the following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }

            return true;
        }
    }
}
