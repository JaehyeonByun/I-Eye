/******************************************************************************
 * File: XrFrameState.cs
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
    internal struct XrFrameState
    {
        private XrStructureType _type;
        private IntPtr _next;
        private long _predictedDisplayTime;
        private long _predictedDisplayPeriod;
        private uint _shouldRender;
        public long PredictedDisplayTime => _predictedDisplayTime;
    }
}
