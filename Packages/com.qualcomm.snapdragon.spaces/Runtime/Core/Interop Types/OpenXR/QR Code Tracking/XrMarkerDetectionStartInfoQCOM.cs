/******************************************************************************
 * File: XrMarkerDetectionStartInfoQCOM.cs
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
    internal struct XrMarkerDetectionStartInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _markerTypeCount;
        private IntPtr /* XrMarkerTypeQCOM* */ _markerTypes;

        public XrMarkerDetectionStartInfoQCOM(uint markerTypeCount, IntPtr markerTypes, IntPtr qrCodeVersionFilter)
        {
            _type = XrStructureType.XR_TYPE_MARKER_DETECTION_START_INFO_QCOMX;
            _next = qrCodeVersionFilter;
            _markerTypeCount = markerTypeCount;
            _markerTypes = markerTypes;
        }

        public XrMarkerDetectionStartInfoQCOM(uint markerTypeCount, IntPtr markerTypes)
        {
            _type = XrStructureType.XR_TYPE_MARKER_DETECTION_START_INFO_QCOMX;
            _next = IntPtr.Zero;
            _markerTypeCount = markerTypeCount;
            _markerTypes = markerTypes;
        }
    }
}
