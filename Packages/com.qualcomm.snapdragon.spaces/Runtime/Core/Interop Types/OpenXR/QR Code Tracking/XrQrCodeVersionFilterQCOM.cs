/******************************************************************************
 * File: XrQrCodeVersionFilterQCOM.cs
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
    internal struct XrQrCodeVersionFilterQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _rangeCount;
        private IntPtr /* XrQRCodeVersionRangeQCOMX* */ _ranges;

        public XrQrCodeVersionFilterQCOM(uint rangeCount, IntPtr ranges)
        {
            _type = XrStructureType.XR_TYPE_QR_CODE_VERSION_FILTER_QCOMX;
            _next = IntPtr.Zero;
            _rangeCount = rangeCount;
            _ranges = ranges;
        }
    }
}
