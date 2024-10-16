// /******************************************************************************
//  * File: HandTrackingFeature.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

#if XR_OPENXR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if SPACES
using Qualcomm.Snapdragon.Spaces;
#endif

namespace QCHT.Interactions.Core
{
#if UNITY_EDITOR
    [OpenXRFeature(
        FeatureId = FeatureId,
        UiName = FeatureName,
        Desc = FeatureDescription,
        Company = "Qualcomm",
        BuildTargetGroups = new[] {BuildTargetGroup.Android},
#if !SPACES
        CustomRuntimeLoaderBuildTargets = new[] {BuildTarget.Android},
#endif
        DocumentationLink = "",
        OpenxrExtensionStrings = "",
        Version = "0.16.0",
        Required = false,
        Category = FeatureCategory.Feature)]
#endif
#if SPACES
    public partial class HandTrackingFeature : SpacesOpenXRFeature
#else
    public partial class HandTrackingFeature : OpenXRFeature
#endif    
    {
#if SPACES
        public const string FeatureId = "com.qualcomm.snapdragon.spaces.handtracking";
        public const string FeatureName = "Hand Tracking";
        public const string FeatureDescription = "Enables Hand Tracking feature on Snapdragon Spaces enabled devices";
#else
        public const string FeatureId = "com.qualcomm.snapdragon.handtracking";
        public const string FeatureName = "Qualcomm Hand Tracking";
        public const string FeatureDescription =
            "Enables Hand Tracking and gestures feature on Snapdragon enabled devices.";
#endif

        [Tooltip("Should HaT subsystem automatically start after the OpenXR loader has started its subsystems?")]
        public bool AutoStart = true;

        private static readonly List<XRHandTrackingSubsystemDescriptor> s_handTrackingSubsystemDescriptors =
            new List<XRHandTrackingSubsystemDescriptor>();

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            return base.OnInstanceCreate(xrInstance);

#if UNITY_ANDROID && !UNITY_EDITOR
            var activity =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var runtimeChecker =
                new AndroidJavaClass("com.qualcomm.snapdragon.spaces.unityserviceshelper.RuntimeChecker");

            if (!runtimeChecker.CallStatic<bool>("CheckCameraPermissions", new object[] {activity}))
            {
                Debug.LogError("Snapdragon Spaces Services has no camera permissions! Hand Tracking feature disabled.");
                return false;
            }
#endif
        }

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
#if SPACES
            base.HookGetInstanceProcAddr(func);
#endif
            return QCHTOpenXRPlugin.GetInterceptedInstanceProcAddr(func);
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem>(
                s_handTrackingSubsystemDescriptors, QCHTHandTrackingProvider.ID);
        }

        protected override void OnSubsystemStart()
        {
            if (AutoStart)
            {
                StartSubsystem<XRHandTrackingSubsystem>();
            }
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRHandTrackingSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRHandTrackingSubsystem>();
        }

        protected override void OnAppSpaceChange(ulong xrSpace)
        {
            QCHTOpenXRPlugin.SetAppSpace(xrSpace);
        }

        public static string GetCurrentInteractionProfileForHand(XrHandedness handedness)
        {
            const string userPathHandLeft = "/user/hand/left";
            const string userPathHandRight = "/user/hand/right";
            var userPath = handedness == XrHandedness.XR_HAND_LEFT ? userPathHandLeft : userPathHandRight;
            return PathToString(GetCurrentInteractionProfile(userPath));
        }

        public static bool IsHandTrackingSupported()
        {
            return QCHTOpenXRPlugin.IsHandTrackingSupported();
        }
    }
}
#endif