/******************************************************************************
 * File: XRTrackedMarker.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    public struct XRTrackedMarker : ITrackable, IEquatable<XRTrackedMarker>
    {
        public TrackableId trackableId => _id;
        public Pose pose => _pose;
        public TrackingState trackingState => _trackingState;
        public IntPtr nativePtr => _nativePtr;

        private TrackableId _id;
        private Pose _pose;
        private TrackingState _trackingState;
        private IntPtr _nativePtr;

        public XRTrackedMarker(
            TrackableId trackableId,
            Pose pose,
            TrackingState trackingState,
            IntPtr nativePtr)
        {
            _id = trackableId;
            _pose = pose;
            _trackingState = trackingState;
            _nativePtr = nativePtr;
        }

        public static XRTrackedMarker defaultValue { get; } = new XRTrackedMarker
        {
            _id = TrackableId.invalidId,
            _pose = Pose.identity,
            _trackingState = TrackingState.None,
            _nativePtr = IntPtr.Zero
        };

        public override int GetHashCode()
        {
            var hashCode = _id.GetHashCode();
            hashCode = hashCode * 486187739 + _pose.GetHashCode();
            hashCode = hashCode * 486187739 + ((int)_trackingState).GetHashCode();
            return hashCode;
        }

        public bool Equals(XRTrackedMarker other)
        {
            return
                _id.Equals(other._id) &&
                _pose.Equals(other._pose) &&
                _trackingState == other._trackingState;
        }

        public override bool Equals(object obj)
        {
            return obj is XRTrackedMarker other && Equals(other);
        }

        public static bool operator ==(XRTrackedMarker lhs, XRTrackedMarker rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(XRTrackedMarker lhs, XRTrackedMarker rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
