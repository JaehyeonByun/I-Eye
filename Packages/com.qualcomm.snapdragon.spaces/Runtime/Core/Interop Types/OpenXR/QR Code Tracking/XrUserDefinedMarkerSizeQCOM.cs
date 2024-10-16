/******************************************************************************
 * File: XrUserDefinedMarkerSizeQCOM.cs
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
    internal struct XrUserDefinedMarkerSizeQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrExtent2Df _size;

        public XrUserDefinedMarkerSizeQCOM(IntPtr next, XrExtent2Df size)
        {
            _type = XrStructureType.XR_TYPE_USER_DEFINED_MARKER_SIZE_QCOMX;
            _next = next;
            _size = size;
        }
    }
}
