/******************************************************************************
 * File: XrSceneBoundsMSFT.cs
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
    internal struct XrSceneBoundsMSFT
    {
        private ulong _space;
        private long _time;
        private uint _sphereCount;

        // XrSphereBoundMSFT
        private IntPtr _spheres;
        private uint _boxCount;

        // XrSceneOrientedBoxBoundMSFT
        private IntPtr _boxes;
        private uint _frustumCount;

        // XrSceneFrustumBoundMSFT
        private IntPtr _frustums;
    }
}
