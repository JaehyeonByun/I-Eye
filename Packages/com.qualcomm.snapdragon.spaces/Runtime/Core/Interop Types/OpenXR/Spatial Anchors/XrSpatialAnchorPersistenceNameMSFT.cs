/******************************************************************************
 * File: XrSpatialAnchorPersistenceNameMSFT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSpatialAnchorPersistenceNameMSFT
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;

        public XrSpatialAnchorPersistenceNameMSFT(string name)
        {
            Name = name;
        }
    }
}
