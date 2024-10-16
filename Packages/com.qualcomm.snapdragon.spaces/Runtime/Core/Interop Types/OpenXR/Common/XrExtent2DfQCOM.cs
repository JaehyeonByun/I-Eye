/******************************************************************************
 * File: XrExtent2DfQCOM.cs
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
    internal struct XrExtent2DfQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private float _min;
        private float _max;

        public float Min => _min;
        public float Max => _max;
    }
}
