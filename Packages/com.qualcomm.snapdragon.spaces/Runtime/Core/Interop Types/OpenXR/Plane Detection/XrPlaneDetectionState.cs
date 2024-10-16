/******************************************************************************
 * File: XrPlaneDetectionState.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

namespace Qualcomm.Snapdragon.Spaces
{
    internal enum XrPlaneDetectionState
    {
        XR_PLANE_DETECTION_STATE_NONE_QCOM = 0,
        XR_PLANE_DETECTION_STATE_INITIALIZING_QCOM = 1,
        XR_PLANE_DETECTION_STATE_TRACKING_QCOM = 2,
        XR_PLANE_DETECTION_STATE_ERROR_QCOM = 3,
        XR_PLANE_DETECTION_STATE_MAX_ENUM_QCOM = 0x7FFFFFFF
    }
}
