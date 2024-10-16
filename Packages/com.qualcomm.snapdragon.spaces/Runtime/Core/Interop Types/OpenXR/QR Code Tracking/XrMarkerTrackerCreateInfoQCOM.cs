/******************************************************************************
 * File: XrMarkerTrackerCreateInfoQCOM.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrMarkerTrackerCreateInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;

        public void InitStructureType()
        {
            _type = XrStructureType.XR_TYPE_MARKER_TRACKER_CREATE_INFO_QCOMX;
            _next = IntPtr.Zero;
        }
    }
}
