/******************************************************************************
 * File: XrSpatialAnchorCreateInfoMSFT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSpatialAnchorCreateInfoMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _space;
        private XrPosef _pose;
        private long _time;

        public XrSpatialAnchorCreateInfoMSFT(Pose pose, ulong space, long time)
        {
            _type = XrStructureType.XR_TYPE_SPATIAL_ANCHOR_CREATE_INFO_MSFT;
            _next = IntPtr.Zero;
            _space = space;
            _pose = new XrPosef(pose);
            _time = time;
        }
    }
}
