/******************************************************************************
 * File: PlaneDetectionFeature.Delegates.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class PlaneDetectionFeature
    {
        private const string Library = "libMeshingProvider";

        [DllImport(Library, EntryPoint = "GetInterceptedInstanceProcAddr")]
        private static extern IntPtr Internal_GetInterceptedInstanceProcAddr(IntPtr xrGetInstanceProcAddr);

        [DllImport(Library, EntryPoint = "RegisterProviderWithSceneObserver")]
        private static extern void Internal_RegisterProviderWithSceneObserver([MarshalAs(UnmanagedType.LPStr)] string subsystemId, int requestedFeatures);

        [DllImport(Library, EntryPoint = "UnregisterProviderWithSceneObserver")]
        private static extern void Internal_UnregisterProviderWithSceneObserver([MarshalAs(UnmanagedType.LPStr)] string subsystemId);

        [DllImport(Library, EntryPoint = "SetInstanceHandle")]
        private static extern void Internal_SetInstanceHandle(ulong instance);

        [DllImport(Library, EntryPoint = "SetSessionHandle")]
        private static extern void Internal_SetSessionHandle(ulong session);

        [DllImport(Library, EntryPoint = "SetSpaceHandle")]
        private static extern void Internal_SetSpaceHandle(ulong space);

        [DllImport(Library, EntryPoint = "UpdateObservedScene")]
        private static extern bool Internal_UpdateObservedScene([MarshalAs(UnmanagedType.LPStr)] string subsystemId);

        [DllImport(Library, EntryPoint = "UpdatePlanes")]
        private static extern bool Internal_UpdatePlanes();

        [DllImport(Library, EntryPoint = "CountScenePlanes")]
        private static extern bool Internal_CountScenePlanes(ref uint scenePlanesCount);

        [DllImport(Library, EntryPoint = "FetchScenePlanes")]
        private static extern bool Internal_FetchScenePlanes(uint scenePlanesCount, IntPtr scenePlanes);

        [DllImport(Library, EntryPoint = "FetchPlaneVertices")]
        private static extern bool Internal_FetchPlaneVertices(IntPtr scenePlane, IntPtr vertices, IntPtr indices);

        #region XR_QCOM_plane_detection bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreatePlaneDetectionQCOMDelegate(ulong session, ref XrPlaneDetectionCreateInfoQCOM createInfo, ref ulong planeDetection);

        private static CreatePlaneDetectionQCOMDelegate _xrCreatePlaneDetectionQCOM;

        private delegate XrResult DestroyPlaneDetectionQCOMDelegate(ulong planeDetection);

        private static DestroyPlaneDetectionQCOMDelegate _xrDestroyPlaneDetectionQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult LocatePlanesQCOMDelegate(ulong planeDetection, ref XrPlanesLocateInfoQCOM locateInfo, ref XrPlaneLocationsQCOM locations);

        private static LocatePlanesQCOMDelegate _xrLocatePlanesQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetPlaneDetectioNStateQCOMDelegate(ulong planeDetection, ref XrPlaneDetectionState state);

        private static GetPlaneDetectioNStateQCOMDelegate _xrGetPlaneDetectionStateQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrGetPlaneConvexHullVertexBufferQCOMDelegate(ulong planeDetection, ref XrPlaneConvexHullBufferInfoQCOM convexHullInfo, ref XrPlaneConvexHullVertexBufferQCOM buffers);

        private static xrGetPlaneConvexHullVertexBufferQCOMDelegate _xrGetPlaneConvexHullVertexBufferQCOM;

        #endregion
    }
}
