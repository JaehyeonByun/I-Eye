/******************************************************************************
 * File: XRMarkerDescriptor.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public struct XRMarkerDescriptor : IEquatable<XRMarkerDescriptor>
    {
        public MarkerTrackingMode TrackingMode => _trackingMode;
        public Vector2 Size => _size;
        public Tuple<byte, byte> QrCodeVersionRange => _qrCodeVersionRange;

        private MarkerTrackingMode _trackingMode;
        private Vector2 _size;
        private Tuple<byte, byte> _qrCodeVersionRange;

        public XRMarkerDescriptor(
            MarkerTrackingMode trackingMode,
            Vector2 size,
            Tuple<byte, byte> qrCodeVersionRange)
        {
            _trackingMode = trackingMode;
            _size = size;
            _qrCodeVersionRange = qrCodeVersionRange;
        }

        public static XRMarkerDescriptor DefaultDescriptor { get; } = new()
        {
            _trackingMode = MarkerTrackingMode.Dynamic,
            _size = new Vector2(0.1f, 0.1f),
            _qrCodeVersionRange = new(1, 10)
        };

        public bool Equals(XRMarkerDescriptor other)
        {
            return this._trackingMode == other._trackingMode &&
                this._size == other._size &&
                Equals(this._qrCodeVersionRange, other._qrCodeVersionRange);
        }

        public override int GetHashCode()
        {
            return ((int)_trackingMode).GetHashCode()
                + _size.GetHashCode()
                + _qrCodeVersionRange.GetHashCode();
        }
    }
}
