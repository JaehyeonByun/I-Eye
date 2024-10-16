/****************************************************************
 * File: XrCameraSensorFacingFlagsQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;

namespace Qualcomm.Snapdragon.Spaces
{
    [Flags]
    internal enum XrCameraSensorFacingFlagsQCOM : byte
    {
        None = 0,
        XR_CAMERA_SENSOR_FACING_UP_BIT_QCOM = 1,
        XR_CAMERA_SENSOR_FACING_DOWN_BIT_QCOM = 2,
        XR_CAMERA_SENSOR_FACING_LEFT_BIT_QCOM = 4,
        XR_CAMERA_SENSOR_FACING_RIGHT_BIT_QCOM = 8,
        XR_CAMERA_SENSOR_FACING_FRONT_BIT_QCOM = 16,
        XR_CAMERA_SENSOR_FACING_BACK_BIT_QCOM = 32
    }
}
