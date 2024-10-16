/******************************************************************************
 * File: XrPlanesLocateInfoQCOM.cs
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
    internal struct XrPlanesLocateInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _baseSpace;
        private long _time;

        public XrPlanesLocateInfoQCOM(ulong xrSpaceHandle, long time)
        {
            _type = XrStructureType.XR_TYPE_PLANES_LOCATE_INFO_QCOM;
            _next = IntPtr.Zero;
            _baseSpace = xrSpaceHandle;
            _time = time;
        }
    }
}
