/******************************************************************************
 * File: XrSpatialAnchorSpaceCreateInfoMSFT.cs
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
    internal struct XrSpatialAnchorSpaceCreateInfoMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _anchor;
        private XrPosef _poseInAnchorSpace;

        public XrSpatialAnchorSpaceCreateInfoMSFT(ulong anchorHandle)
        {
            _type = XrStructureType.XR_TYPE_SPATIAL_ANCHOR_SPACE_CREATE_INFO_MSFT;
            _next = IntPtr.Zero;
            _anchor = anchorHandle;
            _poseInAnchorSpace = new XrPosef(XrQuaternionf.identity, XrVector3f.zero);
        }
    }
}
