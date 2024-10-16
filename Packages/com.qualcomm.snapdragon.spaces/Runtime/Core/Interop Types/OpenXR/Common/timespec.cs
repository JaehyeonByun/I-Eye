/******************************************************************************
 * File: timespec.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Timespec
    {
        private long _sec;
        private long _nsec;

        public long Seconds => _sec;
        public long Nanoseconds => _nsec;

    }
}
