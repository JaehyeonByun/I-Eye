/******************************************************************************
 * File: XrGetSystemInfo.cs
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
    public struct XrGetSystemInfo
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrFormFactor _formFactor;

        internal XrGetSystemInfo(XrFormFactor formFactor)
        {
            _type = XrStructureType.XR_TYPE_SYSTEM_GET_INFO;
            _next = IntPtr.Zero;
            _formFactor = formFactor;
        }
    }
}
