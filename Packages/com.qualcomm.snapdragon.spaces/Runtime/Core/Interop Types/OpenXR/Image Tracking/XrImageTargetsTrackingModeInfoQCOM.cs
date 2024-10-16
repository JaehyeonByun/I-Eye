/******************************************************************************
 * File: XrImageTargetsTrackingModeInfoQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrImageTargetsTrackingModeInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private int _targetCount;

        // const char*
        private IntPtr _targetNames;

        //const XrImageTargetTrackingModeQCOM*
        private IntPtr _targetModes;

        public XrImageTargetsTrackingModeInfoQCOM(int targetCount, IntPtr targetNames, IntPtr targetModes)
        {
            _type = XrStructureType.XR_TYPE_IMAGE_TARGETS_TRACKING_MODE_INFO_QCOM;
            _next = IntPtr.Zero;
            _targetCount = targetCount;
            _targetNames = targetNames;
            _targetModes = targetModes;
        }
    }
}
