/******************************************************************************
 * File: CompositionLayersFeature.Delegates.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{

    internal partial class CompositionLayersFeature
    {
        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetCompositionLayerFeatureEnabled")]
        private static extern uint Internal_SetCompositionLayerFeatureEnabled(bool isFeatureEnabled);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "CreateCompositionLayer")]
        private static extern uint Internal_CreateCompositionLayer(ulong instance, ulong session, uint width, uint height, int sortingOrder, SpacesNativeCompositionLayerType layerType, bool useAndroidSurfaceSwapchain);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "DestroyCompositionLayer")]
        private static extern void Internal_DestroyCompositionLayer(uint layerId);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "DestroyCompositionLayersInSession")]
        private static extern void Internal_DestroyCompositionLayersInSession(ulong session);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "AcquireSwapchainImageForLayer")]
        private static extern IntPtr Internal_AcquireSwapchainImageForLayer(uint layerId);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "ReleaseSwapchainImageForLayer")]
        private static extern void Internal_ReleaseSwapchainImageForLayer(uint layerId);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetSizeForQuadLayer")]
        private static extern void Internal_SetSizeForQuadLayer(uint layerId, float width, float height);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetRadiusForCylinderLayer")]
        private static extern void Internal_SetRadiusForCylinderLayer(uint layerId, float radius);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetCentralAngleForCylinderLayer")]
        private static extern void Internal_SetCentralAngleForCylinderLayer(uint layerId, float angle);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetRadiusForEquirectLayer")]
        private static extern void Internal_SetRadiusForEquirectLayer(uint layerId, float radius);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetCentralHorizontalAngleForEquirectLayer")]
        private static extern void Internal_SetCentralHorizontalAngleForEquirectLayer(uint layerId, float angle);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetLowerVerticalAngleForEquirectLayer")]
        private static extern void Internal_SetLowerVerticalAngleForEquirectLayer(uint layerId, float angle);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetUpperVerticalAngleForEquirectLayer")]
        private static extern void Internal_SetUpperVerticalAngleForEquirectLayer(uint layerId, float angle);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetOrientationForLayer")]
        private static extern void Internal_SetOrientationForLayer(uint layerId, float x, float y, float z, float w);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetPositionForLayer")]
        private static extern void Internal_SetPositionForLayer(uint layerId, float x, float y, float z);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetSortingOrderForLayer")]
        private static extern void Internal_SetSortingOrderForLayer(uint layerId, int sortingOrder);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetLayerVisible")]
        private static extern void Internal_SetLayerVisible(uint layerId, bool visible);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetMaxCompositionLayerCount")]
        private static extern void Internal_SetMaxCompositionLayerCount(uint maxLayerCount);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "IsXrFrameInProgress")]
        private static extern bool Internal_IsXrFrameInProgress();

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SendToSwapchain")]
        private static extern void Internal_SendToSwapchain(uint layerId, IntPtr texture, int width, int height);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "UseNativeTexture")]
        private static extern bool Internal_UseNativeTexture();

#if UNITY_ANDROID
        [DllImport(InterceptOpenXRLibrary, EntryPoint = "GetAndroidSurfaceObject")]
        private static extern IntPtr Internal_GetAndroidSurfaceObject(uint layerId);

        // [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetUseAndroidSurfaceSwapchain")]
        // private static extern void Internal_SetUseAndroidSurfaceSwapchain(uint layerId, bool useAndroidSurfaceSwapchain);
#endif
    }
}
