/******************************************************************************
 * File: BaseRuntimeFeature.FeatureValidations.cs
 * Copyright (c)2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(new ValidationRule(this)
            {
                message = "Only the \"Microsoft Mixed Reality Motion Controller Profile\" and \"Oculus Touch Controller Profile\" are fully supported by the Snapdragon Spaces samples.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings)
                    {
                        return false;
                    }

                    var motionControllerProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature.enabled && feature is SpacesMicrosoftMixedRealityMotionControllerProfile);
                    var oculusTouchControllerProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature.enabled && feature is OculusTouchControllerProfile);
                    return motionControllerProfile && oculusTouchControllerProfile;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings)
                    {
                        return;
                    }

                    var motionControllerProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature is SpacesMicrosoftMixedRealityMotionControllerProfile);
                    var oculusProfile = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(oclususFeature => oclususFeature is OculusTouchControllerProfile);
                    if (motionControllerProfile)
                    {
                        motionControllerProfile.enabled = true;
                    }

                    if (oculusProfile)
                    {
                        oculusProfile.enabled = true;
                    }
                },
                error = false
            });
            rules.Add(new ValidationRule(this)
            {
                message = "Only the OpenGLES3 graphics API is fully supported at the moment. Some provided samples might not work correctly with Vulkan.",
                checkPredicate = () =>
                {
                    return PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)
                        .SequenceEqual(new[]
                        {
                            GraphicsDeviceType.OpenGLES3
                        });
                },
                fixIt = () =>
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                        new[]
                        {
                            GraphicsDeviceType.OpenGLES3
                        });
                },
                error = false
            });
            rules.Add(new ValidationRule(this)
            {
                message = "Minimum Android SDK version has to be equal or greater than 29.",
                checkPredicate = () =>
                {
                    return PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel29;
                },
                fixIt = () =>
                {
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
                },
                error = true
            });
#if UNITY_2022_2_OR_NEWER
            rules.Add(new ValidationRule(this) {
                message = "Target Android SDK version has to be equal or greater than 31.",
                checkPredicate = () => PlayerSettings.Android.targetSdkVersion == 0 || PlayerSettings.Android.targetSdkVersion >= (AndroidSdkVersions) 31,
                fixIt = () => {
                    PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions) 31;
                },
                error = true,
            });
#endif
            rules.Add(new ValidationRule(this)
            {
                message = "The Scripting backend has to be set to IL2CPP for arm64.",
                checkPredicate = () =>
                {
                    var isUsingIIL2CPP = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
                    var isTargetingARM64 = PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64;
                    return isUsingIIL2CPP && isTargetingARM64;
                },
                fixIt = () =>
                {
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                },
                error = true
            });
            rules.Add(new ValidationRule(this)
            {
                message = "Only \"Landscape Left\" orientation is supported, when launching the application straight to the Viewer. Change the default orientation to \"Landscape Left\".",
                checkPredicate = () =>
                {
                    if (LaunchAppOnViewer)
                    {
                        var isUIOrientationLandscapeLeft = PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft;
                        return isUIOrientationLandscapeLeft;
                    }

                    return true;
                },
                fixIt = () =>
                {
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                },
                error = true
            });
            rules.Add(new ValidationRule(this)
            {
                message = "[Optional] Uncheck 'Force Remove Internet Permission' from the Meta Quest Feature settings in OpenXR.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
                    var questFeature = settings.GetFeature<MetaQuestFeature>();
                    if (!questFeature)
                    {
                        return false;
                    }
#if OPENXR_1_9_1_OR_NEWER
                    return !questFeature.ForceRemoveInternetPermission;
#else
                    var serializedFeature = new UnityEditor.SerializedObject(questFeature);
                    return !serializedFeature.FindProperty("forceRemoveInternetPermission").boolValue;
#endif
                },
                fixIt = () =>
                {
#if OPENXR_1_9_1_OR_NEWER
                    OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android).GetFeature<MetaQuestFeature>().ForceRemoveInternetPermission = false;
#else
                    var questFeature = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android).GetFeature<MetaQuestFeature>();
                    var serializedFeature = new UnityEditor.SerializedObject(questFeature);
                    serializedFeature.FindProperty("forceRemoveInternetPermission").boolValue = false;
                    serializedFeature.ApplyModifiedProperties();
#endif
                },
                error = false
            });
            rules.Add(new ValidationRule(this)
            {
                message = "[Optional] Add \"USING_SNAPDRAGON_SPACES_SDK\" to the scripting define symbols list if needed.",
                checkPredicate = () =>
                {
#if !USING_SNAPDRAGON_SPACES_SDK
                    return false;
#else
                    return true;
#endif
                },
                fixIt = () =>
                {
#if UNITY_2021_3_OR_NEWER
                    var target = NamedBuildTarget.Android;
                    var newDefines = PlayerSettings.GetScriptingDefineSymbols(target);
                    newDefines += ";USING_SNAPDRAGON_SPACES_SDK";
                    PlayerSettings.SetScriptingDefineSymbols(target, newDefines);

#else
                    var target = BuildTargetGroup.Android;
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                    defines += ";USING_SNAPDRAGON_SPACES_SDK";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
#endif
                },
                error = false,
                errorEnteringPlaymode = false,
            });
        }
    }
}
#endif
