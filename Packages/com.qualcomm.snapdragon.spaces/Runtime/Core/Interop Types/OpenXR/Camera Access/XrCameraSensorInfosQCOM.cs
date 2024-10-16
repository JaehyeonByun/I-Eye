/****************************************************************
 * File: XrCameraSensorInfosQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraSensorInfosQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _baseSpace;
        private uint _sensorPropertyCount;

        // XrCameraSensorPropertiesQCOM[]
        private IntPtr _sensorProperties;

        public XrCameraSensorInfosQCOM(ulong baseSpace, uint SensorPropertyCount, IntPtr /* XrCameraSensorPropertiesQCOM[] */ SensorProperties)
        {
            _type = XrStructureType.XR_TYPE_CAMERA_SENSOR_INFOS_QCOMX;
            _next = IntPtr.Zero;
            _baseSpace = baseSpace;
            _sensorPropertyCount = SensorPropertyCount;
            _sensorProperties = SensorProperties;
        }

        public override string ToString()
        {
            return String.Join("\n",
                "[XrCameraSensorInfosQCOM]",
                $"Type:\t{_type}",
                $"Next:\t{_next}",
                $"BaseSpace:\t{_baseSpace}",
                $"SensorPropertyCount:\t{_sensorPropertyCount}",
                $"SensorProperties:\t{_sensorProperties}");
        }
    }
}
