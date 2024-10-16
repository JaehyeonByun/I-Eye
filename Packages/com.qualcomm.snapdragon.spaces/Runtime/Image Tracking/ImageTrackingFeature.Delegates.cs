/******************************************************************************
 * File: ImageTrackingFeature.Delegates.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class ImageTrackingFeature
    {
        #region XR_QCOM_image_tracking bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateImageTrackerQCOMDelegate(ulong session, ref XrImageTrackerCreateInfoQCOM createInfo, ref ulong imageTracker);

        private static CreateImageTrackerQCOMDelegate _xrCreateImageTrackerQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult DestroyImageTrackerQCOMDelegate(ulong imageTracker);

        private static DestroyImageTrackerQCOMDelegate _xrDestroyImageTrackerQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult LocateImageTargetsQCOMDelegate(ulong imageTracker, ref XrImageTargetsLocateInfoQCOM locateInfo, ref XrImageTargetLocationsQCOM locations);

        private static LocateImageTargetsQCOMDelegate _xrLocateImageTargetsQCOM;

        private delegate XrResult ImageTargetToNameAndIdQCOMDelegate(ulong imageTarget, uint bufferCapacityInput, ref uint bufferCountOutput, IntPtr buffer, ref uint id);

        private static ImageTargetToNameAndIdQCOMDelegate _xrImageTargetToNameAndIdQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult SetImageTargetsTrackingModeQCOMDelegate(ulong imageTracker, XrImageTargetsTrackingModeInfoQCOM trackingModeInfo);

        private static SetImageTargetsTrackingModeQCOMDelegate _xrSetImageTargetsTrackingModeQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult StopImageTargetTrackingQCOMDelegate(ulong imageTarget);

        private static StopImageTargetTrackingQCOMDelegate _xrStopImageTargetTrackingQCOM;

        #endregion

        #region XR_QCOM_tracking_optimization_settings bindings

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        private delegate int xrSetTrackingOptimizationSettingsHintQCOMDelegate(ulong session, XrTrackingOptimizationSettingsDomainQCOM domain, XrTrackingOptimizationSettingsHintQCOM hint);

        private static xrSetTrackingOptimizationSettingsHintQCOMDelegate _xrSetTrackingOptimizationSettingsHintQCOM;

        #endregion
    }
}
