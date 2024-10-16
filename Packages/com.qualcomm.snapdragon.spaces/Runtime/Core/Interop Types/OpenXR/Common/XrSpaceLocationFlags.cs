/******************************************************************************
 * File: XrSpaceLocationFlags.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;

namespace Qualcomm.Snapdragon.Spaces
{
    [Flags]
    internal enum XrSpaceLocationFlags : byte
    {
        None = 0,
        XR_SPACE_LOCATION_ORIENTATION_VALID_BIT = 1,
        XR_SPACE_LOCATION_POSITION_VALID_BIT = 2,
        XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT = 4,
        XR_SPACE_LOCATION_POSITION_TRACKED_BIT = 8
    }
}
