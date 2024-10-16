/******************************************************************************
 * File: XrSceneFrustumBoundMSFT.cs
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
    internal struct XrSceneFrustumBoundMSFT
    {
        private XrPosef _pose;

        // XrFovf
        private IntPtr _fov;
        private float _farDistance;
    }
}
