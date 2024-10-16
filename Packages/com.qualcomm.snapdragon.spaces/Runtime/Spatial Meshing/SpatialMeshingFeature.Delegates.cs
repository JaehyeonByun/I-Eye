/******************************************************************************
 * File: SceneUnderstandingFeature.Delegates.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class SpatialMeshingFeature
    {
        private const string Library = "libMeshingProvider";

        [DllImport(Library, EntryPoint = "GetInterceptedInstanceProcAddr")]
        private static extern IntPtr Internal_GetInterceptedInstanceProcAddr(IntPtr xrGetInstanceProcAddr);

        [DllImport(Library, EntryPoint = "RegisterMeshingLifecycleProvider")]
        private static extern void Internal_RegisterMeshingLifecycleProvider();

        [DllImport(Library, EntryPoint = "SetInstanceHandle")]
        private static extern void Internal_SetInstanceHandle(ulong instance);

        [DllImport(Library, EntryPoint = "SetSessionHandle")]
        private static extern void Internal_SetSessionHandle(ulong session);

        [DllImport(Library, EntryPoint = "SetSpaceHandle")]
        private static extern void Internal_SetSpaceHandle(ulong space);

        [DllImport(Library, EntryPoint = "RegisterProviderWithSceneObserver")]
        private static extern void Internal_RegisterProviderWithSceneObserver([MarshalAs(UnmanagedType.LPStr)] string subsystemId, int requestedFeatures);

        [DllImport(Library, EntryPoint = "UnregisterProviderWithSceneObserver")]
        private static extern void Internal_UnregisterProviderWithSceneObserver([MarshalAs(UnmanagedType.LPStr)] string subsystemId);

        #region XR_MSFT_scene_understanding bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateSceneObserverMSFTDelegate(ulong session, ref XrSceneObserverCreateInfoMSFT createInfo, ref ulong sceneObserver);

        private static CreateSceneObserverMSFTDelegate _xrCreateSceneObserverMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult ComputeNewSceneMSFTDelegate(ulong sceneObserver, ref XrNewSceneComputeInfoMSFT computeInfo);

        private static ComputeNewSceneMSFTDelegate _xrComputeNewSceneMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetSceneComputeStateMSFTDelegate(ulong sceneObserver, ref XrSceneComputeStateMSFT state);

        private static GetSceneComputeStateMSFTDelegate _xrGetSceneComputeStateMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateSceneMSFTDelegate(ulong sceneObserver, ref XrSceneCreateInfoMSFT state, ulong scene);

        private static CreateSceneMSFTDelegate _xrCreateSceneMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetSceneComponentsMSFTDelegate(ulong scene, ref XrSceneComponentsGetInfoMSFT getInfo, ref XrSceneComponentsMSFT components);

        private static GetSceneComponentsMSFTDelegate _xrGetSceneComponentsMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult LocateSceneComponentsMSFTDelegate(ulong scene, ref XrSceneComponentsLocateInfoMSFT locateInfo, ref XrSceneComponentLocationsMSFT locations);

        private static LocateSceneComponentsMSFTDelegate _xrLocateSceneComponentMSFT;

        #endregion
    }
}
