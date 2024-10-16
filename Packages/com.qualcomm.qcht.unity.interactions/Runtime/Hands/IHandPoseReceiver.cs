// /******************************************************************************
//  * File: IHandPoseOverrider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Hands
{
    public interface IHandPoseReceiver
    {
        public Pose? RootPoseOverride { get; }
        public HandData? HandPoseOverride { get; set; }
        public HandMask? HandPoseMask { get; set; }
    }
}