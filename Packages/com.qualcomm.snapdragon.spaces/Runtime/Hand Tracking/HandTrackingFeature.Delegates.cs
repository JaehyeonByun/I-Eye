/******************************************************************************
 * File: HandTrackingFeature.Delegates.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class HandTrackingFeature
    {
        #region XR_EXT_hand_tracking bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateHandTrackerEXTDelegate(ulong session, ref XrHandTrackerCreateInfoEXT createInfo, ref ulong handTracker);

        private static CreateHandTrackerEXTDelegate _xrCreateHandTrackerEXT;

        private delegate XrResult DestroyHandTrackerEXTDelegate(ulong handTracker);

        private static DestroyHandTrackerEXTDelegate _xrDestroyHandTrackerEXT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult LocateHandJointsEXTDelegate(ulong handTracker, ref XrHandJointsLocateInfoEXT locateInfo, ref XrHandJointLocationsEXT locations);

        private static LocateHandJointsEXTDelegate _xrLocateHandJointsEXT;

        #endregion

        #region XR_QCOM_hand_tracking_gesture bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetHandGestureQCOMDelegate(ulong handTracker, long time, ref XrHandGestureQCOM gesture);

        private static GetHandGestureQCOMDelegate _xrGetHandGestureQCOM;

        #endregion
    }
}
