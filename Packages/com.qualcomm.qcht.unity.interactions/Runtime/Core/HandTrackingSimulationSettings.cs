// /******************************************************************************
//  * File: HandTrackingSimulationSettings.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using Unity.XR.CoreUtils;

namespace QCHT.Interactions.Core
{
    [ScriptableSettingsPath(Path)]
    public class HandTrackingSimulationSettings : ScriptableSettings<HandTrackingSimulationSettings>
    {
        private const string Path = "Assets/XR/Settings/";

        public bool enabled;
        public DataSource dataSource;

        public enum DataSource
        {
            SimulationSubsystem,
            //RecordingPlaybackSubsystem
        }
    }
}