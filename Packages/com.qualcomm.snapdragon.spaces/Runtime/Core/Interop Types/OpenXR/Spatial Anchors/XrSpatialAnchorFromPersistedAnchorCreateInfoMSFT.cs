/******************************************************************************
 * File: XrSpatialAnchorFromPersistedAnchorCreateInfoMSFT.cs
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
    internal struct XrSpatialAnchorFromPersistedAnchorCreateInfoMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _spatialAnchorStore;
        private XrSpatialAnchorPersistenceNameMSFT _spatialAnchorPersistenceName;

        public XrSpatialAnchorFromPersistedAnchorCreateInfoMSFT(ulong spatialAnchorStore, string spatialAnchorName)
        {
            _type = XrStructureType.XR_TYPE_SPATIAL_ANCHOR_FROM_PERSISTED_ANCHOR_CREATE_INFO_MSFT;
            _next = IntPtr.Zero;
            _spatialAnchorStore = spatialAnchorStore;
            _spatialAnchorPersistenceName = new XrSpatialAnchorPersistenceNameMSFT(spatialAnchorName);
        }
    }
}
