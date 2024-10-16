/******************************************************************************
 * File: XrMarkerUpdateInfoQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrMarkerUpdateInfoQCOM
    {
        private ulong _marker;
        private long _time;
        private uint /*XrBool32*/ _isDetected;              // The only valid values are XR_TRUE and XR_FALSE
        private uint /*XrBool32*/ _isMarkerDataAvailable;   // The only valid values are XR_TRUE and XR_FALSE

        public ulong Marker => _marker;
        public bool IsDetected => _isDetected > 0;
        public bool IsMarkerDataAvailable => _isMarkerDataAvailable > 0;

        public override string ToString()
        {
            return "Marker: " + _marker
                + " -- IsDetected: " + IsDetected
                + " -- IsMarkerDataAvailable: " + IsMarkerDataAvailable;
        }
    }
}
