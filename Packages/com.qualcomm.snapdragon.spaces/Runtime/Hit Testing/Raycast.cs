/******************************************************************************
 * File: Raycast.cs
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
    internal class Raycast
    {
        public XRRaycast SubsystemRaycast;
        public Ray Ray;

        public Raycast(ulong raycastHandle, Ray ray)
        {
            SubsystemRaycast = new XRRaycast(new TrackableId(raycastHandle, 42), Pose.identity, TrackingState.None, IntPtr.Zero, Mathf.Infinity, TrackableId.invalidId);
            Ray = ray;
        }

        public ulong RaycastHandle => SubsystemRaycast.trackableId.subId1;

        public void UpdateSubsystemRaycastHitAndTrackingState(XRRaycastHit hit, TrackingState trackingState)
        {
            // Since the hitTrackableId is read only we have to replace the actual XRRaycast object.
            SubsystemRaycast = new XRRaycast(SubsystemRaycast.trackableId, SubsystemRaycast.pose, trackingState, SubsystemRaycast.nativePtr, Mathf.Infinity, hit.trackableId);
        }
    }
}
