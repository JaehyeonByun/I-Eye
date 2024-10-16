/******************************************************************************
 * File: SpacesQrCodeManager.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

#if UNITY_2022_2_OR_NEWER
using Unity.XR.CoreUtils;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    [Serializable]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(int.MinValue + 1)]
#if UNITY_2022_2_OR_NEWER
    [RequireComponent(typeof(XROrigin))]
#else
    [RequireComponent(typeof(ARSessionOrigin))]
#endif
    public sealed class SpacesQrCodeManager : ARTrackableManager<XRQrCodeTrackingSubsystem, XRQrCodeTrackingSubsystemDescriptor, XRQrCodeTrackingSubsystem.Provider, XRTrackedMarker, SpacesARMarker>
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each detected marker.")]
        private GameObject _markerPrefab;

        public GameObject markerPrefab
        {
            get => _markerPrefab;
            set => _markerPrefab = value;
        }

        [SerializeField]
        private bool _markerTracking;

        private byte _minQrVersionSupported = 1;
        private byte _maxQrVersionSupported = 10;

        private MarkerTrackingMode _markerTrackingMode = MarkerTrackingMode.Dynamic;

        [Tooltip("The marker tracking mode to be used when tracking markers.")]
        public MarkerTrackingMode MarkerTrackingMode
        {
            get => _markerTrackingMode;
            set {
                _markerTrackingMode = value;
                UpdateMarkerDescriptor();
            }
        }


        [Tooltip("Defines the width and height (in meters) of the physical markers that will be detected.")]
        public Vector2 markerSize = new(0.1f, 0.1f);

        [Tooltip("Defines the inclusive minimum version. Should be smaller than 'maxQrVersion'.")]
        public byte minQrVersion = 1;

        [Tooltip("Defines the inclusive maximum version. Should be bigger than 'minQrVersion'.")]
        public byte maxQrVersion = 10;

        public bool MarkerTracking {
            get => _markerTracking;
            set {
                subsystem.EnableMarkerTracking(value);
                _markerTracking = value;
            }
        }

        public event Action<SpacesMarkersChangedEventArgs> markersChanged;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateMarkerDescriptor();
        }

        protected override GameObject GetPrefab() => _markerPrefab;

        protected override string gameObjectName => nameof(SpacesARMarker);

        protected override void OnAfterSetSessionRelativeData(
            SpacesARMarker marker,
            XRTrackedMarker sessionRelativeData)
        {
            if (!marker.IsMarkerDataAvailable)
            {
                marker.TryGetMarkerData(subsystem);
                marker.TryGetMarkerSize(subsystem);
            }
        }

        protected override void OnTrackablesChanged(
            List<SpacesARMarker> added,
            List<SpacesARMarker> updated,
            List<SpacesARMarker> removed)
        {
            markersChanged?.Invoke(new SpacesMarkersChangedEventArgs(added, updated, removed));
        }

        private void OnValidate()
        {
            minQrVersion = Math.Clamp(minQrVersion, _minQrVersionSupported, _maxQrVersionSupported);
            maxQrVersion = Math.Clamp(maxQrVersion, _minQrVersionSupported, _maxQrVersionSupported);
        }

        private void UpdateMarkerDescriptor()
        {
            minQrVersion = Math.Clamp(minQrVersion, _minQrVersionSupported, _maxQrVersionSupported);
            maxQrVersion = Math.Clamp(maxQrVersion, _minQrVersionSupported, _maxQrVersionSupported);
            subsystem.SetMarkerDescriptor(new XRMarkerDescriptor(
                _markerTrackingMode, markerSize,
                new Tuple<byte, byte>(
                    minQrVersion,
                    maxQrVersion)));
            subsystem.EnableMarkerTracking(_markerTracking);
        }
    }
}
