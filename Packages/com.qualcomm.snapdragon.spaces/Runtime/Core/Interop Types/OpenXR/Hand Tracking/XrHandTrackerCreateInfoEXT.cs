/******************************************************************************
 * File: XrHandTrackerCreateInfoEXT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete]
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrHandTrackerCreateInfoEXT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrHandEXT _hand;
        private XrHandJointSetEXT _handJointSet;

        public XrHandTrackerCreateInfoEXT(XrHandEXT hand)
        {
            _type = XrStructureType.XR_TYPE_HAND_TRACKER_CREATE_INFO_EXT;
            _next = IntPtr.Zero;
            _hand = hand;
            _handJointSet = XrHandJointSetEXT.XR_HAND_JOINT_SET_MAX_ENUM_EXT;
        }
    }
}
