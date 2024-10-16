/******************************************************************************
 * File: SpatialAnchorsFeature.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
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
    [OpenXRFeature(UiName = FeatureName,
        BuildTargetGroups = new[]
        {
            BuildTargetGroup.Android
        },
        Company = "Qualcomm",
        Desc = "Enables Spatial Anchors feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class SpatialAnchorsFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Spatial Anchors";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.spatialanchors";
        public const string FeatureExtensions = "XR_MSFT_spatial_anchor XR_MSFT_spatial_anchor_persistence XR_QCOM_spatial_anchor_persistence_extent";
        private static readonly List<XRAnchorSubsystemDescriptor> _spatialAnchorsSubsystemDescriptors = new List<XRAnchorSubsystemDescriptor>();
        private BaseRuntimeFeature _baseRuntimeFeature;
        protected override bool IsRequiringBaseRuntimeFeature => true;

        public ulong TryCreateSpatialAnchorHandle(Pose pose)
        {
            ulong anchorHandle = 0;
            if (_xrCreateSpatialAnchorMSFTPtr != null)
            {
                var spatialAnchorCreateInfo = new XrSpatialAnchorCreateInfoMSFT(pose, SpaceHandle, _baseRuntimeFeature.PredictedDisplayTime);
                var callResult = _xrCreateSpatialAnchorMSFTPtr(SessionHandle, spatialAnchorCreateInfo, ref anchorHandle);
                if (callResult != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("Creating Spatial Anchor failed with error: " + Enum.GetName(typeof(XrResult), callResult));
                }
            }

            return anchorHandle;
        }

        public ulong TryCreateSpatialAnchorSpaceHandle(ulong anchorHandle)
        {
            ulong anchorSpaceHandle = 0;
            if (_xrCreateSpatialAnchorSpaceMSFTPtr != null)
            {
                var spatialAnchorSpaceCreateInfo = new XrSpatialAnchorSpaceCreateInfoMSFT(anchorHandle);
                var callResult = _xrCreateSpatialAnchorSpaceMSFTPtr(SessionHandle, spatialAnchorSpaceCreateInfo, ref anchorSpaceHandle);
                if (callResult != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("Creating Spatial Anchor Space failed with error: " + Enum.GetName(typeof(XrResult), callResult));
                }
            }

            return anchorSpaceHandle;
        }

        public bool TryDestroySpatialAnchor(ulong anchorHandle)
        {
            if (_xrDestroySpatialAnchorMSFTPtr != null)
            {
                var callResult = _xrDestroySpatialAnchorMSFTPtr(anchorHandle);
                if (callResult == XrResult.XR_SUCCESS)
                {
                    return true;
                }

                Debug.LogError("Destroying Spatial Anchor failed with error: " + Enum.GetName(typeof(XrResult), callResult));
            }

            return false;
        }

        public Tuple<Pose, TrackingState> TryGetSpatialAnchorSpacePoseAndTrackingState(ulong anchorSpaceHandle)
        {
            var returnValue = new Tuple<Pose, TrackingState>(new Pose(), TrackingState.None);
            if (_xrLocateSpacePtr != null)
            {
                var spaceLocation = new XrSpaceLocation();
                // Because the struct can't have inline declaration, we shall init values after creating a struct object.
                spaceLocation.InitStructureType();
                var callResult = _xrLocateSpacePtr(anchorSpaceHandle, SpaceHandle, _baseRuntimeFeature.PredictedDisplayTime, ref spaceLocation);
                if (callResult == XrResult.XR_SUCCESS)
                {
                    var pose = spaceLocation.GetPose();
                    var trackingState = spaceLocation.GetTrackingState();
                    returnValue = new Tuple<Pose, TrackingState>(pose, trackingState);
                }
                else
                {
                    Debug.LogError("Locating Spatial Anchor Space failed with error: " + Enum.GetName(typeof(XrResult), callResult));
                }
            }

            return returnValue;
        }

        public bool TryCreateSpatialAnchorStoreConnection(out ulong spatialAnchorStore)
        {
            spatialAnchorStore = 0;
            if (_xrCreateSpatialAnchorStoreConnectionMSFT == null)
            {
                Debug.LogError("xrCreateSpatialAnchorStoreConnectionMSFT method not found!");
                return false;
            }

            XrResult result = _xrCreateSpatialAnchorStoreConnectionMSFT(SessionHandle, ref spatialAnchorStore);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to create spatial anchor store connection: " + result);
                return false;
            }

            return true;
        }

        public bool TryDestroySpatialAnchorStoreConnection(ulong spatialAnchorStore)
        {
            if (_xrDestroySpatialAnchorStoreConnectionMSFT == null)
            {
                Debug.LogError("xrDestroySpatialAnchorStoreConnectionMSFT method not found!");
                return false;
            }

            XrResult result = _xrDestroySpatialAnchorStoreConnectionMSFT(spatialAnchorStore);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to destroy spatial anchor store connection: " + result);
                return false;
            }

            return true;
        }

        public bool TryClearSpatialAnchorStoreMSFT(ulong spatialAnchorStore)
        {
            if (_xrClearSpatialAnchorStoreMSFT == null)
            {
                Debug.LogError("xrClearSpatialAnchorStoreMSFT method not found!");
                return false;
            }

            XrResult result = _xrClearSpatialAnchorStoreMSFT(spatialAnchorStore);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to clear spatial anchor store: " + result);
                return false;
            }

            return true;
        }

        public bool TryPersistSpatialAnchor(ulong spatialAnchorStore, ulong spatialAnchorHandle, string anchorName, out SpacesAnchorStore.SaveAnchorResult saveResult)
        {
            saveResult = SpacesAnchorStore.SaveAnchorResult.FAILURE_RUNTIME_ERROR;
            if (_xrPersistSpatialAnchorMSFT == null)
            {
                Debug.LogError("xrPersistSpatialAnchorMSFT method not found!");
                return false;
            }

            XrSpatialAnchorPersistenceInfoMSFT info = new XrSpatialAnchorPersistenceInfoMSFT(anchorName, spatialAnchorHandle);
            XrResult result = _xrPersistSpatialAnchorMSFT(spatialAnchorStore, ref info);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to persist spatial anchor: " + result);
                if (result == XrResult.XR_ERROR_SPATIAL_ANCHOR_INSUFFICIENT_QUALITY_QCOM)
                {
                    saveResult = SpacesAnchorStore.SaveAnchorResult.FAILURE_INSUFFICIENT_QUALITY;
                }
                return false;
            }

            saveResult = SpacesAnchorStore.SaveAnchorResult.SAVED;
            return true;
        }

        public bool TryUnpersistSpatialAnchor(ulong spatialAnchorStore, string anchorName)
        {
            if (_xrUnpersistSpatialAnchorMSFT == null)
            {
                Debug.LogError("xrUnpersistSpatialAnchorMSFT method not found!");
                return false;
            }

            XrSpatialAnchorPersistenceNameMSFT spatialAnchorPersistenceName = new XrSpatialAnchorPersistenceNameMSFT(anchorName);
            XrResult result = _xrUnpersistSpatialAnchorMSFT(spatialAnchorStore, ref spatialAnchorPersistenceName);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to unpersist spatial anchor: " + result);
                return false;
            }

            return true;
        }

        public bool TryCreateSpatialAnchorFromPersistedNameMSFT(ulong spatialAnchorStore, string spatialAnchorName, out ulong spatialAnchor)
        {
            spatialAnchor = 0;
            if (_xrCreateSpatialAnchorFromPersistedNameMSFT == null)
            {
                Debug.LogError("xrCreateSpatialAnchorFromPersistedNameMSFT method not found!");
                return false;
            }

            XrSpatialAnchorFromPersistedAnchorCreateInfoMSFT createInfo = new XrSpatialAnchorFromPersistedAnchorCreateInfoMSFT(spatialAnchorStore, spatialAnchorName);
            XrResult result = _xrCreateSpatialAnchorFromPersistedNameMSFT(SessionHandle, ref createInfo, ref spatialAnchor);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to create spatial anchor from persisted name: " + result);
                return false;
            }

            return true;
        }

        public bool TryEnumeratePersistedSpatialAnchorNames(ulong spatialAnchorStore, out string[] namesList)
        {
            namesList = Array.Empty<string>();
            if (_xrEnumeratePersistedSpatialAnchorNamesMSFT == null)
            {
                Debug.LogError("xrEnumeratePersistedSpatialAnchorNamesMSFT method not found!");
                return false;
            }

            uint namesCountOutput = 0;
            XrResult result = _xrEnumeratePersistedSpatialAnchorNamesMSFT(spatialAnchorStore,
                0,
                ref namesCountOutput,
                IntPtr.Zero);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get spatial anchor names count output: " + result);
                return false;
            }

            if (namesCountOutput == 0)
            {
                Debug.Log("No spatial anchor names found.");
                return true;
            }

            uint namesCapacityInput = namesCountOutput;
            using ScopeArrayPtr<XrSpatialAnchorPersistenceNameMSFT> namesPtr = new((int)namesCountOutput);
            result = _xrEnumeratePersistedSpatialAnchorNamesMSFT(spatialAnchorStore,
                namesCapacityInput,
                ref namesCountOutput,
                namesPtr.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to enumerate persisted spatial anchor names: " + result);
                return false;
            }

            namesList = new string[namesCountOutput];
            for (int i = 0; i < namesCountOutput; i++)
            {
                var anchorName = namesPtr.AtIndex(i);
                namesList[i] = anchorName.Name;
            }

            return true;
        }

        protected override string GetXrLayersToLoad()
        {
            return "XR_APILAYER_QCOM_spatial_anchor";
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

            return true;
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(_spatialAnchorsSubsystemDescriptors, SpatialAnchorsSubsystem.ID);
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRAnchorSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRAnchorSubsystem>();
        }

        protected override void OnHookMethods()
        {
            HookMethod("xrCreateSpatialAnchorMSFT", out _xrCreateSpatialAnchorMSFTPtr);
            HookMethod("xrCreateSpatialAnchorSpaceMSFT", out _xrCreateSpatialAnchorSpaceMSFTPtr);
            HookMethod("xrDestroySpatialAnchorMSFT", out _xrDestroySpatialAnchorMSFTPtr);
            HookMethod("xrLocateSpace", out _xrLocateSpacePtr);
            HookMethod("xrCreateSpatialAnchorStoreConnectionMSFT", out _xrCreateSpatialAnchorStoreConnectionMSFT);
            HookMethod("xrDestroySpatialAnchorStoreConnectionMSFT", out _xrDestroySpatialAnchorStoreConnectionMSFT);
            HookMethod("xrPersistSpatialAnchorMSFT", out _xrPersistSpatialAnchorMSFT);
            HookMethod("xrUnpersistSpatialAnchorMSFT", out _xrUnpersistSpatialAnchorMSFT);
            HookMethod("xrEnumeratePersistedSpatialAnchorNamesMSFT", out _xrEnumeratePersistedSpatialAnchorNamesMSFT);
            HookMethod("xrCreateSpatialAnchorFromPersistedNameMSFT", out _xrCreateSpatialAnchorFromPersistedNameMSFT);
            HookMethod("xrClearSpatialAnchorStoreMSFT", out _xrClearSpatialAnchorStoreMSFT);
        }
    }
}
