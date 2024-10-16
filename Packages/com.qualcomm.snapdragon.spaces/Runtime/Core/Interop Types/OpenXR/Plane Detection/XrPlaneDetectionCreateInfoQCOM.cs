/******************************************************************************
 * File: XrPlaneDetectionCreateInfoQCOM.cs
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
    internal struct XrPlaneDetectionCreateInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrPlaneFilterQCOM _planeFilter;
        private uint _enableConvexHull;

        public XrPlaneDetectionCreateInfoQCOM(XrPlaneFilterQCOM planeFilter, bool enableConvexHull)
        {
            _type = XrStructureType.XR_TYPE_PLANE_DETECTION_CREATE_INFO_QCOM;
            _next = IntPtr.Zero;
            _planeFilter = planeFilter;
            _enableConvexHull = (uint)(enableConvexHull ? 1 : 0);
        }
    }
}
