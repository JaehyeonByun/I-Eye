// /******************************************************************************
//  * File: HandTrackingFeature.FeatureValidation.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

#if UNITY_EDITOR && XR_OPENXR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace QCHT.Interactions.Core
{
    public partial class HandTrackingFeature
    {
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(new ValidationRule(this)
            {
                message = "At least 4 bones skin weights is required for suitable hand mesh quality",
                checkPredicate = () =>
                {
                    var levels = QualitySettings.names;
                    var quality = QualitySettings.GetQualityLevel(); // save current quality settings
                    var isOk = true;
                    
                    for (var i = 0; i < levels.Length; i++)
                    {
                        QualitySettings.SetQualityLevel(i);
                        isOk &= QualitySettings.skinWeights >= SkinWeights.FourBones;
                    }

                    QualitySettings.SetQualityLevel(quality); // restore quality settings

                    return isOk;
                },
                fixIt = () =>
                {
                    var levels = QualitySettings.names;
                    var quality = QualitySettings.GetQualityLevel(); // save current quality settings
                    for (var i = 0; i < levels.Length; i++)
                    {
                        QualitySettings.SetQualityLevel(i);
                        if (QualitySettings.skinWeights < SkinWeights.FourBones)
                            QualitySettings.skinWeights = SkinWeights.FourBones;
                    }

                    QualitySettings.SetQualityLevel(quality); // restore quality settings
                }
            });
            // TODO: Enable this for QCHT >= 5.24 versions 
//             rules.Add(new ValidationRule(this)
//             {
// #if XR_OPENXR_1_8_0_OR_NEWER
//                 message = "Hand interaction profile or Microsoft Hand interaction profile should be enabled for full HaT support.",
// #else
//                 message = "Microsoft Hand interaction profile should be enabled for full HaT support.",
// #endif
//                 checkPredicate = () =>
//                 {
//                     var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
//                     if (!settings)
//                         return false;
//                     var msftHandProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature is MicrosoftHandInteraction);
//                     
// #if XR_OPENXR_1_8_0_OR_NEWER
//                     var handProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature is HandInteractionProfile);
//                     return handProfile && handProfile.enabled || msftHandProfile && msftHandProfile.enabled;
// #else
//                     return msftHandProfile && msftHandProfile.enabled;
// #endif
//                 },
//                 fixIt = () =>
//                 {
//                     var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
//                     if (!settings)
//                         return;
//                     
// #if XR_OPENXR_1_8_0_OR_NEWER
//                     var handProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature is HandInteractionProfile);
//                     if (handProfile)
//                     {
//                         handProfile.enabled = true;
//                         return;
//                     }
// #endif            
//                     var msftHandProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature is MicrosoftHandInteraction);
//                     if (msftHandProfile)
//                     {
//                         msftHandProfile.enabled = true;
//                     }
//                 }
//             });
        }
    }
}
#endif