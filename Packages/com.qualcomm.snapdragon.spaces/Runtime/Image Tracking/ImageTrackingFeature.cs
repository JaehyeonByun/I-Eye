/******************************************************************************
 * File: ImageTrackingFeature.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
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
#if UNITY_EDITOR
    [OpenXRFeature(UiName = FeatureName,
        BuildTargetGroups = new[]
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.Standalone
        },
        Company = "Qualcomm",
        Desc = "Enables Image Tracking feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class ImageTrackingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Image Tracking";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.imagetracking";
        public const string FeatureExtensions = "XR_QCOM_image_tracking";
        public bool ExtendedRangeMode;
        public bool LowPowerMode;
        private static readonly List<XRImageTrackingSubsystemDescriptor> _imageTrackingSubsystemDescriptors = new List<XRImageTrackingSubsystemDescriptor>();
        private readonly Dictionary<string, XRReferenceImage> _trackablesNameToSourceXRReferenceImages = new Dictionary<string, XRReferenceImage>();
        private readonly Dictionary<string, Dictionary<uint, ulong>> _trackablesNameAndIdToHandle = new Dictionary<string, Dictionary<uint, ulong>>();
        private BaseRuntimeFeature _baseRuntimeFeature;
        protected override bool IsRequiringBaseRuntimeFeature => true;

        public bool TryCreateImageTracker(out ulong imageTrackerHandle, RuntimeReferenceImageLibrary imageLibrary, int maxNumberOfMovingImages)
        {
            imageTrackerHandle = 0;
            if (_xrCreateImageTrackerQCOM == null)
            {
                Debug.LogError("XrCreateImageTrackerQCOM method not found!");
                return false;
            }

            // Set optimization hints.
            if (_xrSetTrackingOptimizationSettingsHintQCOM != null)
            {
                if (LowPowerMode)
                {
                    _xrSetTrackingOptimizationSettingsHintQCOM(SessionHandle,
                        XrTrackingOptimizationSettingsDomainQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_DOMAIN_IMAGE_TRACKING_QCOM,
                        XrTrackingOptimizationSettingsHintQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_HINT_LOW_POWER_PRIORIZATION_QCOM);
                }

                if (ExtendedRangeMode)
                {
                    _xrSetTrackingOptimizationSettingsHintQCOM(SessionHandle,
                        XrTrackingOptimizationSettingsDomainQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_DOMAIN_IMAGE_TRACKING_QCOM,
                        XrTrackingOptimizationSettingsHintQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_HINT_LONG_RANGE_PRIORIZATION_QCOM);
                }
            }

            SpacesRuntimeReferenceImageLibrary spacesImageLibrary =
                (SpacesRuntimeReferenceImageLibrary)imageLibrary;

            // Create Array of XrImageTrackerDataSetImageQCOM.
            int targetCount = spacesImageLibrary.count;
            IntPtr[] dataSetArray = new IntPtr[targetCount];
            IntPtr[] tempTextureBufferPtrs = new IntPtr[targetCount];
            IntPtr[] targetNamesPtrs = new IntPtr[targetCount];
            int[] targetModes = new int[targetCount];
            var trackingModesLookup = spacesImageLibrary.TrackingModes;
            var targetModesLookup = trackingModesLookup.TrackingModes;
            for (int i = 0; i < targetCount; i++)
            {
                if (_trackablesNameToSourceXRReferenceImages.ContainsKey(spacesImageLibrary[i].name))
                {
                    Debug.LogWarning("Duplicate name in the XRReferenceImageLibrary. Skipping '" + spacesImageLibrary[i].name + "'.");
                    continue;
                }

                _trackablesNameToSourceXRReferenceImages.Add(spacesImageLibrary[i].name, spacesImageLibrary[i]);
                NativeArray<byte> nativeTextureBuffer = spacesImageLibrary[i].texture.GetRawTextureData<byte>();
                tempTextureBufferPtrs[i] = Marshal.AllocHGlobal(nativeTextureBuffer.Length);
                Marshal.Copy(nativeTextureBuffer.ToArray(), 0, tempTextureBufferPtrs[i], nativeTextureBuffer.Length);
                var dataSetImage = new XrImageTrackerDataSetImageQCOM(spacesImageLibrary[i].name,
                    spacesImageLibrary[i].height,
                    new XrVector2f(spacesImageLibrary[i].texture.width, spacesImageLibrary[i].texture.height),
                    tempTextureBufferPtrs[i],
                    (uint)nativeTextureBuffer.Length);
                dataSetArray[i] = Marshal.AllocHGlobal(Marshal.SizeOf<XrImageTrackerDataSetImageQCOM>());
                Marshal.StructureToPtr(dataSetImage, dataSetArray[i], false);
                if (i < trackingModesLookup.Count)
                {
                    targetNamesPtrs[i] = Marshal.StringToHGlobalAnsi(trackingModesLookup.ReferenceImageNames[i]);
                    targetModes[i] = (int)targetModesLookup[i];
                }
            }

            GCHandle pinnedDataSetArray = GCHandle.Alloc(dataSetArray, GCHandleType.Pinned);
            IntPtr dataSetPtr = pinnedDataSetArray.AddrOfPinnedObject();
            GCHandle pinnedTargetNamesArray = GCHandle.Alloc(targetNamesPtrs, GCHandleType.Pinned);
            IntPtr targetNamesArrayPtr = pinnedTargetNamesArray.AddrOfPinnedObject();

            using ScopeArrayPtr<int> targetModesPtr = new(targetCount);
            Marshal.Copy(targetModes, 0, targetModesPtr.Raw, targetCount);

            XrImageTargetsTrackingModeInfoQCOM modeInfo = new XrImageTargetsTrackingModeInfoQCOM(targetCount, targetNamesArrayPtr, targetModesPtr.Raw);
            using ScopePtr<XrImageTargetsTrackingModeInfoQCOM> modeInfoPtr = new();
            modeInfoPtr.Copy(modeInfo);

            // Create XrImageTrackerCreateInfoQCOM.
            XrImageTrackerCreateInfoQCOM createInfo;
            if (_xrSetImageTargetsTrackingModeQCOM == null)
            {
                createInfo = new XrImageTrackerCreateInfoQCOM(dataSetPtr, (uint)targetCount, (uint)maxNumberOfMovingImages, IntPtr.Zero);
            }
            else
            {
                createInfo = new XrImageTrackerCreateInfoQCOM(dataSetPtr, (uint)targetCount, (uint)maxNumberOfMovingImages, modeInfoPtr.Raw);
            }

            // Create Image Tracker.
            XrResult result = _xrCreateImageTrackerQCOM(SessionHandle, ref createInfo, ref imageTrackerHandle);
            pinnedTargetNamesArray.Free();
            for (int i = 0; i < targetCount; ++i)
            {
                Marshal.FreeHGlobal(targetNamesPtrs[i]);
            }

            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to create Image Tracker: " + result);
                pinnedDataSetArray.Free();
                for (int i = 0; i < targetCount; i++)
                {
                    Marshal.FreeHGlobal(tempTextureBufferPtrs[i]);
                    Marshal.FreeHGlobal(dataSetArray[i]);
                }

                return false;
            }

            pinnedDataSetArray.Free();
            for (int i = 0; i < spacesImageLibrary.count; i++)
            {
                Marshal.FreeHGlobal(tempTextureBufferPtrs[i]);
                Marshal.FreeHGlobal(dataSetArray[i]);
            }

            return true;
        }

        public bool TryDestroyImageTracker(ulong imageTrackerHandle)
        {
            if (_xrDestroyImageTrackerQCOM == null)
            {
                Debug.LogError("XrDestroyImageTrackerQCOM method not found!");
                return false;
            }

            XrResult result = _xrDestroyImageTrackerQCOM(imageTrackerHandle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to destroy ImageTracker");
                return false;
            }

            _trackablesNameToSourceXRReferenceImages.Clear();
            return true;
        }

        public bool TryLocateImageTargets(ulong imageTracker, int imageCount, out List<XRTrackedImage> updatedTrackedImages)
        {
            updatedTrackedImages = new List<XRTrackedImage>();
            _trackablesNameAndIdToHandle.Clear();
            if (_xrLocateImageTargetsQCOM == null)
            {
                Debug.LogError("XrLocateImageTargetsQCOM method not found!");
                return false;
            }

            using ScopeArrayPtr<XrImageTargetLocationQCOM> imageTargetLocationsPtr = new(imageCount);
            XrImageTargetLocationsQCOM locations = new XrImageTargetLocationsQCOM((uint)imageCount, imageTargetLocationsPtr.Raw);
            XrImageTargetsLocateInfoQCOM locateInfo = new XrImageTargetsLocateInfoQCOM(SpaceHandle, _baseRuntimeFeature.PredictedDisplayTime);
            XrResult result = _xrLocateImageTargetsQCOM(imageTracker, ref locateInfo, ref locations);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to locate image targets: " + result);
                return false;
            }

            for (int i = 0; i < locations.Count; i++)
            {
                var imageLocation = imageTargetLocationsPtr.AtIndex(i);
                if (TryGetImageTargetNameAndId(imageLocation.ImageTargetHandle, out string identifier, out uint id))
                {
                    if (_trackablesNameToSourceXRReferenceImages.TryGetValue(identifier, out var image))
                    {
                        if (!_trackablesNameAndIdToHandle.ContainsKey(identifier))
                        {
                            _trackablesNameAndIdToHandle.Add(identifier, new Dictionary<uint, ulong>());
                        }

                        _trackablesNameAndIdToHandle[identifier].Add(id, imageLocation.ImageTargetHandle);
                        updatedTrackedImages.Add(new XRTrackedImage(new TrackableId(imageTracker, id),
                            image.guid,
                            imageLocation.XrPose.ToPose(),
                            image.size,
                            TrackingState.Tracking,
                            IntPtr.Zero));
                    }
                    else
                    {
                        Debug.LogWarning("No image trackable with identifier '" + identifier + "' has been found!");
                    }
                }
            }

            return true;
        }

        public bool TrySetTrackingModes(ulong imageTrackerHandle, List<string> referenceImageNames, List<SpacesImageTrackingMode> trackingModes)
        {
            if (_xrSetImageTargetsTrackingModeQCOM == null)
            {
                Debug.LogError("XrSetImageTargetsTrackingModeQCOM method not found!");
                return false;
            }

            var targetCount = trackingModes.Count;
            IntPtr[] targetNamesPtrs = new IntPtr[targetCount];
            int[] targetModes = new int[targetCount];
            for (int i = 0; i < trackingModes.Count; i++)
            {
                targetNamesPtrs[i] = Marshal.StringToHGlobalAnsi(referenceImageNames[i]);
                targetModes[i] = (int)trackingModes[i];
            }

            GCHandle pinnedTargetNamesArray = GCHandle.Alloc(targetNamesPtrs, GCHandleType.Pinned);
            IntPtr targetNamesArrayPtr = pinnedTargetNamesArray.AddrOfPinnedObject();
            using ScopeArrayPtr<int> targetModesPtr = new(targetCount);
            Marshal.Copy(targetModes, 0, targetModesPtr.Raw, targetCount);
            var modeInfo = new XrImageTargetsTrackingModeInfoQCOM(targetCount, targetNamesArrayPtr, targetModesPtr.Raw);
            using ScopePtr<XrImageTargetsTrackingModeInfoQCOM> modeInfoPtr = new();
            modeInfoPtr.Copy(modeInfo);
            XrResult result = _xrSetImageTargetsTrackingModeQCOM(imageTrackerHandle, modeInfo);
            pinnedTargetNamesArray.Free();
            for (int i = 0; i < targetCount; ++i)
            {
                Marshal.FreeHGlobal(targetNamesPtrs[i]);
            }

            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError($"XrSetImageTargetsTrackingModeQCOM failed with result: {result}");
                return false;
            }

            return true;
        }

        public bool TryStopTrackingImageInstance(string referenceImageName, uint id)
        {
            if (_xrStopImageTargetTrackingQCOM == null)
            {
                Debug.LogError("XrStopImageTargetTrackingQCOM method not found!");
                return false;
            }

            if (!_trackablesNameAndIdToHandle.TryGetValue(referenceImageName, out var idToHandle))
            {
                Debug.LogError($"TryStopTrackingImageInstance called with unknown referenceImageName: {referenceImageName}");
                return false;
            }

            if (!idToHandle.TryGetValue(id, out var imageTargetHandle))
            {
                Debug.LogError($"TryStopTrackingImageInstance called with unknown instance id: {id} for referenceImageName: {referenceImageName}");
                return false;
            }

            XrResult result = _xrStopImageTargetTrackingQCOM(imageTargetHandle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError($"XrStopImageTargetTrackingQCOM failed with result: {result}");
                return false;
            }

            return true;
        }

        protected override string GetXrLayersToLoad()
        {
            return "XR_APILAYER_QCOM_retina_tracking";
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
            if (!_baseRuntimeFeature.CheckServicesCameraPermissions())
            {
                Debug.LogError("The Image Tracking Feature is missing the camera permissions and can't be created therefore!");
                return false;
            }
#endif
            return true;
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(_imageTrackingSubsystemDescriptors, ImageTrackingSubsystem.ID);
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRImageTrackingSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRImageTrackingSubsystem>();
        }

        protected override void OnHookMethods()
        {
            HookMethod("xrCreateImageTrackerQCOM", out _xrCreateImageTrackerQCOM);
            HookMethod("xrDestroyImageTrackerQCOM", out _xrDestroyImageTrackerQCOM);
            HookMethod("xrLocateImageTargetsQCOM", out _xrLocateImageTargetsQCOM);
            HookMethod("xrImageTargetToNameAndIdQCOM", out _xrImageTargetToNameAndIdQCOM);
            HookMethod("xrSetImageTargetsTrackingModeQCOM", out _xrSetImageTargetsTrackingModeQCOM);
            HookMethod("xrStopImageTargetTrackingQCOM", out _xrStopImageTargetTrackingQCOM);
        }

        private bool TryGetImageTargetNameAndId(ulong imageTarget, out string identifier, out uint id)
        {
            identifier = null;
            id = 0;
            if (_xrImageTargetToNameAndIdQCOM == null)
            {
                Debug.LogError("XrImageTargetToNameAndIdQCOM method not found!");
                return false;
            }

            uint bufferCountOutput = 0;
            XrResult result = _xrImageTargetToNameAndIdQCOM(imageTarget,
                0,
                ref bufferCountOutput,
                IntPtr.Zero,
                ref id);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get name buffer size");
                return false;
            }

            using ScopePtr<int> bufferPtr = new((int)bufferCountOutput);
            result = _xrImageTargetToNameAndIdQCOM(imageTarget,
                bufferCountOutput,
                ref bufferCountOutput,
                bufferPtr.Raw,
                ref id);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get name buffer size");
                return false;
            }

            identifier = bufferPtr.AsString();
            return true;
        }
    }
}
