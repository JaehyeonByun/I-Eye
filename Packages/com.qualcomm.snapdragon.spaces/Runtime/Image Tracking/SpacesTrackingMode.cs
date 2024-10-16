/******************************************************************************
 * File: SpacesImageTrackingMode.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.ComponentModel;

namespace Qualcomm.Snapdragon.Spaces
{
    public enum SpacesImageTrackingMode
    {
        [Description("Dynamic mode updates the position of tracked images frequently, and works on moving and static targets. If the tracked image cannot be found, no location or pose is reported.")]
        DYNAMIC = 0,

        [Description("Static mode is useful for tracking images that are known to be static. Images tracked in this mode are fixed in position when first detected, and never updated. This leads to less power consumption and greater performance.")]
        STATIC = 1,

        [Description("Adaptive mode will periodically update the position of static images if they have moved slightly. This finds a balance between power consumption and accuracy for static images..")]
        ADAPTIVE = 2,

        [Description("Invalid tracking mode.")]
        INVALID = 0x7FFFFFFF
    }
}
