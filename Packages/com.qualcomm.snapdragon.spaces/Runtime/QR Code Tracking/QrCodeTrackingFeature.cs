/******************************************************************************
 * File: QrCodeTrackingFeature.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
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

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = FeatureName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Qualcomm",
        Desc = "Enables QR Code Tracking feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class QrCodeTrackingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "QR Code Tracking (Experimental)";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.qrcodetracking";
        public const string FeatureExtensions = "XR_QCOMX_marker_tracking";
        private static readonly List<XRQrCodeTrackingSubsystemDescriptor> _qrCodeSubsystemDescriptors = new();
        private BaseRuntimeFeature _baseRuntimeFeature;
        protected override bool IsRequiringBaseRuntimeFeature => true;

        private XrMarkerTrackingModeQCOM[] _availableTrackingModes = {  };

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
            if (!_baseRuntimeFeature.CheckServicesCameraPermissions())
            {
                Debug.LogError("The QR Code Feature is missing the camera permissions and can't be created therefore!");
                return false;
            }
#endif

            return true;
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRQrCodeTrackingSubsystemDescriptor, XRQrCodeTrackingSubsystem>(_qrCodeSubsystemDescriptors, QrCodeTrackingSubsystem.ID);
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRQrCodeTrackingSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRQrCodeTrackingSubsystem>();
        }

        protected override void OnHookMethods()
        {
            HookMethod("xrEnumerateMarkerTypesQCOMX", out _xrEnumerateMarkerTypesQCOM);
            HookMethod("xrEnumerateMarkerTrackingModesQCOMX", out _xrEnumerateMarkerTrackingModesQCOM);
            HookMethod("xrCreateMarkerTrackerQCOMX", out _xrCreateMarkerTrackerQCOM);
            HookMethod("xrDestroyMarkerTrackerQCOMX", out _xrDestroyMarkerTrackerQCOM);
            HookMethod("xrStartMarkerDetectionQCOMX", out _xrStartMarkerDetectionQCOM);
            HookMethod("xrStopMarkerDetectionQCOMX", out _xrStopMarkerDetectionQCOM);
            HookMethod("xrGetMarkerSizeQCOMX", out _xrGetMarkerSizeQCOM);
            HookMethod("xrGetMarkerTypeQCOMX", out _xrGetMarkerTypeQCOM);
            HookMethod("xrGetQrCodeVersionQCOMX", out _xrGetQrCodeVersionQCOM);
            HookMethod("xrGetQrCodeStringDataQCOMX", out _xrGetQrCodeStringDataQCOM);
            HookMethod("xrGetQrCodeRawDataQCOMX", out _xrGetQrCodeRawDataQCOM);
            HookMethod("xrCreateMarkerSpaceQCOMX", out _xrCreateMarkerSpaceQCOM);
            HookMethod("xrPollMarkerUpdateQCOMX", out _xrPollMarkerUpdateQCOM);
            HookMethod("xrGetMarkerUpdateInfoQCOMX", out _xrGetMarkerUpdateInfoQCOM);
            HookMethod("xrReleaseMarkerUpdateQCOMX", out _xrReleaseMarkerUpdateQCOM);
            HookMethod("xrLocateSpace", out _xrLocateSpace);
            HookMethod("xrDestroySpace", out _xrDestroySpace);
        }

        public bool TryCreateMarkerTracker(out ulong tracker)
        {
            tracker = 0;
            XrMarkerTrackerCreateInfoQCOM createInfo = new XrMarkerTrackerCreateInfoQCOM();
            // Because the struct can't have inline declaration, we shall init values after creating a struct object.
            createInfo.InitStructureType();

            XrResult result = _xrCreateMarkerTrackerQCOM(SessionHandle, ref createInfo, ref tracker);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrCreateMarkerTrackerQCOM: " + result);
                return false;
            }

            return true;
        }

        public bool TryDestroyMarkerTracker(ulong tracker)
        {
            XrResult result = _xrDestroyMarkerTrackerQCOM(tracker);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrDestroyMarkerTrackerQCOM: " + result);
                return false;
            }

            return true;
        }

        public bool TryStartMarkerDetection(ulong tracker, Tuple<byte, byte> qrCodeVersionRange)
        {
            // TODO(TD): Add possibility to chose different XrMarkerTypes from manager
            XrMarkerTypeQCOM markerType = XrMarkerTypeQCOM.XR_MARKER_TYPE_QR_CODE_QCOMX;

            // NOTE(TD): Not using a ScopedArrayPtr for this, because an enum is a managed data type
            // and therefore cannot be marshaled.
            GCHandle handle = GCHandle.Alloc(markerType, GCHandleType.Pinned);
            IntPtr markerTypePtr = handle.AddrOfPinnedObject();

            using ScopePtr<XrQrCodeVersionRangeQCOM> qrCodeVersionRangesPtr = new(new XrQrCodeVersionRangeQCOM(
                XrQrCodeSymbolTypeQCOM.XR_QR_CODE_SYMBOL_TYPE_QR_CODE_QCOM,
                qrCodeVersionRange.Item1,
                qrCodeVersionRange.Item2));
            using ScopePtr<XrQrCodeVersionFilterQCOM> qrCodeVersionFilterPtr = new(new XrQrCodeVersionFilterQCOM(1, qrCodeVersionRangesPtr.Raw));

            XrMarkerDetectionStartInfoQCOM startInfo = new XrMarkerDetectionStartInfoQCOM(1, markerTypePtr, qrCodeVersionFilterPtr.Raw);

            XrResult result = _xrStartMarkerDetectionQCOM(tracker, ref startInfo);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrStartMarkerDetectionQCOM: " + result);
                return false;
            }

            return true;
        }

        public bool TryStopMarkerDetection(ulong tracker)
        {
            XrResult result = _xrStopMarkerDetectionQCOM(tracker);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrStopMarkerDetectionQCOM: " + result);
                return false;
            }

            return true;
        }

        public bool TryAcquireMarkerUpdate(ulong tracker, out ulong markerUpdate)
        {
            markerUpdate = 0;

            XrResult result = _xrPollMarkerUpdateQCOM(tracker, ref markerUpdate);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrPollMarkerUpdateQCOM: " + result);
                return false;
            }

            return true;
        }

        public bool TryGetMarkerUpdateInfo(ulong markerUpdate, out uint updateInfoCount, out IntPtr updateInfosPtr)
        {
            updateInfoCount = 0;
            // INFO: IntPtr updateInfosPtr is a runtime allocated array of XrMarkerUpdateInfoQCOM elements.
            updateInfosPtr = IntPtr.Zero;

            XrResult result = _xrGetMarkerUpdateInfoQCOM(markerUpdate, ref updateInfoCount, ref updateInfosPtr);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrGetMarkerUpdateInfoQCOM: " + result);
                return false;
            }

            return true;
        }

        public bool TryReleaseMarkerUpdate(ulong markerUpdate)
        {
            XrResult result = _xrReleaseMarkerUpdateQCOM(markerUpdate);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrReleaseMarkerUpdateQCOM: " + result);
                return false;
            }

            return true;
        }

        public bool TryCreateMarkerSpace(ulong marker, MarkerTrackingMode trackingMode, Vector2 markerSize, out ulong markerSpace)
        {
            markerSpace = 0;

            // Try to get tracking modes if there are none available
            if (_availableTrackingModes.Length == 0)
            {
                uint trackingModeCountOutput = 0;
                XrResult trackingModesResult = _xrEnumerateMarkerTrackingModesQCOM(SessionHandle, 0,
                    ref trackingModeCountOutput, IntPtr.Zero);
                if (trackingModesResult != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("Failed at call xrEnumerateMarkerTrackingModesQCOM 1: " + trackingModesResult);
                    return false;
                }

                // NOTE(TD): Do not use a ScopedArrayPtr for this, because an enum is a managed data type
                // and therefore cannot be marshaled.
                _availableTrackingModes = new XrMarkerTrackingModeQCOM[trackingModeCountOutput];
                GCHandle handle = GCHandle.Alloc(_availableTrackingModes, GCHandleType.Pinned);
                IntPtr trackingModesPtr = handle.AddrOfPinnedObject();

                trackingModesResult = _xrEnumerateMarkerTrackingModesQCOM(SessionHandle, trackingModeCountOutput,
                    ref trackingModeCountOutput, trackingModesPtr);
                if (trackingModesResult != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("Failed at call xrEnumerateMarkerTrackingModesQCOM 2: " + trackingModesResult);
                    return false;
                }
            }

            var markerTrackingMode = GetXrMarkerTrackingMode(trackingMode);
            if (!_availableTrackingModes.Contains(markerTrackingMode))
            {
                Debug.LogError($"Tracking mode {markerTrackingMode} not supported");
                return false;
            }

            using ScopePtr<XrMarkerTrackingModeInfoQCOM> trackingModePtr = new();
            trackingModePtr.Copy(new XrMarkerTrackingModeInfoQCOM(markerTrackingMode));

            using ScopePtr<XrUserDefinedMarkerSizeQCOM> markerSizePtr = new();
            markerSizePtr.Copy(new XrUserDefinedMarkerSizeQCOM(trackingModePtr.Raw, new XrExtent2Df(markerSize.x, markerSize.y)));

            XrPosef pose = new XrPosef(new XrQuaternionf(0, 0, 0, 1), new XrVector3f(0, 0, 0));
            XrMarkerSpaceCreateInfoQCOM markerSpaceCreateInfo = new(markerSizePtr.Raw, pose);

            XrResult result = _xrCreateMarkerSpaceQCOM(marker, ref markerSpaceCreateInfo, ref markerSpace);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrCreateMarkerSpaceQCOM: " + result);
                return false;
            }

            return true;
        }

        private XrMarkerTrackingModeQCOM GetXrMarkerTrackingMode(MarkerTrackingMode trackingMode)
        {
            switch (trackingMode)
            {
                case MarkerTrackingMode.Dynamic:
                    return XrMarkerTrackingModeQCOM.XR_MARKER_TRACKING_MODE_DYNAMIC_QCOM;
                case MarkerTrackingMode.Static:
                    return XrMarkerTrackingModeQCOM.XR_MARKER_TRACKING_MODE_STATIC_QCOM;
                case MarkerTrackingMode.Adaptive:
                    return XrMarkerTrackingModeQCOM.XR_MARKER_TRACKING_MODE_ADAPTIVE_QCOM;
                default:
                    Debug.LogError("Unknown MarkerTrackingMode! Defaulting to Dynamic");
                    return XrMarkerTrackingModeQCOM.XR_MARKER_TRACKING_MODE_DYNAMIC_QCOM;
            }
        }

        public bool TryDestroyMarkerSpace(ulong markerSpace)
        {
            XrResult result = _xrDestroySpace(markerSpace);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to destroy space (" + markerSpace + "): " + result);
                return false;
            }

            return true;
        }

        public bool TryGetQrCodeStringData(ulong marker, out string data)
        {
            data = string.Empty;

            uint markerDataSize = 0;
            XrResult result = _xrGetQrCodeStringDataQCOM(marker, 0, ref markerDataSize, IntPtr.Zero);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrGetQRCodeStringDataQCOM 1: " + result);
                return false;
            }

            if (markerDataSize == 0)
            {
                return false;
            }

            using ScopePtr<int> markerDataBuffer = new ScopePtr<int>((int)markerDataSize);
            result = _xrGetQrCodeStringDataQCOM(marker, markerDataSize, ref markerDataSize, markerDataBuffer.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrGetQRCodeStringDataQCOM 2: " + result);
                return false;
            }
            data = markerDataBuffer.AsString();

            return true;
        }

        public bool TryGetQrCodeSize(ulong marker, out Vector2 size)
        {
            using ScopePtr<XrExtent2Df> extentPtr = new(new XrExtent2Df());
            XrResult result = _xrGetMarkerSizeQCOM(marker, extentPtr.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed at call xrGetMarkerSizeQCOM 1: " + result);
                size = new Vector2();
                return false;
            }

            XrExtent2Df sizeExt = extentPtr.AsStruct();
            size.x = sizeExt.Width;
            size.y = sizeExt.Height;
            return true;
        }

        public bool TryGetMarkerPoseAndTrackingState(ulong markerSpace, out Tuple<Pose, TrackingState> poseAndState)
        {
            var spaceLocation = new XrSpaceLocation();
            // Because the struct can't have inline declaration, we shall init values after creating a struct object.
            spaceLocation.InitStructureType();

            XrResult result = _xrLocateSpace(markerSpace, SpaceHandle, _baseRuntimeFeature.PredictedDisplayTime, ref spaceLocation);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Locating Marker Space failed: " + result);
                poseAndState = new Tuple<Pose, TrackingState>(Pose.identity, TrackingState.None);
                return false;
            }

            var pose = spaceLocation.GetPose();
            var trackingState = spaceLocation.GetTrackingState();

            poseAndState = new Tuple<Pose, TrackingState>(pose, trackingState);
            return true;
        }
    }
}
