/******************************************************************************
 * File: SpacesARMarker.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Qualcomm.Snapdragon.Spaces
{
    [DisallowMultipleComponent]
    public sealed class SpacesARMarker : ARTrackable<XRTrackedMarker, SpacesARMarker>
    {
        public bool IsMarkerDataAvailable { get; private set; }
        public string Data => IsMarkerDataAvailable ? _data : string.Empty;
        public Vector2 Size => IsMarkerDataAvailable ? _size : new Vector2();

        internal void TryGetMarkerData(XRQrCodeTrackingSubsystem subsystem)
        {
            IsMarkerDataAvailable = subsystem.TryGetMarkerData(sessionRelativeData.trackableId, out _data);
        }

        internal void TryGetMarkerSize(XRQrCodeTrackingSubsystem subsystem)
        {
            subsystem.TryGetMarkerSize(sessionRelativeData.trackableId, out _size);
        }

        private string _data;
        private Vector2 _size;
    }
}
