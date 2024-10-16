/****************************************************************
 * File: XrCameraFramePlaneQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraFramePlaneQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrCameraFramePlaneTypeQCOM _planeType;
        private uint _offset;
        private uint _stride;

        public XrCameraFramePlaneQCOM(XrCameraFramePlaneTypeQCOM planeType, uint offset, uint stride)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_FRAME_PLANE_QCOMX;
            _next = IntPtr.Zero;
            _planeType = planeType;
            _offset = offset;
            _stride = stride;
        }

        public XrCameraFramePlaneTypeQCOM PlaneType => _planeType;
        public uint Offset => _offset;
        public uint Stride => _stride;

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraFramePlaneQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"PlaneType:\t{_planeType}",
                $"Offset:\t{_offset}",
                $"Stride:\t{_stride}");
        }
    }
}
