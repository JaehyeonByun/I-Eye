/******************************************************************************
 * File: XrNewSceneComputeInfoMSFT.cs
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
    internal struct XrNewSceneComputeInfoMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _requestedFeatureCount;

        // XrSceneComputeFeatureMSFT
        private IntPtr _requestedFeatures;
        private XrSceneComputeConsistencyMSFT _consistency;
        private XrSceneBoundsMSFT _bounds;
    }
}
