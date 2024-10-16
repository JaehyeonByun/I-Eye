/******************************************************************************
 * File: QRCodeTrackingSubsystem.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    internal class QrCodeTrackingSubsystem : XRQrCodeTrackingSubsystem
    {
        private class QrCodeTrackingProvider : Provider
        {
            private struct SpacesMarker
            {
                public XRTrackedMarker SubsystemMarker;
                public ulong Handle;
                public ulong Space;
                public bool IsMarkerDataAvailable;

                public SpacesMarker(ulong handle, ulong space)
                {
                    Handle = handle;
                    Space = space;
                    IsMarkerDataAvailable = false;
                    SubsystemMarker = new XRTrackedMarker(new TrackableId(handle, space), Pose.identity, TrackingState.None, IntPtr.Zero);
                }

                public void UpdatePoseAndTrackingState(Tuple<Pose, TrackingState> poseAndState)
                {
                    // NOTE(TD): Since pose and tracking state are read only we have to replace the actual XrTrackedMarker object.
                    if (poseAndState.Item2 == TrackingState.Tracking)
                    {
                        SubsystemMarker = new XRTrackedMarker(SubsystemMarker.trackableId, poseAndState.Item1, poseAndState.Item2, IntPtr.Zero);
                    }
                    // NOTE(GG): If the location flags are invalid, the location should not be overwritten.
                    else
                    {
                        SubsystemMarker = new XRTrackedMarker(SubsystemMarker.trackableId, SubsystemMarker.pose, poseAndState.Item2, IntPtr.Zero);
                    }
                }
            }

            private QrCodeTrackingFeature _feature;
            private ulong _trackerHandle;
            private bool _isTrackingMarkers;

            private XRMarkerDescriptor _markerDesc = XRMarkerDescriptor.DefaultDescriptor;

            private Dictionary<ulong /* Marker Handle */, SpacesMarker> _activeMarkersDict = new();

            private List<XRTrackedMarker> _addedMarkers = new();
            private List<XRTrackedMarker> _updatedMarkers = new();
            private List<TrackableId> _removedMarkers = new();

            public override void Start()
            {
                _feature = OpenXRSettings.Instance.GetFeature<QrCodeTrackingFeature>();

                if (_feature.TryCreateMarkerTracker(out _trackerHandle))
                {
                    _feature.TryStartMarkerDetection(_trackerHandle, _markerDesc.QrCodeVersionRange);
                }
            }

            public override void Stop()
            {
                foreach (var (key, value) in _activeMarkersDict)
                {
                    _feature.TryDestroyMarkerSpace(value.Space);
                }
                _activeMarkersDict.Clear();

                _feature.TryStopMarkerDetection(_trackerHandle);
                _feature.TryDestroyMarkerTracker(_trackerHandle);
            }

            public override void Destroy()
            {
            }

            public override TrackableChanges<XRTrackedMarker> GetChanges(XRTrackedMarker defaultMarker, Allocator allocator)
            {
                _addedMarkers.Clear();
                _updatedMarkers.Clear();
                _removedMarkers.Clear();

                if (_feature.TryAcquireMarkerUpdate(_trackerHandle, out ulong markerUpdate))
                {
                    if (_feature.TryGetMarkerUpdateInfo(markerUpdate, out uint updateInfoCount, out IntPtr updateInfosPtr))
                    {
                        for (int i = 0; i < updateInfoCount; i++)
                        {
                            XrMarkerUpdateInfoQCOM updateInfo = Marshal.PtrToStructure<XrMarkerUpdateInfoQCOM>(updateInfosPtr + Marshal.SizeOf<XrMarkerUpdateInfoQCOM>() * i);
                            SpacesMarker marker;

                            if (updateInfo.IsDetected)
                            {
                                if (!_activeMarkersDict.ContainsKey(updateInfo.Marker))
                                {
                                    if (_feature.TryCreateMarkerSpace(updateInfo.Marker, _markerDesc.TrackingMode, _markerDesc.Size, out ulong markerSpace))
                                    {
                                        marker = new SpacesMarker(updateInfo.Marker, markerSpace);
                                        _activeMarkersDict.Add(updateInfo.Marker, marker);
                                        _addedMarkers.Add(marker.SubsystemMarker);
                                    }
                                }
                            }
                            else // is lost
                            {
                                if (_activeMarkersDict.TryGetValue(updateInfo.Marker, out SpacesMarker markerToRemove))
                                {
                                    _feature.TryDestroyMarkerSpace(markerToRemove.Space);
                                    _activeMarkersDict.Remove(updateInfo.Marker);
                                    _removedMarkers.Add(markerToRemove.SubsystemMarker.trackableId);
                                }
                            }

                            if (updateInfo.IsMarkerDataAvailable)
                            {
                                if (_activeMarkersDict.TryGetValue(updateInfo.Marker, out marker))
                                {
                                    marker.IsMarkerDataAvailable = true;
                                    _activeMarkersDict[updateInfo.Marker] = marker;
                                }
                            }
                        }
                    }
                    _feature.TryReleaseMarkerUpdate(markerUpdate);
                }

                if (_isTrackingMarkers)
                {
                    foreach (var kv in _activeMarkersDict)
                    {
                        SpacesMarker marker = kv.Value;
                        if (_feature.TryGetMarkerPoseAndTrackingState(marker.Space, out var poseAndState))
                        {
                            marker.UpdatePoseAndTrackingState(poseAndState);
                            _updatedMarkers.Add(marker.SubsystemMarker);
                        }
                    }
                }

                return TrackableChanges<XRTrackedMarker>.CopyFrom(new NativeArray<XRTrackedMarker>(_addedMarkers.ToArray(), allocator),
                    new NativeArray<XRTrackedMarker>(_updatedMarkers.ToArray(), allocator),
                    new NativeArray<TrackableId>(_removedMarkers.ToArray(), allocator),
                    allocator);
            }

            public override void SetMarkerDescriptor(XRMarkerDescriptor markerDescriptor)
            {
                _markerDesc = markerDescriptor;
            }

            public override bool TryGetMarkerData(TrackableId trackableId, out string data)
            {
                data = string.Empty;

                foreach (var (key, value) in _activeMarkersDict)
                {
                    if (value.SubsystemMarker.trackableId == trackableId)
                    {
                        return value.IsMarkerDataAvailable && _feature.TryGetQrCodeStringData(key, out data);
                    }
                }

                return false;
            }

            public override bool TryGetMarkerSize(TrackableId trackableId, out Vector2 size)
            {
                size = new Vector2();
                foreach (var (key, value) in _activeMarkersDict)
                {
                    if (value.SubsystemMarker.trackableId == trackableId)
                    {
                        return value.IsMarkerDataAvailable && _feature.TryGetQrCodeSize(key, out size);
                    }
                }

                return false;
            }

            public override void EnableMarkerTracking(bool value)
            {
                _isTrackingMarkers = value;
            }
        }

        public const string ID = "Spaces-QRCodeSubsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRQrCodeTrackingSubsystemDescriptor.Create(new XRQrCodeTrackingSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(QrCodeTrackingProvider),
                subsystemTypeOverride = typeof(QrCodeTrackingSubsystem)
            });
        }
    }
}
