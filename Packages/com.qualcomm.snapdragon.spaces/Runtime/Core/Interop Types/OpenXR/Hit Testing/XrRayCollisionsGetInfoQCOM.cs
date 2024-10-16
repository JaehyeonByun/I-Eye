/****************************************************************
 * Copyright (c) 2022 Qualcomm Technologies, Inc.
 * All Rights Reserved.
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrRayCollisionsGetInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _baseSpace;

        public XrRayCollisionsGetInfoQCOM(ulong baseSpace)
        {
            _type = XrStructureType.XR_TYPE_RAY_COLLISIONS_GET_INFO_QCOM;
            _next = IntPtr.Zero;
            _baseSpace = baseSpace;
        }
    }
}
