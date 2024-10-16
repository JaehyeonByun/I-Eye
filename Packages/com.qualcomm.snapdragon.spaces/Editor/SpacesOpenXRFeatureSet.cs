/******************************************************************************
 * File: SpacesOpenXRFeatureSet.cs
 * Copyright (c) 2021-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;
using UnityEditor.XR.OpenXR.Features;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [OpenXRFeatureSet(FeatureIds = new[]
        {
            BaseRuntimeFeature.FeatureID,
            "com.qualcomm.snapdragon.spaces.spatialanchors",
            "com.qualcomm.snapdragon.spaces.planedetection",
            "com.qualcomm.snapdragon.spaces.imagetracking",
            "com.qualcomm.snapdragon.spaces.handtracking",
            "com.qualcomm.snapdragon.spaces.handtracking.deprecated",
            "com.qualcomm.snapdragon.spaces.raycasting",
            "com.qualcomm.snapdragon.spaces.foveatedrendering"
        },
        DefaultFeatureIds = new[]
        {
            BaseRuntimeFeature.FeatureID,
            "com.qualcomm.snapdragon.spaces.spatialanchors",
            "com.qualcomm.snapdragon.spaces.planedetection",
            "com.qualcomm.snapdragon.spaces.imagetracking",
            "com.qualcomm.snapdragon.spaces.handtracking",
            "com.qualcomm.snapdragon.spaces.handtracking.deprecated",
            "com.qualcomm.snapdragon.spaces.raycasting",
            "com.qualcomm.snapdragon.spaces.foveatedrendering"
        },
        UiName = "Snapdragon Spaces",
        Description = "Feature set with all of Snapdragon Spaces' glorious capabilities.",
        FeatureSetId = "com.qualcomm.snapdragon.spaces",
        SupportedBuildTargets = new[]
        {
            BuildTargetGroup.Android
        })]
    internal class SpacesOpenXRFeatureSet
    {
    }

    [OpenXRFeatureSet(FeatureIds = new[]
        {
            "com.qualcomm.snapdragon.spaces.cameraaccess",
            "com.qualcomm.snapdragon.spaces.sceneunderstanding",
            "com.qualcomm.snapdragon.spaces.qrcodetracking",
            "com.qualcomm.snapdragon.spaces.compositionlayers",
            "com.qualcomm.snapdragon.spaces.passthroughlayer",
            "com.qualcomm.snapdragon.spaces.fusion",
        },
        DefaultFeatureIds = new[]
        {
            "com.qualcomm.snapdragon.spaces.cameraaccess",
            "com.qualcomm.snapdragon.spaces.sceneunderstanding",
            "com.qualcomm.snapdragon.spaces.qrcodetracking",
            "com.qualcomm.snapdragon.spaces.compositionlayers",
            "com.qualcomm.snapdragon.spaces.passthroughlayer",
            "com.qualcomm.snapdragon.spaces.fusion",
        },
        UiName = "Snapdragon Spaces (Experimental)",
        Description = "Experimental features coming to Snapdragon Spaces.",
        FeatureSetId = "com.qualcomm.snapdragon.spaces.experimental",
        SupportedBuildTargets = new[]
        {
            BuildTargetGroup.Android
        })]
    internal class SpacesOpenXRExperimentalFeatureSet
    {
    }
}
