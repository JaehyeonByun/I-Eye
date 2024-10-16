/****************************************************************
 * File: XrCameraFrameHardwareBufferQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraFrameHardwareBufferQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        // AHardwareBuffer*
        private IntPtr _buffer;

        public XrCameraFrameHardwareBufferQCOM(IntPtr buffer)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_FRAME_HARDWARE_BUFFER_QCOMX;
            _next = IntPtr.Zero;
            _buffer = buffer;
        }

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraFrameHardwareBufferQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"Buffer:\t{_buffer}");
        }
    }
}
