/******************************************************************************
 * File: XrPlaneLocationsQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrPlaneLocationsQCOM
    {
        private readonly XrStructureType _type;
        private readonly IntPtr _next;
        private readonly uint _planeCapacityInput;

        public XrPlaneLocationsQCOM(IntPtr planeCountOutput)
        {
            _type = XrStructureType.XR_TYPE_PLANE_LOCATIONS_QCOM;
            _next = IntPtr.Zero;
            _planeCapacityInput = 0;
            PlaneCountOutput = planeCountOutput;
            PlaneLocations = IntPtr.Zero;
        }

        public XrPlaneLocationsQCOM(uint planeCapacityInput, IntPtr planeCountOutput, IntPtr planeLocations)
        {
            _type = XrStructureType.XR_TYPE_PLANE_LOCATIONS_QCOM;
            _next = IntPtr.Zero;
            _planeCapacityInput = planeCapacityInput;
            PlaneCountOutput = planeCountOutput;
            PlaneLocations = planeLocations;
        }

        // uint
        public IntPtr PlaneCountOutput { get; }

        // XrPlaneLocationQCOM
        public IntPtr PlaneLocations { get; }
    }
}
