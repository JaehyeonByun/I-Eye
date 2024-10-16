/******************************************************************************
 * File: XrImageTargetLocationQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrImageTargetLocationQCOM
    {
        private ulong _locationFlags;
        private XrPosef _pose;
        private ulong _imageTarget;
        public ulong LocationFlags => _locationFlags;
        public ulong ImageTargetHandle => _imageTarget;
        public XrPosef XrPose => _pose;
    }
}
