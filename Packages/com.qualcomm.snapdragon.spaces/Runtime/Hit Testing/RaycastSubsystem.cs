/******************************************************************************
 * File: RaycastSubsystem.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public class RaycastSubsystem : XRRaycastSubsystem
    {
        private class SpacesProvider : Provider
        {
            private HitTestingFeature _underlyingFeature;
            private List<Raycast> _activeRaycasts;
            private List<XRRaycast> _xrRaycastsToAdd;
            private List<TrackableId> _trackablesToRemove;
            private ulong _tempRaycastHandle;

            public override void Start()
            {
                _underlyingFeature = OpenXRSettings.Instance.GetFeature<HitTestingFeature>();
                _activeRaycasts = new List<Raycast>();
                _xrRaycastsToAdd = new List<XRRaycast>();
                _trackablesToRemove = new List<TrackableId>();
                _underlyingFeature.TryCreateRaycast(out _tempRaycastHandle);
            }

            public override void Stop()
            {
                if (_activeRaycasts == null)
                {
                    return;
                }

                foreach (var raycast in _activeRaycasts)
                {
                    RemoveRaycast(raycast.SubsystemRaycast.trackableId);
                }

                _underlyingFeature.TryDestroyRayCast(_tempRaycastHandle);
            }

            public override TrackableChanges<XRRaycast> GetChanges(XRRaycast defaultRaycast, Allocator allocator)
            {
                // Update the closest hit of active raycasts and update their status to tracking.
                var updatedRaycasts = new List<XRRaycast>();
                foreach (var raycast in _activeRaycasts.Where(raycast => !_xrRaycastsToAdd.Contains(raycast.SubsystemRaycast) && raycast.SubsystemRaycast.trackingState == TrackingState.None))
                {
                    if (!_underlyingFeature.TryGetRaycastState(raycast.RaycastHandle))
                    {
                        if (raycast.SubsystemRaycast.hitTrackableId != XRRaycastHit.defaultValue.trackableId || raycast.SubsystemRaycast.trackingState != TrackingState.None)
                        {
                            raycast.UpdateSubsystemRaycastHitAndTrackingState(XRRaycastHit.defaultValue, TrackingState.None);
                            updatedRaycasts.Add(raycast.SubsystemRaycast);
                        }

                        continue;
                    }

                    if (_underlyingFeature.TryGetRayCastCollisions(raycast.RaycastHandle, raycast.Ray.origin, out List<XRRaycastHit> raycastHits))
                    {
                        // Order hits by closest.
                        raycastHits.Sort((h1, h2) => h1.distance.CompareTo(h2.distance));
                        if (raycastHits.Count == 0)
                        {
                            continue;
                        }

                        if (raycast.SubsystemRaycast.hitTrackableId != raycastHits[0].trackableId || raycast.SubsystemRaycast.trackingState != TrackingState.Tracking)
                        {
                            raycast.UpdateSubsystemRaycastHitAndTrackingState(raycastHits[0], TrackingState.Tracking);
                            updatedRaycasts.Add(raycast.SubsystemRaycast);
                        }
                    }
                }

                var trackableChanges = TrackableChanges<XRRaycast>.CopyFrom(new NativeArray<XRRaycast>(_xrRaycastsToAdd.ToArray(), allocator),
                    new NativeArray<XRRaycast>(updatedRaycasts.ToArray(), allocator),
                    new NativeArray<TrackableId>(_trackablesToRemove.ToArray(), allocator),
                    allocator);
                _xrRaycastsToAdd.Clear();
                _trackablesToRemove.Clear();
                return trackableChanges;
            }

            public override bool TryAddRaycast(Vector2 screenPoint, float estimatedDistance, out XRRaycast raycast)
            {
                Ray ray = OriginLocationUtility.GetOriginCamera(true).ScreenPointToRay(screenPoint);
                return TryAddRaycast(ray, estimatedDistance, out raycast);
            }

            public override bool TryAddRaycast(Ray ray, float estimatedDistance, out XRRaycast raycast)
            {
                if (_underlyingFeature.TryCreateRaycast(out ulong rayCastHandle))
                {
                    if (_underlyingFeature.TryCastRay(rayCastHandle, ray.origin, ray.direction, Mathf.Infinity))
                    {
                        var newRaycast = new Raycast(rayCastHandle, ray);
                        _activeRaycasts.Add(newRaycast);
                        _xrRaycastsToAdd.Add(newRaycast.SubsystemRaycast);
                        raycast = newRaycast.SubsystemRaycast;
                        return true;
                    }
                }

                raycast = XRRaycast.defaultValue;
                return false;
            }

            public override void RemoveRaycast(TrackableId trackableId)
            {
                try
                {
                    var raycastToRemove = _activeRaycasts.SingleOrDefault(raycast => raycast.SubsystemRaycast.trackableId == trackableId);
                    if (_underlyingFeature.TryDestroyRayCast(raycastToRemove.RaycastHandle))
                    {
                        _trackablesToRemove.Add(raycastToRemove.SubsystemRaycast.trackableId);
                        _activeRaycasts.Remove(raycastToRemove);
                    }
                }
                catch (InvalidOperationException)
                {
                    Debug.LogError("Trying to remove XRRaycast with an invalid Trackable ID: " + trackableId);
                }
            }

            public override NativeArray<XRRaycastHit> Raycast(XRRaycastHit defaultRaycastHit, Ray ray, TrackableType trackableTypeMask, Allocator allocator)
            {
                return Raycast(defaultRaycastHit, ray.origin, ray.direction, trackableTypeMask, allocator);
            }

            public override NativeArray<XRRaycastHit> Raycast(XRRaycastHit defaultRaycastHit, Vector2 screenPoint, TrackableType trackableTypeMask, Allocator allocator)
            {
                return Raycast(defaultRaycastHit, screenPoint, Vector3.forward, trackableTypeMask, allocator);
            }

            private NativeArray<XRRaycastHit> Raycast(XRRaycastHit defaultRaycastHit, Vector3 origin, Vector3 direction, TrackableType trackableTypeMask, Allocator allocator)
            {
                var result = new NativeArray<XRRaycastHit>();
                if (_underlyingFeature.TryCreateRaycast(out ulong rayCastHandle))
                {
                    if (_underlyingFeature.TryCastRay(rayCastHandle, origin, direction, Mathf.Infinity))
                    {
                        if (_underlyingFeature.TryGetRaycastState(rayCastHandle))
                        {
                            if (_underlyingFeature.TryGetRayCastCollisions(rayCastHandle,
                                    origin,
                                    out List<XRRaycastHit> raycastHits))
                            {
                                /* Order hits by closest */
                                raycastHits.Sort((h1, h2) => h1.distance.CompareTo(h2.distance));
                                result = new NativeArray<XRRaycastHit>(raycastHits.ToArray(), allocator);
                            }
                        }
                    }

                    _underlyingFeature.TryDestroyRayCast(rayCastHandle);
                }

                return result;
            }
        }

        public const string ID = "Spaces-RaycastSubsystem";
        private const int MaxThreadBlockingTimeInMilliseconds = 100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(SpacesProvider),
                subsystemTypeOverride = typeof(RaycastSubsystem),
                supportsViewportBasedRaycast = true,
                supportsWorldBasedRaycast = true,
                supportedTrackableTypes = TrackableType.Planes,
                supportsTrackedRaycasts = false
            });
        }
    }
}
