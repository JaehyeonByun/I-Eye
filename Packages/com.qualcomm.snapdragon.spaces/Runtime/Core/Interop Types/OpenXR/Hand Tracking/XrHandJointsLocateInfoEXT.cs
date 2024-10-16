/******************************************************************************
 * File: XrHandJointsLocateInfoEXT.cs
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
    internal struct XrHandJointsLocateInfoEXT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _baseSpace;
        private long _time;

        public XrHandJointsLocateInfoEXT(ulong baseSpace, long time)
        {
            _type = XrStructureType.XR_TYPE_HAND_JOINTS_LOCATE_INFO_EXT;
            _next = IntPtr.Zero;
            _baseSpace = baseSpace;
            _time = time;
        }
    }
}
