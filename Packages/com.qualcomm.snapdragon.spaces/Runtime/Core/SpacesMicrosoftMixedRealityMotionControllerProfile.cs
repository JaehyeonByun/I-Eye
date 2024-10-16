/******************************************************************************
 * File: SpacesMicrosoftMixedRealityMotionControllerProfile.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine.XR.OpenXR.Features.Interactions;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
using UnityEditor;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Microsoft Mixed Reality Motion Controller Profile",
        BuildTargetGroups = new[]
        {
            BuildTargetGroup.Android
        },
        Company = "Qualcomm",
        Desc = "Allows for mapping input to the Microsoft Mixed Reality Motion Controller interaction profile.",
        OpenxrExtensionStrings = "",
        Version = "0.23.0",
        Category = FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class SpacesMicrosoftMixedRealityMotionControllerProfile : MicrosoftMotionControllerProfile
    {
    }
}
