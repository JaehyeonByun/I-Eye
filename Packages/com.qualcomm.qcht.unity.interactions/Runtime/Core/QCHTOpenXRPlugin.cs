// /******************************************************************************
//  * File: QCHTOpenXRPlugin.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace QCHT.Interactions.Core
{
    internal static class QCHTOpenXRPlugin
    {
        private const string DllName = "QCHTOpenXRPlugin";

        [DllImport(DllName)]
        internal static extern IntPtr GetInterceptedInstanceProcAddr(IntPtr func);

        [DllImport(DllName)]
        internal static extern int SetAppSpace(ulong xrSpace);

        [DllImport(DllName)]
        internal static extern bool IsHandTrackingSupported();
        
        #region XR_EXT_hand_tracking
        
        [DllImport(DllName)]
        internal static extern XrResult TryLocateHandJoints(XrHandEXT handedness,
            ref bool isTracked,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int) XrHandJoint.XR_HAND_JOINT_MAX)]
            XrPosef[] handPoses,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int) XrHandJoint.XR_HAND_JOINT_MAX)]
            float[] radius);

        #endregion
        
        #region XR_QCOM_hand_tracking_gesture
        
        [DllImport(DllName)]
        internal static extern XrResult TryGetHandGestureData(XrHandEXT handExt, ref XrHandGestureQCOM data);
        
        [DllImport(DllName)]
        internal static extern XrResult TryGetInteractionData(XrHandEXT handExt, ref XrHandGestureV2QCOM data);
        
        #endregion
        
        #region Passthrough

        [DllImport(DllName)]
        internal static extern void SetPassthroughEnabled(bool enable);

        [DllImport(DllName)]
        internal static extern bool GetPassthroughEnabled();

        [DllImport(DllName)]
        internal static extern bool IsPassthroughSupported();

        #endregion
    }
}