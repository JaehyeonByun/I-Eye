/****************************************************************
 * File: XrCameraActivationFrameConfigurationQCOM.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraActivationFrameConfigurationQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrCameraFrameFormatQCOM _format;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private string _resolutionName;

        private uint _fps;

        public XrCameraActivationFrameConfigurationQCOM(XrCameraFrameFormatQCOM format, string resolutionName, uint fps, bool stitchImages)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_ACTIVATION_FRAME_CONFIGURATION_INFO_QCOMX;
            _next = IntPtr.Zero;
            _format = format;
            _resolutionName = resolutionName;
            _fps = fps;
        }

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraActivationFrameConfigurationQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"Format:\t{_format}",
                $"ResolutionName:\t{_resolutionName}",
                $"Fps:\t{_fps}");
        }
    }
}
