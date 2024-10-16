/******************************************************************************
 * File: CameraAccessFeature.Delegates.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class CameraAccessFeature : SpacesOpenXRFeature
    {
        #region XR_QCOM_camera_access bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult EnumerateCamerasQCOMDelegate(ulong session, uint cameraInfoCapacityInput, ref uint cameraInfoCountOutput, IntPtr /*ref XrCameraInfoQCOM[]*/ cameraInfos);

        private static EnumerateCamerasQCOMDelegate _xrEnumerateCamerasQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetSupportedFrameConfigurationsQCOMDelegate(ulong session, [MarshalAs(UnmanagedType.LPTStr)] string cameraSet, uint frameConfigurationCapacity, ref uint frameConfigurationCount, IntPtr /* XrCameraFrameConfigurationQCOM[] */ frameConfigurations);

        private static GetSupportedFrameConfigurationsQCOMDelegate _xrGetSupportedFrameConfigurationsQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateCameraHandleQCOMDelegate(ulong session, ref XrCameraActivationInfoQCOM activationInfo, ref ulong cameraHandle);

        private static CreateCameraHandleQCOMDelegate _xrCreateCameraHandleQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult ReleaseCameraHandleQCOMDelegate(ulong cameraHandle);

        private static ReleaseCameraHandleQCOMDelegate _xrReleaseCameraHandleQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult AccessFrameQCOMDelegate(ulong cameraHandle, ref XrCameraFrameDataQCOM frameData, ref XrCameraFrameBuffersQCOM frameBuffers);

        private static AccessFrameQCOMDelegate _xrAccessFrameQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult ReleaseFrameQCOMDelegate(ulong frame);

        private static ReleaseFrameQCOMDelegate _xrReleaseFrameQCOM;

        #endregion

        #region XR_KHR_convert_timespec_time bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrConvertTimespecTimeToTimeKHR(ulong instance, ref Timespec timespecTime, ref long time);

        private static xrConvertTimespecTimeToTimeKHR _xrConvertTimespecTimeToTimeKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrConvertTimeToTimespecTimeKHR(ulong instance, long time, ref Timespec timespecTime);

        private static xrConvertTimeToTimespecTimeKHR _xrConvertTimeToTimespecTimeKHR;

        #endregion
    }
}
