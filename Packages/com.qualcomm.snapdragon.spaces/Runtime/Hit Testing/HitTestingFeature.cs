/******************************************************************************
 * File: RaycastFeature.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
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
#if UNITY_EDITOR
    [OpenXRFeature(UiName = FeatureName,
        BuildTargetGroups = new[]
        {
            BuildTargetGroup.Android
        },
        Company = "Qualcomm",
        Desc = "Enables Hit Testing feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class HitTestingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Hit Testing";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.raycasting";
        public const string FeatureExtensions = "XR_QCOM_ray_casting";
        private static readonly List<XRRaycastSubsystemDescriptor> _raycastSubsystemDescriptors = new List<XRRaycastSubsystemDescriptor>();

        private PlaneDetectionFeature _planeDetectionFeature;

        public bool TryCreateRaycast(out ulong rayCastHandle)
        {
            rayCastHandle = 0;
            if (_xrCreateRayCastQCOM == null)
            {
                Debug.LogError("XrCreateRayCastQCOM method not found!");
                return false;
            }

            XrRayCastTargetTypeQCOM[] targetTypes =
            {
                XrRayCastTargetTypeQCOM.XR_RAY_CAST_TARGET_TYPE_PLANE_QCOM,
                XrRayCastTargetTypeQCOM.XR_RAY_CAST_TARGET_TYPE_MESH_QCOM
            };
            XrRayCastCreateInfoQCOM createInfo = new XrRayCastCreateInfoQCOM(XrRayTypeQCOM.XR_RAY_TYPE_STOPPING_QCOM, (uint)targetTypes.Length, targetTypes);
            using ScopePtr<ulong> rayCastHandlePtr = new();
            XrResult result = _xrCreateRayCastQCOM(SessionHandle, ref createInfo, rayCastHandlePtr.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to create RayCast: " + result);
                return false;
            }

            rayCastHandle = rayCastHandlePtr.AsStruct();
            return true;
        }

        public bool TryDestroyRayCast(ulong rayCastHandle)
        {
            if (_xrDestroyRayCastQCOM == null)
            {
                Debug.LogError("XrDestroyRayCastQCOM method not found!");
                return false;
            }

            XrResult result = _xrDestroyRayCastQCOM(rayCastHandle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to destroy RayCast: " + result);
                return false;
            }

            return true;
        }

        public bool TryCastRay(ulong rayCastHandle, Vector3 origin, Vector3 direction, float maxDistance)
        {
            if (_xrCastRayQCOM == null)
            {
                Debug.LogError("XrCastRayQCOM method not found!");
                return false;
            }

            XrResult result = _xrCastRayQCOM(rayCastHandle, SpaceHandle, new XrVector3f(origin), new XrVector3f(direction), maxDistance);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("CastRay failed: " + result);
                return false;
            }

            return true;
        }

        public bool TryGetRaycastState(ulong rayCastHandle)
        {
            if (_xrGetRayCastStateQCOM == null)
            {
                Debug.LogError("XrGetRayCastStateQCOM method not found!");
                return false;
            }

            XrRayCastStateQCOM state = XrRayCastStateQCOM.XR_RAY_CAST_STATE_NONE_QCOM;
            XrResult result = _xrGetRayCastStateQCOM(rayCastHandle, ref state);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get RayCast State: " + result);
                return false;
            }

            if (state != XrRayCastStateQCOM.XR_RAY_CAST_STATE_FINISHED_QCOM)
            {
                return false;
            }

            return true;
        }

        public bool TryGetRayCastCollisions(ulong rayCastHandle, Vector3 origin, out List<XRRaycastHit> raycastHits)
        {
            raycastHits = new List<XRRaycastHit>();
            if (_xrGetRayCollisionsQCOM == null)
            {
                Debug.LogError("XrGetRayCollisionsQCOM method not found!");
                return false;
            }

            var getInfo = new XrRayCollisionsGetInfoQCOM(SpaceHandle);
            using ScopePtr<uint> collisionsCapacityOutputPtr = new();
            collisionsCapacityOutputPtr.Copy(0);
            XrRayCollisionsQCOM rayCollisions = new XrRayCollisionsQCOM(collisionsCapacityOutputPtr.Raw);
            XrResult result = _xrGetRayCollisionsQCOM(rayCastHandle, ref getInfo, ref rayCollisions);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get RayCast collisions capacity output: " + result);
                return false;
            }

            uint collisionsCapacityInput = collisionsCapacityOutputPtr.AsStruct();
            if (collisionsCapacityInput == 0)
            {
                return true;
            }

            using ScopeArrayPtr<XrRayCollisionQCOM> collisionsPtr = new((int)collisionsCapacityInput);
            var defaultRayCollision = new XrRayCollisionQCOM(0, 0, 0, XrVector3f.zero, XrVector3f.zero, 0);
            for (int i = 0; i < collisionsCapacityInput; i++)
            {
                collisionsPtr.Copy(defaultRayCollision, i);
            }

            rayCollisions = new XrRayCollisionsQCOM(collisionsCapacityInput, collisionsCapacityOutputPtr.Raw, collisionsPtr.Raw);
            result = _xrGetRayCollisionsQCOM(rayCastHandle, ref getInfo, ref rayCollisions);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get RayCast collisions: " + result);
                return false;
            }

            for (int i = 0; i < collisionsCapacityInput; i++)
            {
                var rayCollision = collisionsPtr.AtIndex(i);
                Vector3 position = rayCollision.Position;
                raycastHits.Add(new XRRaycastHit(new TrackableId(_planeDetectionFeature.ActiveHandle, rayCollision.TargetId), new Pose(position, Quaternion.LookRotation(rayCollision.SurfaceNormal)), Vector3.Distance(position, origin), TrackableType.Planes));
            }

            return true;
        }

        protected override string GetXrLayersToLoad()
        {
            // Should not be tied to a specific layer - the extension can come from multiple locations.
            return "";
        }

        protected override bool OnInstanceCreate(ulong instanceHandle)
        {
            base.OnInstanceCreate(instanceHandle);
            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any())
            {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }

            BaseRuntimeFeature baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            _planeDetectionFeature = OpenXRSettings.Instance.GetFeature<PlaneDetectionFeature>();
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!baseRuntimeFeature.CheckServicesCameraPermissions())
            {
                Debug.LogError("The Hit Testing Feature is missing the camera permissions and can't be created therefore!");
                return false;
            }
#endif
            return true;
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(_raycastSubsystemDescriptors, RaycastSubsystem.ID);
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRRaycastSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRRaycastSubsystem>();
        }

        protected override void OnHookMethods()
        {
            HookMethod("xrCreateRayCastQCOM", out _xrCreateRayCastQCOM);
            HookMethod("xrDestroyRayCastQCOM", out _xrDestroyRayCastQCOM);
            HookMethod("xrGetRayCastStateQCOM", out _xrGetRayCastStateQCOM);
            HookMethod("xrCastRayQCOM", out _xrCastRayQCOM);
            HookMethod("xrGetRayCollisionsQCOM", out _xrGetRayCollisionsQCOM);
        }
    }
}
