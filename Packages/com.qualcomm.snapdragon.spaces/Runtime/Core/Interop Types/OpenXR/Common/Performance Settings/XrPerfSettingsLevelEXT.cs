/******************************************************************************
 * File: XrPerfSettingsLevelEXT.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

namespace Qualcomm.Snapdragon.Spaces
{
    internal enum XrPerfSettingsLevelEXT
    {
        XR_PERF_SETTINGS_LEVEL_POWER_SAVINGS_EXT = 0,
        XR_PERF_SETTINGS_LEVEL_SUSTAINED_LOW_EXT = 25,
        XR_PERF_SETTINGS_LEVEL_SUSTAINED_HIGH_EXT = 50,
        XR_PERF_SETTINGS_LEVEL_BOOST_EXT = 75,
        XR_PERF_SETTINGS_LEVEL_MAX_ENUM_EXT = 0x7FFFFFFF
    }
}
