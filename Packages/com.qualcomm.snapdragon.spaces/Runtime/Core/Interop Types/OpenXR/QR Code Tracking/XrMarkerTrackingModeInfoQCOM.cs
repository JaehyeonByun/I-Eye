/******************************************************************************
 * File: XrMarkerTrackingModeInfoQCOM.cs
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
    internal struct XrMarkerTrackingModeInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrMarkerTrackingModeQCOM _trackingMode;

        public XrMarkerTrackingModeInfoQCOM(XrMarkerTrackingModeQCOM trackingMode)
        {
            _type = XrStructureType.XR_TYPE_MARKER_TRACKING_MODE_INFO_QCOMX;
            _next = IntPtr.Zero;
            _trackingMode = trackingMode;
        }
    }
}
