/******************************************************************************
 * File: XrMarkerSpaceCreateInfoQCOM.cs
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
    internal struct XrMarkerSpaceCreateInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrPosef _poseInMarkerSpace;

        public XrMarkerSpaceCreateInfoQCOM(IntPtr next, XrPosef pose)
        {
            _type = XrStructureType.XR_TYPE_MARKER_SPACE_CREATE_INFO_QCOMX;
            _next = next;
            _poseInMarkerSpace = pose;
        }
    }
}
