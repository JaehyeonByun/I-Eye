/****************************************************************
 * File: XrCameraFrameBuffersQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraFrameBuffersQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _frameBufferCount;

        // XrCameraFrameBufferQCOM[]
        private IntPtr _frameBuffers;

        public XrCameraFrameBuffersQCOM(IntPtr next, uint frameBufferCount, IntPtr frameBuffersPointer)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_FRAME_BUFFERS_QCOMX;
            _next = next;
            _frameBufferCount = frameBufferCount;
            _frameBuffers = frameBuffersPointer;
        }

        public uint FrameBufferCount => _frameBufferCount;
        public IntPtr FrameBuffers => _frameBuffers;

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraFrameBuffersQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"FrameBufferCount:\t{_frameBufferCount}",
                $"FrameBuffers:\t{_frameBuffers}");
        }
    }
}
