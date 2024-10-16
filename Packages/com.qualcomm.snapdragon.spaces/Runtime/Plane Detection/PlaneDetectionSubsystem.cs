/******************************************************************************
 * File: PlaneDetectionSubsystem.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    internal class PlaneDetectionSubsystem : XRPlaneSubsystem
    {
        private class PlaneDetectionProvider : Provider
        {
            private readonly Dictionary<TrackableId, Plane> _activePlanes = new Dictionary<TrackableId, Plane>();
            private PlaneDetectionFeature _underlyingFeature;
            private PlaneDetectionMode _currentPlaneDetectionMode = PlaneDetectionMode.None;
            private PlaneDetectionMode _requestedPlaneDetectionMode = PlaneDetectionMode.None;
            public override PlaneDetectionMode currentPlaneDetectionMode => _currentPlaneDetectionMode;

            public override PlaneDetectionMode requestedPlaneDetectionMode
            {
                get => _requestedPlaneDetectionMode;
                set
                {
                    _requestedPlaneDetectionMode = value;
                    if (_underlyingFeature != null && _underlyingFeature.IsRunning)
                    {
                        Debug.LogWarning("Can't change the plane detection mode while subsystem is running. Recreate PlaneManager to change the plane detection mode.");
                    }
                }
            }

            public override void Start()
            {
                _underlyingFeature = OpenXRSettings.Instance.GetFeature<PlaneDetectionFeature>();
                if (_underlyingFeature != null)
                {
                    _underlyingFeature.TryCreatePlaneDetection(ID, _requestedPlaneDetectionMode, _underlyingFeature.ConvexHullEnabled);
                    _underlyingFeature.RegisterProviderWithSceneObserver(ID);
                }

                _currentPlaneDetectionMode = _requestedPlaneDetectionMode;
            }

            public override void Stop()
            {
                DestroyPlaneDetection();
                if (_underlyingFeature != null)
                {
                    _underlyingFeature.UnregisterProviderWithSceneObserver(ID);
                }
            }

            public override void Destroy()
            {
                if (_underlyingFeature != null)
                {
                    _underlyingFeature.UnregisterProviderWithSceneObserver(ID);
                }
            }

            public override void GetBoundary(TrackableId trackableId, Allocator allocator, ref NativeArray<Vector2> boundary)
            {
                var vertexPositions = new List<Vector2>();
                ulong convexHullBufferId = _activePlanes[trackableId].ConvexHullId;
                _underlyingFeature.TryGetPlaneConvexHullVertexBuffer(convexHullBufferId, ref vertexPositions);
                boundary = new NativeArray<Vector2>(vertexPositions.ToArray(), allocator);
            }

            public override TrackableChanges<BoundedPlane> GetChanges(BoundedPlane defaultPlane, Allocator allocator)
            {
                var addedPlanes = new List<BoundedPlane>();
                var updatedPlanes = new List<BoundedPlane>();
                var removedPlanes = new List<TrackableId>();
                if (_underlyingFeature == null || !_underlyingFeature.TryLocatePlanes(out List<Plane> newPlanes))
                {
                    return new TrackableChanges<BoundedPlane>(0, 0, 0, allocator, defaultPlane);
                }

                foreach (var newPlane in newPlanes)
                {
                    bool canAdd = true;
                    foreach (var entry in _activePlanes)
                    {
                        if (newPlane.BoundedPlane.trackableId == entry.Key)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                    {
                        if (!_activePlanes.ContainsKey(newPlane.BoundedPlane.trackableId))
                        {
                            _activePlanes.Add(newPlane.BoundedPlane.trackableId, newPlane);
                        }

                        addedPlanes.Add(newPlane.BoundedPlane);
                    }
                    else
                    {
                        updatedPlanes.Add(newPlane.BoundedPlane);
                    }
                }

                foreach (KeyValuePair<TrackableId, Plane> entry in _activePlanes)
                {
                    bool canRemove = true;
                    foreach (var newPlane in newPlanes)
                    {
                        if (entry.Key == newPlane.BoundedPlane.trackableId)
                        {
                            canRemove = false;
                            break;
                        }
                    }

                    if (canRemove)
                    {
                        removedPlanes.Add(entry.Key);
                    }
                }

                foreach (var removedPlaneId in removedPlanes)
                {
                    _activePlanes.Remove(removedPlaneId);
                }

                return TrackableChanges<BoundedPlane>.CopyFrom(new NativeArray<BoundedPlane>(addedPlanes.ToArray(), allocator),
                    new NativeArray<BoundedPlane>(updatedPlanes.ToArray(), allocator),
                    new NativeArray<TrackableId>(removedPlanes.ToArray(), allocator),
                    allocator);
            }

            private void DestroyPlaneDetection()
            {
                if (_underlyingFeature != null)
                {
                    _underlyingFeature.TryDestroyPlaneDetection(ID);
                }
            }
        }

        public const string ID = "Spaces-PlaneDetectionSubsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(PlaneDetectionProvider),
                subsystemTypeOverride = typeof(PlaneDetectionSubsystem),
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
                supportsArbitraryPlaneDetection = true,
                supportsBoundaryVertices = true,
                supportsClassification = false
            });
        }
    }
}
