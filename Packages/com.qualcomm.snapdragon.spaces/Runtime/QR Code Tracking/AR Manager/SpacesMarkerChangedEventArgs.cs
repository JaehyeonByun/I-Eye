/******************************************************************************
 * File: SpacesMarkerChangedEventArgs.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

namespace Qualcomm.Snapdragon.Spaces
{
    public readonly struct SpacesMarkersChangedEventArgs : IEquatable<SpacesMarkersChangedEventArgs>
    {
        public List<SpacesARMarker> added { get; }
        public List<SpacesARMarker> updated { get; }
        public List<SpacesARMarker> removed { get; }

        public SpacesMarkersChangedEventArgs(
            List<SpacesARMarker> added,
            List<SpacesARMarker> updated,
            List<SpacesARMarker> removed)
        {
            this.added = added;
            this.updated = updated;
            this.removed = removed;
        }

        public override int GetHashCode() => (added.GetHashCode() * 4999559)
            + (updated.GetHashCode() * 4999559)
            + (removed.GetHashCode() * 4999559);

        public override bool Equals(object obj) => obj is SpacesMarkersChangedEventArgs other && Equals(other);

        public bool Equals(SpacesMarkersChangedEventArgs other)
        {
            return (added == other.added) &&
                (updated == other.updated) &&
                (removed == other.removed);
        }
    }
}
