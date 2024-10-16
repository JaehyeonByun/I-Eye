/******************************************************************************
 * File: Hand.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html", false)]
    internal class Hand
    {
        public Pose[] Joints;
        public int Gesture;
        public float GestureRatio;
        public float FlipRatio;
        private XRTrackedHand _trackedHand;

        public Hand(XRTrackedHand trackedHand, bool isLeft)
        {
            _trackedHand = trackedHand;
            IsLeft = isLeft;
        }

        public XRTrackedHand TrackedHand => _trackedHand;
        public bool IsLeft { get; }

        public void UpdateTrackedHandPoseAndTrackingState(Pose pose, TrackingState state)
        {
            _trackedHand.UpdatePoseAndTrackedState(pose, state);
        }
    }
}
