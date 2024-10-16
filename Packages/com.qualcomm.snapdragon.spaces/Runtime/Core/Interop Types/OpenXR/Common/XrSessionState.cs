/******************************************************************************
 * File: XrSessionState.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

namespace Qualcomm.Snapdragon.Spaces
{
    internal enum XrSessionState
    {
        XR_SESSION_STATE_UNKNOWN = 0,
        XR_SESSION_STATE_IDLE = 1,
        XR_SESSION_STATE_READY = 2,
        XR_SESSION_STATE_SYNCHRONIZED = 3,
        XR_SESSION_STATE_VISIBLE = 4,
        XR_SESSION_STATE_FOCUSED = 5,
        XR_SESSION_STATE_STOPPING = 6,
        XR_SESSION_STATE_LOSS_PENDING = 7,
        XR_SESSION_STATE_EXITING = 8,
        XR_SESSION_STATE_MAX_ENUM = 0x7FFFFFFF
    }
}
