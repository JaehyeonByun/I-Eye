/******************************************************************************
 * File: RaycastFeature.FeatureValidation.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class HitTestingFeature
    {
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(new ValidationRule(this)
            {
                message = "The \"Ray Casting\" Feature currently requires the \"Plane Detection\" feature to be enabled. Please enable Plane Detection or disable both.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings)
                    {
                        return false;
                    }

                    var feature = settings.GetFeature<PlaneDetectionFeature>();
                    if (!feature)
                    {
                        return false;
                    }

                    return feature.enabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings)
                    {
                        return;
                    }

                    var feature = settings.GetFeature<PlaneDetectionFeature>();
                    if (!feature)
                    {
                        return;
                    }

                    feature.enabled = true;
                },
                error = true
            });
        }
    }
}
#endif
