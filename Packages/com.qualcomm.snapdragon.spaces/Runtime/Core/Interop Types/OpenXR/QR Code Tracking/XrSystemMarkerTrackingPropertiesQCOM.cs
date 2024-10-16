/******************************************************************************
 * File: XrSystemMarkerTrackingPropertiesQCOM.cs
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
    internal struct XrSystemMarkerTrackingPropertiesQCOM {
        private XrStructureType _type;
        private IntPtr _next;
        private bool _supportsMarkerTracking;
        private XrMarkerTrackingModeQCOM defaultTrackingMode;
        private XrUserDefinedMarkerSizeSupportQCOM userDefinedMarkerSizeSupport;
    }
}
