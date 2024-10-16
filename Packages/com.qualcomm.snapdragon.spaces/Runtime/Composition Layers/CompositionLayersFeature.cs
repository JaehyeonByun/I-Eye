/******************************************************************************
 * File: CompositionLayersFeature.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;
using System.Linq;
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
        Desc = "Enables Composition Layers feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class CompositionLayersFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Composition Layers (Experimental)";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.compositionlayers";
        public const string FeatureExtensions = "XR_KHR_composition_layer_cylinder XR_KHR_composition_layer_equirect2 XR_FB_composition_layer_alpha_blend XR_KHR_android_surface_swapchain";

        protected override bool IsRequiringBaseRuntimeFeature => true;

        public delegate void CompositionLayerCreatedEventHandler(uint layerId);
        public event CompositionLayerCreatedEventHandler OnCompositionLayerCreated;
        public delegate void CompositionLayerDestroyedEventHandler(uint layerId);
        public event CompositionLayerDestroyedEventHandler OnCompositionLayerDestroyed;

        protected override bool OnInstanceCreate(ulong instanceHandle)
        {
            base.OnInstanceCreate(instanceHandle);

            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any())
            {
                Debug.Log(FeatureName + " is missing the following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }

            Internal_SetCompositionLayerFeatureEnabled(true);

            if (SystemIDHandle != 0)
            {
                Internal_GetSystemProperties();
            }

            return true;
        }

        protected override void OnSystemChange(ulong systemIDHandle)
        {
            base.OnSystemChange(systemIDHandle);

            if (InstanceHandle != 0)
            {
                Internal_GetSystemProperties();
            }
        }

        protected override void OnGetSystemProperties()
        {
            SetMaxCompositionLayerCount(SystemProperties.GetGraphicsProperties().MaxLayerCount);
        }

        internal uint CreateCompositionLayer(SpacesCompositionLayerType layerType, ulong instance, ulong session, uint width, uint height, int sortingOrder, bool useAndroidSurfaceSwapchain)
        {
            var graphicsProperties = SystemProperties.GetGraphicsProperties();

            if (width > graphicsProperties.MaxSwapchainImageWidth || height > graphicsProperties.MaxSwapchainImageHeight)
            {
                Debug.LogWarning($"Trying to create composition layer with dimensions: \"{width}, {height}\". Max Swapchain Image Dimensions are: \"{graphicsProperties.MaxSwapchainImageWidth}, {graphicsProperties.MaxSwapchainImageHeight}\"");
                return 0;
            }

            var layerId = Internal_CreateCompositionLayer(instance, session, width, height, sortingOrder, (SpacesNativeCompositionLayerType) layerType, useAndroidSurfaceSwapchain);
            OnCompositionLayerCreated?.Invoke(layerId);

            return layerId;
        }

        internal void DestroyCompositionLayer(uint layerId)
        {
            OnCompositionLayerDestroyed?.Invoke(layerId);
            Internal_DestroyCompositionLayer(layerId);
            RemoveActiveLayer(layerId);
        }

        internal void DestroyCompositionLayersInSession(ulong session)
        {
            Internal_DestroyCompositionLayersInSession(session);
            DecoupleLayersFromSession(session);
        }

        internal IntPtr AcquireSwapchainImageForLayer(uint layerId)
        {
            return Internal_AcquireSwapchainImageForLayer(layerId);
        }

        internal void ReleaseSwapchainImageForLayer(uint layerId)
        {
            Internal_ReleaseSwapchainImageForLayer(layerId);
        }

        public void SetSizeForQuadLayer(uint layerId, Vector2 extents)
        {
            Internal_SetSizeForQuadLayer(layerId, extents.x, extents.y);
        }

        public void SetRadiusForCylinderLayer(uint layerId, float radius)
        {
            Internal_SetRadiusForCylinderLayer(layerId, radius);
        }

        public void SetCentralAngleForCylinderLayer(uint layerId, float angle)
        {
            Internal_SetCentralAngleForCylinderLayer(layerId, angle);
        }

        public void SetRadiusForEquirectLayer(uint layerId, float radius)
        {
            Internal_SetRadiusForEquirectLayer(layerId, radius);
        }

        public void SetCentralHorizontalAngleForEquirectLayer(uint layerId, float angle)
        {
            Internal_SetCentralHorizontalAngleForEquirectLayer(layerId, angle);
        }

        public void SetLowerVerticalAngleForEquirectLayer(uint layerId, float angle)
        {
            Internal_SetLowerVerticalAngleForEquirectLayer(layerId, angle);
        }

        public void SetUpperVerticalAngleForEquirectLayer(uint layerId, float angle)
        {
            Internal_SetUpperVerticalAngleForEquirectLayer(layerId, angle);
        }

        internal void SetOrientationForLayer(uint layerId, Quaternion orientation)
        {
            Internal_SetOrientationForLayer(layerId, orientation.x, orientation.y, orientation.z, orientation.w);
        }

        internal void SetPositionForLayer(uint layerId, Vector3 position)
        {
            Internal_SetPositionForLayer(layerId, position.x, position.y, position.z);
        }

        public void SetSortingOrderForLayer(uint layerId, int sortingOrder)
        {
            Internal_SetSortingOrderForLayer(layerId, sortingOrder);
        }

        internal void SetLayerVisible(uint layerId, bool visible)
        {
            Internal_SetLayerVisible(layerId, visible);
        }

        internal void SetMaxCompositionLayerCount(uint maxLayerCount)
        {
            Internal_SetMaxCompositionLayerCount(maxLayerCount);
        }

        internal bool IsXrFrameInProgress()
        {
            return Internal_IsXrFrameInProgress();
        }

        internal void SendToSwapchain(uint layerId, IntPtr texture, int width, int height)
        {
            Internal_SendToSwapchain(layerId, texture, width, height);
        }

        internal bool UseNativeTexture()
        {
            return Internal_UseNativeTexture();
        }

#if UNITY_ANDROID
        internal SpacesAndroidSurface GetAndroidSurfaceObject(uint layerId)
        {
            IntPtr surfaceObject = Internal_GetAndroidSurfaceObject(layerId);
            if (surfaceObject == IntPtr.Zero)
            {
                return null;
            }

            return new SpacesAndroidSurface(layerId, surfaceObject);
        }

        // internal void SetUseAndroidSurfaceSwapchain(uint layerId, bool useAndroidSurfaceSwapchain)
        // {
        //     Internal_SetUseAndroidSurfaceSwapchain(layerId, useAndroidSurfaceSwapchain);
        // }
#endif
    }
}
