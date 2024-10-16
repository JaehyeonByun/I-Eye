/******************************************************************************
 * File: XrPlaneExtentQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrPlaneExtentQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrExtent2DfQCOM _extentX;
        private XrExtent2DfQCOM _extentY;

        public XrExtent2DfQCOM ExtentX => _extentX;
        public XrExtent2DfQCOM ExtentY => _extentY;
    }
}
