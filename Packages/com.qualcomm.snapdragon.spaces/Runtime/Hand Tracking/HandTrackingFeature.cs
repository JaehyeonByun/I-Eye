/******************************************************************************
 * File: HandTrackingFeature.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html", false)]
#if UNITY_EDITOR
    [OpenXRFeature(UiName = FeatureName,
        BuildTargetGroups = new[]
        {
            BuildTargetGroup.Android
        },
        Company = "Qualcomm",
        Desc = "Enables Hand Tracking and gestures feature on Snapdragon Spaces enabled devices.",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class HandTrackingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Hand Tracking (Deprecated)";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.handtracking.deprecated";
        public const string FeatureExtensions = "XR_EXT_hand_tracking XR_QCOM_hand_tracking_gesture";
        private static readonly List<XRHandTrackingSubsystemDescriptor> _handTrackingSubsystemDescriptors = new List<XRHandTrackingSubsystemDescriptor>();
        private readonly XrHandGestureQCOM _leftGesture = new XrHandGestureQCOM(-1, 0, 0f);
        private readonly XrHandGestureQCOM _rightGesture = new XrHandGestureQCOM(-1, 0, 0f);
        private ulong _leftHandTrackerHandle;
        private ulong _rightHandTrackerHandle;
        private BaseRuntimeFeature _baseRuntimeFeature;
        internal ulong LeftHandTrackerHandle => _leftHandTrackerHandle;
        internal ulong RightHandTrackerHandle => _rightHandTrackerHandle;
        protected override bool IsRequiringBaseRuntimeFeature => true;

        public bool TryCreateHandTracking()
        {
            if (_xrCreateHandTrackerEXT == null)
            {
                Debug.LogError("xrCreateHandTrackerEXT method not found!");
                return false;
            }

            var leftCreateInfo = new XrHandTrackerCreateInfoEXT(XrHandEXT.XR_HAND_LEFT_EXT);
            var rightCreateInfo = new XrHandTrackerCreateInfoEXT(XrHandEXT.XR_HAND_RIGHT_EXT);
            var result = _xrCreateHandTrackerEXT(SessionHandle, ref leftCreateInfo, ref _leftHandTrackerHandle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to create left hand tracker: " + result);
            }

            result = _xrCreateHandTrackerEXT(SessionHandle, ref rightCreateInfo, ref _rightHandTrackerHandle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to create right hand tracker: " + result);
            }

            return result == XrResult.XR_SUCCESS;
        }

        public bool TryDestroyHandTracking()
        {
            if (_xrDestroyHandTrackerEXT == null)
            {
                Debug.LogError("xrDestroyHandTrackerEXT method not found!");
                return false;
            }

            if (_leftHandTrackerHandle != 0)
            {
                var result = _xrDestroyHandTrackerEXT(_leftHandTrackerHandle);
                if (result != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("Failed to destroy left hand tracker: " + result);
                    return false;
                }

                _leftHandTrackerHandle = 0;
            }

            if (_rightHandTrackerHandle != 0)
            {
                var result = _xrDestroyHandTrackerEXT(_rightHandTrackerHandle);
                if (result != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("Failed to destroy right hand tracker: " + result);
                    return false;
                }

                _rightHandTrackerHandle = 0;
            }

            return true;
        }

        public Tuple<List<Pose>, TrackingState> TryGetHandTrackingJointsAndTrackingState(bool forLeftHand)
        {
            var defaultReturn = new Tuple<List<Pose>, TrackingState>(new List<Pose>(), TrackingState.None);
            if (_xrLocateHandJointsEXT == null)
            {
                Debug.LogError("xrLocateHandJointsEXT method not found!");
                return defaultReturn;
            }

            var handTrackerHandle = forLeftHand ? _leftHandTrackerHandle : _rightHandTrackerHandle;
            if (handTrackerHandle == 0)
            {
                return defaultReturn;
            }

            var handJointsLocateInfoExt = new XrHandJointsLocateInfoEXT(_baseRuntimeFeature.SpaceHandle, _baseRuntimeFeature.PredictedDisplayTime);
            int size = Marshal.SizeOf(typeof(XrHandJointLocationEXT)) * (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT;
            IntPtr jointsPointer = Marshal.AllocHGlobal(size);
            var xrHandJointLocations = new XrHandJointLocationsEXT((int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT, jointsPointer);
            var result = _xrLocateHandJointsEXT(handTrackerHandle, ref handJointsLocateInfoExt, ref xrHandJointLocations);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Locate hand joints for " + (forLeftHand ? "left" : "right") + " hand failed: " + result);
                return defaultReturn;
            }

            var joints = new List<Pose>();
            if (xrHandJointLocations.IsActive)
            {
                if (xrHandJointLocations.JointCount != (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT)
                {
                    Debug.LogError("Located fewer joints than requested. Returning empty joint list.");
                    return defaultReturn;
                }

                for (int i = 0; i < xrHandJointLocations.JointCount; i++)
                {
                    var joint = Marshal.PtrToStructure<XrHandJointLocationEXT>(xrHandJointLocations.JointLocations + (Marshal.SizeOf(typeof(XrHandJointLocationEXT)) * i));
                    joints.Add(joint.pose.ToPose());
                }
            }

            Marshal.FreeHGlobal(jointsPointer);
            return new Tuple<List<Pose>, TrackingState>(joints, xrHandJointLocations.IsActive ? TrackingState.Tracking : TrackingState.None);
        }

        public Tuple<int, float, float> TryGetHandGestureData(bool forLeftHand)
        {
            var defaultReturn = new Tuple<int, float, float>(-1, 0f, 0f);
            if (_xrGetHandGestureQCOM == null)
            {
                Debug.LogError("xrGetHandGestureQCOM method not found!");
                return defaultReturn;
            }

            var handTracker = forLeftHand ? LeftHandTrackerHandle : RightHandTrackerHandle;
            var receivedGesture = forLeftHand ? _leftGesture : _rightGesture;
            var result = _xrGetHandGestureQCOM(handTracker, _baseRuntimeFeature.PredictedDisplayTime, ref receivedGesture);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Get hand gesture for " + (forLeftHand ? "left" : "right") + " hand failed: " + result);
                return defaultReturn;
            }

            return new Tuple<int, float, float>(receivedGesture.Gesture, receivedGesture.GestureRatio, receivedGesture.FlipRatio);
        }

        protected override string GetXrLayersToLoad()
        {
            return "XR_APILAYER_QCOM_handtracking";
        }

        protected override bool OnInstanceCreate(ulong instanceHandle)
        {
            base.OnInstanceCreate(instanceHandle);
            _baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any())
            {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var runtimeChecker = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.serviceshelper.RuntimeChecker");

            if ( !runtimeChecker.CallStatic<bool>("CheckCameraPermissions", new object[] { activity }) )
            {
                Debug.LogError("Snapdragon Spaces Services has no camera permissions! Hand Tracking feature disabled.");
                return false;
            }
#endif
            return true;
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem>(_handTrackingSubsystemDescriptors, HandTrackingSubsystem.ID);
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRHandTrackingSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRHandTrackingSubsystem>();
        }

        protected override void OnHookMethods()
        {
            HookMethod("xrCreateHandTrackerEXT", out _xrCreateHandTrackerEXT);
            HookMethod("xrDestroyHandTrackerEXT", out _xrDestroyHandTrackerEXT);
            HookMethod("xrLocateHandJointsEXT", out _xrLocateHandJointsEXT);
            HookMethod("xrGetHandGestureQCOM", out _xrGetHandGestureQCOM);
        }
    }
}
