// /******************************************************************************
//  * File: XRHandSimulationHandPosesSettings.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Hands;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    [ScriptableSettingsPath(Path)]
    public class XRHandSimulationHandPosesSettings : ScriptableSettings<XRHandSimulationHandPosesSettings>
    {
        private const string Path = "Packages/com.qualcomm.qcht.unity.interactions/Settings";
        
        public HandPose leftOpenHand;
        public HandPose leftPinchHand;
        public HandPose leftGrabHand; 
        
        [Space]
        
        public HandPose rightOpenHand;
        public HandPose rightPinchHand;
        public HandPose rightGrabHand;
    }
}