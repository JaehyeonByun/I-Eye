/****************************************************************
 * File: XrCameraSensorIntrinsicsQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraSensorIntrinsicsQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrVector2f _principalPoint;
        private XrVector2f _focalLength;

        // XR_MAX_CAMERA_RADIAL_DISTORSION_PARAMS_LENGTH_QCOMX == 6
        // XR_MAX_CAMERA_TANGENTIAL_DISTORSION_PARAMS_LENGTH_QCOMX == 2
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        private float[] _radialDistortion;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        private float[] _tangentialDistortion;

        private XrCameraDistortionModelQCOM _distortionModel;

        public XrCameraSensorIntrinsicsQCOM(XrVector2f principalPoint, XrVector2f focalLength, float[] radialDistortion, float[] tangentialDistortion, XrCameraDistortionModelQCOM distortionModel)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_SENSOR_INTRINSICS_QCOMX;
            _next = IntPtr.Zero;
            _principalPoint = principalPoint;
            _focalLength = focalLength;
            _radialDistortion = radialDistortion;
            _tangentialDistortion = tangentialDistortion;
            _distortionModel = distortionModel;
        }

        public XrVector2f PrincipalPoint => _principalPoint;
        public XrVector2f FocalLength => _focalLength;
        public float[] RadialDistortion => _radialDistortion;
        public float[] TangentialDistortion => _tangentialDistortion;
        public XrCameraDistortionModelQCOM DistortionModel => _distortionModel;

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraSensorIntrinsicsQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"PrincipalPoint:\t{_principalPoint}",
                $"FocalLength:\t{_focalLength}",
                $"RadialDistortion:\t{_radialDistortion}",
                $"TangentialDistortion:\t{_tangentialDistortion}",
                $"DistortionModel:\t{_distortionModel}");
        }
    }
}
