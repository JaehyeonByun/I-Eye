/******************************************************************************
 * File: QrCodeTrackingFeature.Delegates.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class QrCodeTrackingFeature
    {
        #region XR_QCOMX_marker_tracking bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult EnumerateMarkerTypesQCOMDelegate(ulong session,
            uint markerTypeCapacityInput, ref uint markerTypeCountOutput, IntPtr /*XrMarkerTypeQCOM*/ markerTypes);
        private static EnumerateMarkerTypesQCOMDelegate _xrEnumerateMarkerTypesQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult EnumerateMarkerTrackingModesQCOMDelegate(ulong session,
            uint trackingModeCapacityInput, ref uint trackingModeCountOutput, IntPtr /*XrMarkerTrackingModeQCOM*/ trackingModes);
        private static EnumerateMarkerTrackingModesQCOMDelegate _xrEnumerateMarkerTrackingModesQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateMarkerTrackerQCOMDelegate(ulong session,
            ref XrMarkerTrackerCreateInfoQCOM createInfo,
            ref ulong markerTracker);
        private static CreateMarkerTrackerQCOMDelegate _xrCreateMarkerTrackerQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult DestroyMarkerTrackerQCOMDelegate(ulong markerTracker);
        private static DestroyMarkerTrackerQCOMDelegate _xrDestroyMarkerTrackerQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult StartMarkerDetectionQCOMDelegate(ulong markerTracker, ref XrMarkerDetectionStartInfoQCOM startInfo);
        private static StartMarkerDetectionQCOMDelegate _xrStartMarkerDetectionQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult StopMarkerDetectionQCOMDelegate(ulong markerTracker);
        private static StopMarkerDetectionQCOMDelegate _xrStopMarkerDetectionQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetMarkerSizeQCOMDelegate(ulong markerTracker, IntPtr /*XrExtent2DfQCOM*/ size);
        private static GetMarkerSizeQCOMDelegate _xrGetMarkerSizeQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetMarkerTypeQCOMDelegate(ulong marker, IntPtr /*XrMarkerTypeQCOM*/ type);
        private static GetMarkerTypeQCOMDelegate _xrGetMarkerTypeQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetQrCodeVersionQCOM(ulong marker, IntPtr /*XrQrCodeVersionQCOM*/ version);
        private static GetQrCodeVersionQCOM _xrGetQrCodeVersionQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetQrCodeStringDataQCOMDelegate(ulong marker, uint bufferCapacityInput,
            ref uint bufferCountOutput, IntPtr buffer);
        private static GetQrCodeStringDataQCOMDelegate _xrGetQrCodeStringDataQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetQrCodeRawDataQCOMDelegate(ulong marker, uint bufferCapacityInput,
            ref uint bufferCountOutput, IntPtr buffer);
        private static GetQrCodeRawDataQCOMDelegate _xrGetQrCodeRawDataQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateMarkerSpaceQCOMDelegate(ulong marker, ref XrMarkerSpaceCreateInfoQCOM createInfo, ref ulong space);
        private static CreateMarkerSpaceQCOMDelegate _xrCreateMarkerSpaceQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult PollMarkerUpdateQCOMDelegate(ulong tracker, ref ulong update);
        private static PollMarkerUpdateQCOMDelegate _xrPollMarkerUpdateQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetMarkerUpdateInfoQCOMDelegate(ulong update, ref uint updateInfoCount,
            ref IntPtr /* XrMarkerUpdateInfoQCOM** */ updateInfos);
        private GetMarkerUpdateInfoQCOMDelegate _xrGetMarkerUpdateInfoQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult ReleaseMarkerUpdateQCOMDelegate(ulong update);
        private static ReleaseMarkerUpdateQCOMDelegate _xrReleaseMarkerUpdateQCOM;

        #endregion

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult LocateSpaceDelegate(ulong space, ulong baseSpace, long time, ref XrSpaceLocation location);
        private static LocateSpaceDelegate _xrLocateSpace;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult DestroySpaceDelegate(ulong space);
        private static DestroySpaceDelegate _xrDestroySpace;
    }
}
