/******************************************************************************
 * File: XRQrCodeTrackingSubsystem.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    public class XRQrCodeTrackingSubsystem : TrackingSubsystem<XRTrackedMarker, XRQrCodeTrackingSubsystem, XRQrCodeTrackingSubsystemDescriptor, XRQrCodeTrackingSubsystem.Provider>
    {
        public abstract class Provider : SubsystemProvider<XRQrCodeTrackingSubsystem>
        {
            public abstract TrackableChanges<XRTrackedMarker> GetChanges(XRTrackedMarker defaultMarker, Allocator allocator);
            public abstract void SetMarkerDescriptor(XRMarkerDescriptor markerDesc);
            public abstract bool TryGetMarkerData(TrackableId TrackableId, out string data);
            public abstract bool TryGetMarkerSize(TrackableId trackableId, out Vector2 size);
            public abstract void EnableMarkerTracking(bool value);
        }

        public override TrackableChanges<XRTrackedMarker> GetChanges(Allocator allocator)
        {
            return provider.GetChanges(XRTrackedMarker.defaultValue, allocator);
        }

        public void SetMarkerDescriptor(XRMarkerDescriptor markerDesc)
        {
            provider.SetMarkerDescriptor(markerDesc);
        }

        public bool TryGetMarkerData(TrackableId trackableId, out string data)
        {
            return provider.TryGetMarkerData(trackableId, out data);
        }

        public bool TryGetMarkerSize(TrackableId trackableId, out Vector2 size)
        {
            return provider.TryGetMarkerSize(trackableId, out size);
        }

        public void EnableMarkerTracking(bool value)
        {
            provider.EnableMarkerTracking(value);
        }
    }
}
