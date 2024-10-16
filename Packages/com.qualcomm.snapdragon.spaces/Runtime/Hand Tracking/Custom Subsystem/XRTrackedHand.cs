/******************************************************************************
 * File: XRTrackedHand.cs
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
    public struct XRTrackedHand : ITrackable,
        IEquatable<XRTrackedHand>
    {
        private TrackableId _id;
        public TrackableId trackableId => _id;
        public IntPtr nativePtr => IntPtr.Zero;
        public Pose pose { get; private set; }
        public TrackingState trackingState { get; private set; }
        public static XRTrackedHand defaultValue { get; } = new XRTrackedHand(TrackableId.invalidId);

        public XRTrackedHand(TrackableId trackableId, TrackingState trackingState = TrackingState.None)
        {
            _id = trackableId;
            pose = Pose.identity;
            this.trackingState = trackingState;
        }

        internal void UpdatePoseAndTrackedState(Pose pose, TrackingState trackingState)
        {
            this.pose = pose;
            this.trackingState = trackingState;
        }

        public override int GetHashCode()
        {
            return (_id.GetHashCode() * 4999559) + ((int)trackingState).GetHashCode();
        }

        public bool Equals(XRTrackedHand other)
        {
            return _id.Equals(other._id) && trackingState == other.trackingState;
        }

        public override bool Equals(object obj)
        {
            return obj is XRTrackedHand && Equals((XRTrackedHand)obj);
        }

        public static bool operator ==(XRTrackedHand lhs, XRTrackedHand rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(XRTrackedHand lhs, XRTrackedHand rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
