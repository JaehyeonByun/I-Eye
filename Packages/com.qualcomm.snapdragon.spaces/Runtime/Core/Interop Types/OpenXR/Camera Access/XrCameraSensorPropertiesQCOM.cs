/****************************************************************
 * File: XrCameraSensorPropertiesQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraSensorPropertiesQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrCameraSensorIntrinsicsQCOM _intrinsics;
        private XrPosef _extrinsic;
        private XrOffset2Di _imageOffset;
        private XrExtent2Di _imageDimensions;
        private XrCameraSensorFacingFlagsQCOM _facing;
        private ulong _rollingShutterLineTime;

        public XrCameraSensorPropertiesQCOM(XrCameraSensorIntrinsicsQCOM intrinsics, XrPosef extrinsic, XrOffset2Di imageOffset, XrExtent2Di imageDimensions, XrCameraSensorFacingFlagsQCOM facing, ulong rollingShutterLineTime)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_SENSOR_PROPERTIES_QCOMX;
            _next = IntPtr.Zero;
            _intrinsics = intrinsics;
            _extrinsic = extrinsic;
            _imageOffset = imageOffset;
            _imageDimensions = imageDimensions;
            _facing = facing;
            _rollingShutterLineTime = rollingShutterLineTime;
        }

        public XrCameraSensorIntrinsicsQCOM Intrinsics => _intrinsics;
        public XrPosef Extrinsic => _extrinsic;
        public XrOffset2Di ImageOffset => _imageOffset;
        public XrExtent2Di ImageDimensions => _imageDimensions;
        public XrCameraSensorFacingFlagsQCOM Facing => _facing;
        public ulong RollingShutterLineTime => _rollingShutterLineTime;

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraSensorPropertiesQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"Intrinsics:\t{_intrinsics}",
                $"Extrinsic:\t{_extrinsic}",
                $"ImageOffset:\t{_imageOffset}",
                $"ImageDimensions:\t{_imageDimensions}",
                $"Facing:\t{_facing}",
                $"RollingShutterLineTime:\t{_rollingShutterLineTime}");
        }
    }
}
