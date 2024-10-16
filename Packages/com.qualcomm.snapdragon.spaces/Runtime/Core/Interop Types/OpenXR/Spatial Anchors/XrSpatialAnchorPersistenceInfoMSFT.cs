/******************************************************************************
 * File: XrSpatialAnchorPersistenceInfoMSFT.cs
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
    internal struct XrSpatialAnchorPersistenceInfoMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrSpatialAnchorPersistenceNameMSFT _spatialAnchorPersistenceName;
        private ulong _spatialAnchor;

        public XrSpatialAnchorPersistenceInfoMSFT(string name, ulong spatialAnchor)
        {
            _type = XrStructureType.XR_TYPE_SPATIAL_ANCHOR_PERSISTENCE_INFO_MSFT;
            _next = IntPtr.Zero;
            _spatialAnchorPersistenceName = new XrSpatialAnchorPersistenceNameMSFT(name);
            _spatialAnchor = spatialAnchor;
        }
    }
}
