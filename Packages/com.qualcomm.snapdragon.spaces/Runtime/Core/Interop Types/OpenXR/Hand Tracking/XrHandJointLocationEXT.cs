/******************************************************************************
 * File: XrHandJointLocationEXT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete]
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrHandJointLocationEXT
    {
        private long _locationFlags;
        private XrPosef _pose;
        private float _radius;
        public XrPosef pose => _pose;
    }
}
