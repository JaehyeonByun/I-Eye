/******************************************************************************
 * File: XrSceneComponentLocationsMSFT.cs
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
    internal struct XrSceneComponentLocationsMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _locationCount;

        // XrSceneComponentLocationMSFT[]
        private IntPtr _locations;
    }
}
