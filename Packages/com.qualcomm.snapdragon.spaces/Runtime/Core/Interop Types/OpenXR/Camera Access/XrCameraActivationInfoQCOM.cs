/****************************************************************
 * File: XrCameraActivationInfoQCOM.cs
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
    internal struct XrCameraActivationInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;

        [MarshalAs(UnmanagedType.LPTStr)]
        private string _cameraSet;

        public XrCameraActivationInfoQCOM(string cameraSet)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_ACTIVATION_INFO_QCOMX;
            _next = IntPtr.Zero;
            _cameraSet = cameraSet;
        }

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraActivationInfoQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"CameraSet:\t{_cameraSet}");
        }
    }
}
