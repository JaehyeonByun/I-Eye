/****************************************************************
 * File: XrCameraInfoQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        private string _cameraSet;

        private XrCameraTypeQCOM _cameraType;
        private uint _sensorCount;

        public XrCameraInfoQCOM(string cameraSet, XrCameraTypeQCOM cameraType, uint sensorCount)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_INFO_QCOMX;
            _next = IntPtr.Zero;
            _cameraSet = cameraSet;
            _cameraType = cameraType;
            _sensorCount = sensorCount;
        }

        public string CameraSet => _cameraSet;
        public XrCameraTypeQCOM CameraType => _cameraType;
        public uint SensorCount => _sensorCount;

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraInfoQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"CameraSet:\t{_cameraSet}",
                $"CameraType:\t{_cameraType}",
                $"SensorCount:\t{_sensorCount}");
        }
    }
}
