/****************************************************************
 * File: XrCameraFrameDataQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraFrameDataQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _handle;
        private XrCameraFrameFormatQCOM _format;
        private uint _frameNumber;
        private long _timestamp;

        public XrCameraFrameDataQCOM(IntPtr /* XrCameraSensorInfosQCOM */ sensorInfos, ulong handle = 0, XrCameraFrameFormatQCOM format = 0, uint frameNumber = 0, long timestamp = 0)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_FRAME_DATA_QCOMX;
            _next = sensorInfos;
            _handle = handle;
            _format = format;
            _frameNumber = frameNumber;
            _timestamp = timestamp;
        }

        public XrStructureType Type => _type;
        public IntPtr Next => _next;
        public ulong Handle => _handle;
        public XrCameraFrameFormatQCOM Format => _format;
        public uint FrameNumber => _frameNumber;
        public long Timestamp => _timestamp;

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraFrameDataQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"Handle:\t{_handle}",
                $"Format:\t{_format}",
                $"FrameNumber:\t{_frameNumber}",
                $"Timestamp:\t{_timestamp}");
        }
    }
}
