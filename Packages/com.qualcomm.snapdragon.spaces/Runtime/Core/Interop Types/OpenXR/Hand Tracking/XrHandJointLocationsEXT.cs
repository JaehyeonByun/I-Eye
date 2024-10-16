/******************************************************************************
 * File: XrHandJointLocationsEXT.cs
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
    internal struct XrHandJointLocationsEXT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private bool _isActive;
        private int _jointCount;
        private IntPtr _jointLocations;

        public XrHandJointLocationsEXT(int jointCount, IntPtr jointLocations)
        {
            _type = XrStructureType.XR_TYPE_HAND_JOINT_LOCATIONS_EXT;
            _next = IntPtr.Zero;
            _isActive = false;
            _jointCount = jointCount;
            _jointLocations = jointLocations;
        }

        public bool IsActive => _isActive;
        public int JointCount => _jointCount;
        public IntPtr JointLocations => _jointLocations;
    }
}
