/******************************************************************************
 * File: BaseRuntimeFeature.PerformanceSettings.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        /// <summary>
        /// Sets the performance settings level to be applied on the performance settings domain when settings the performance level hint.
        /// </summary>
        /// <seealso cref="PerfSettingsDomain"/>
        public enum PerfSettingsLevel
        {
            None = 0,

            /// <summary>
            /// Power savings to be prioritized. Consistent frame rendering and low latency are not needed.
            /// </summary>
            PowerSavings,

            /// <summary>
            /// Consistent frame rendering within a thermally sustainable range. The runtime is allowed to reduce power and increase latencies.
            /// </summary>
            SustainedLow,

            /// <summary>
            /// Consistent frame rendering is prioritized within a thermally sustainable range.
            /// </summary>
            SustainedHigh,

            /// <summary>
            /// The runtime is allowed to step up beyond the thermally sustainable range. This level is meant to be used for short-term durations (<30 seconds).
            /// </summary>
            Boost
        }

        /// <summary>
        /// Sets the performance settings domain to be used when settings the performance level hint.
        /// </summary>
        /// <seealso cref="PerfSettingsLevel"/>
        public enum PerfSettingsDomain
        {
            None = 0,

            /// <summary>
            /// The CPU processing domain
            /// </summary>
            CPU,

            /// <summary>
            /// The GPU processing domain
            /// </summary>
            GPU
        }

        XrPerfSettingsDomainEXT PerfSettingsDomainToXrDomain(PerfSettingsDomain domain)
        {
            switch (domain)
            {
                case PerfSettingsDomain.CPU: return XrPerfSettingsDomainEXT.XR_PERF_SETTINGS_DOMAIN_CPU_EXT;
                case PerfSettingsDomain.GPU: return XrPerfSettingsDomainEXT.XR_PERF_SETTINGS_DOMAIN_GPU_EXT;
            }

            Debug.LogWarning("Invalid PerfSettingsDomain: " + domain);
            return XrPerfSettingsDomainEXT.XR_PERF_SETTINGS_DOMAIN_MAX_ENUM_EXT;
        }

        XrPerfSettingsLevelEXT PerfSettingsLevelToXrLevel(PerfSettingsLevel level)
        {
            switch (level)
            {
                case PerfSettingsLevel.PowerSavings:    return XrPerfSettingsLevelEXT.XR_PERF_SETTINGS_LEVEL_POWER_SAVINGS_EXT;
                case PerfSettingsLevel.SustainedLow:    return XrPerfSettingsLevelEXT.XR_PERF_SETTINGS_LEVEL_SUSTAINED_LOW_EXT;
                case PerfSettingsLevel.SustainedHigh:   return XrPerfSettingsLevelEXT.XR_PERF_SETTINGS_LEVEL_SUSTAINED_HIGH_EXT;
                case PerfSettingsLevel.Boost:           return XrPerfSettingsLevelEXT.XR_PERF_SETTINGS_LEVEL_BOOST_EXT;
            }

            Debug.LogWarning("Invalid PerfSettingsLevel: " + level);
            return XrPerfSettingsLevelEXT.XR_PERF_SETTINGS_LEVEL_MAX_ENUM_EXT;
        }


        /// <summary>
        ///     Set the desired performance level to a domain.
        ///     This changes the performance of the CPU or the GPU dynamically, allowing for performance boost in demanding scenes and power saving in simpler ones.
        /// </summary>
        /// <param name="domain">The domain (GPU or CPU) to which the new performance level will be applied to.</param>
        /// <param name="level">The desired performance level.</param>
        public bool SetPerformanceLevelHint(PerfSettingsDomain domain, PerfSettingsLevel level)
        {
            if (_xrPerfSettingsSetPerformanceLevelEXT == null)
            {
                Debug.LogError("xrPerfSettingsSetPerformanceLevelEXT method not found!");
                return false;
            }

            XrPerfSettingsDomainEXT xrDomain = PerfSettingsDomainToXrDomain(domain);
            XrPerfSettingsLevelEXT xrLevel = PerfSettingsLevelToXrLevel(level);
            XrResult result = _xrPerfSettingsSetPerformanceLevelEXT(SessionHandle, xrDomain, xrLevel);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("xrPerfSettingsSetPerformanceLevelEXT failed: " + result);
                return false;
            }

            return true;
        }

        #region XR_EXT_performance_settings bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrPerfSettingsSetPerformanceLevelEXT(ulong session, XrPerfSettingsDomainEXT domain, XrPerfSettingsLevelEXT level);
        private static xrPerfSettingsSetPerformanceLevelEXT _xrPerfSettingsSetPerformanceLevelEXT;

        #endregion
    }
}
