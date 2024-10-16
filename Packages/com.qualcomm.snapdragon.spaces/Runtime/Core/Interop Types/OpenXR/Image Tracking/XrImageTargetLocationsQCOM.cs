/******************************************************************************
 * File: XrImageTargetLocationsQCOM.cs
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
    internal struct XrImageTargetLocationsQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _isActive;
        private uint _imageCount;

        // XrImageTargetLocationQCOM
        private IntPtr _imageTargetLocations;

        public XrImageTargetLocationsQCOM(uint imageCount, IntPtr imageTargetLocationsPtr)
        {
            _type = XrStructureType.XR_TYPE_IMAGE_TARGET_LOCATIONS_QCOM;
            _next = IntPtr.Zero;
            _isActive = 1;
            _imageCount = imageCount;
            _imageTargetLocations = imageTargetLocationsPtr;
        }

        public int Count => (int)_imageCount;
        public IntPtr ImageTargetLocationsPtr => _imageTargetLocations;
    }
}
