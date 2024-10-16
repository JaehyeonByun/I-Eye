/******************************************************************************
 * File: XrImageTrackerCreateInfoQCOM.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrImageTrackerCreateInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _dataSetCount;

        // XrImageTrackerDataSetImageQCOM
        private IntPtr _dataSets;
        private uint _imageCount;

        public XrImageTrackerCreateInfoQCOM(IntPtr dataSets, uint dataSetCount, uint imageCount, IntPtr modeInfo)
        {
            _type = XrStructureType.XR_TYPE_IMAGE_TRACKER_CREATE_INFO_QCOM;
            _next = modeInfo;
            _dataSets = dataSets;
            _dataSetCount = dataSetCount;
            _imageCount = imageCount;
        }
    }
}
