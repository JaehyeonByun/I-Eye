/******************************************************************************
 * File: XrPlaneConvexHullBufferInfoQCOM.cs
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
    internal struct XrPlaneConvexHullBufferInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _convexHullBufferId;

        public XrPlaneConvexHullBufferInfoQCOM(ulong convexHullBufferId)
        {
            _type = XrStructureType.XR_TYPE_PLANE_CONVEX_HULL_BUFFER_INFO_QCOM;
            _next = IntPtr.Zero;
            _convexHullBufferId = convexHullBufferId;
        }
    }
}
