/******************************************************************************
 * File: SpatialAnchorsSubsystem.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    internal class SpatialAnchorsSubsystem : XRAnchorSubsystem
    {
        internal class SpatialAnchorsProvider : Provider
        {
            private SpatialAnchorsFeature _underlyingFeature;
            private List<SpatialAnchor> _activeSpatialAnchors;
            private List<XRAnchor> _xrAnchorsToAdd;
            private List<TrackableId> _trackablesToRemove;
            private SpatialAnchor _persistentAnchorCanditate;
            private Queue<TrackableId> _availableAnchorIds;

            private const int _maxAnchorCapacity = 1000;

            private class AddAnchorData
            {
                public SpatialAnchor SpatialAnchor;
                public Pose Pose;
                public bool Success;

                public AddAnchorData(SpatialAnchor spatialAnchor, Pose pose)
                {
                    SpatialAnchor = spatialAnchor;
                    Pose = pose;
                    Success = false;
                }
            }
            private Thread _tryAddAnchorsWorker;
            private int _tryAddAnchorsWorkerRunning = 0; // false = 0; true = 1
            private readonly ConcurrentQueue<AddAnchorData> _anchorsToTryAdd = new();
            private readonly ConcurrentQueue<AddAnchorData> _anchorsAdded = new();

            public override void Start()
            {
                _underlyingFeature = OpenXRSettings.Instance.GetFeature<SpatialAnchorsFeature>();
                _activeSpatialAnchors = new List<SpatialAnchor>();
                _xrAnchorsToAdd = new List<XRAnchor>();
                _trackablesToRemove = new List<TrackableId>();

                _availableAnchorIds = new Queue<TrackableId>(_maxAnchorCapacity);
                for (int i = 0; i < _maxAnchorCapacity; i++)
                {
                    // NOTE(TD): All trackable Ids must have a subId1 of '1',
                    // because trackerId of (0-0) is an invalid Id.
                    _availableAnchorIds.Enqueue(new TrackableId(1, (ulong)i));
                }

                _tryAddAnchorsWorker = new Thread(AddAnchorsWorker) { Name = "AddAnchorsWorker" };
            }

            public override void Stop()
            {
                Destroy();
            }

            public override void Destroy()
            {
                if (_tryAddAnchorsWorker != null && _tryAddAnchorsWorker.IsAlive)
                {
                    _tryAddAnchorsWorker.Join();
                }

                // Note GÖ: After updating to the OpenXR 1.4.2 plugin, it seems that Destroy is being called after the
                // runtime is being exited and some values are destroyed already. That leads to an error log on
                // application exit. This condition was added to prevent that.
                if (_activeSpatialAnchors == null)
                {
                    Debug.LogWarning("No active spatial anchors list");
                    return;
                }

                if (_activeSpatialAnchors.Count > 0)
                {
                    SpatialAnchor[] activeAnchorsToRemove = new SpatialAnchor[_activeSpatialAnchors.Count];
                    _activeSpatialAnchors.CopyTo(activeAnchorsToRemove);
                    foreach (var anchor in activeAnchorsToRemove)
                    {
                        TryRemoveAnchor(anchor.SubsystemAnchor.trackableId);
                    }
                }
            }

            private void AddAnchorsWorker()
            {
                SpacesThreadUtility.SetThreadHint(SpacesThreadType.SPACES_THREAD_TYPE_APPLICATION_WORKER);

                // Try add all anchors in the queue once the thread has been started
                while (_anchorsToTryAdd.TryDequeue(out AddAnchorData anchorToAdd))
                {
                    ulong anchorHandle = _underlyingFeature.TryCreateSpatialAnchorHandle(anchorToAdd.Pose);
                    if (anchorHandle != 0)
                    {
                        ulong anchorSpaceHandle = _underlyingFeature.TryCreateSpatialAnchorSpaceHandle(anchorHandle);
                        if (anchorSpaceHandle != 0)
                        {
                            anchorToAdd.SpatialAnchor.AnchorHandle = anchorHandle;
                            anchorToAdd.SpatialAnchor.AnchorSpaceHandle = anchorSpaceHandle;

                            anchorToAdd.Success = true;
                            _anchorsAdded.Enqueue(anchorToAdd);
                            continue;
                        }
                    }
                    // NOTE(CH): If a failed anchor is not added to the anchorsAdded queue, GetChanges
                    // will not queue it for removal, leaving an ARAnchor with no subsystem anchor.
                    _anchorsAdded.Enqueue(anchorToAdd);
                    Debug.LogError("Failed to add anchor with TrackableId: " + anchorToAdd.SpatialAnchor.SubsystemAnchor.trackableId);
                }

                Interlocked.Exchange(ref _tryAddAnchorsWorkerRunning, 0);
            }

            public override TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                // Handle all newly added anchors
                while (_anchorsAdded.TryDequeue(out AddAnchorData addedAnchor))
                {
                    if (addedAnchor.Success)
                    {
                        _activeSpatialAnchors.Add(addedAnchor.SpatialAnchor);
                        _xrAnchorsToAdd.Add(addedAnchor.SpatialAnchor.SubsystemAnchor);
                    }
                    // NOTE(TD): If the anchor was not successfully created, it's trackable
                    // will be added to the list of trackableId's to be removed. This means
                    // that it will go straight into ARAnchorsChangedEventArgs' removed list
                    // without ever being added.
                    else
                    {
                        Debug.LogWarning("Failed to add anchor!");
                        _trackablesToRemove.Add(addedAnchor.SpatialAnchor.SubsystemAnchor.trackableId);
                    }
                }

                // Update the poses of active spatial anchors.
                var updatedAnchors = new List<XRAnchor>();
                foreach (var spatialAnchor in _activeSpatialAnchors.Where(spatialAnchor => !_xrAnchorsToAdd.Contains(spatialAnchor.SubsystemAnchor)))
                {
                    var poseAndState = _underlyingFeature.TryGetSpatialAnchorSpacePoseAndTrackingState(spatialAnchor.AnchorSpaceHandle);
                    if (spatialAnchor.UpdateSubsystemAnchorPoseAndTrackingState(poseAndState))
                    {
                        updatedAnchors.Add(spatialAnchor.SubsystemAnchor);
                    }
                }

                var trackableChanges = TrackableChanges<XRAnchor>.CopyFrom(new NativeArray<XRAnchor>(_xrAnchorsToAdd.ToArray(), allocator),
                    new NativeArray<XRAnchor>(updatedAnchors.ToArray(), allocator),
                    new NativeArray<TrackableId>(_trackablesToRemove.ToArray(), allocator),
                    allocator);
                _xrAnchorsToAdd.Clear();
                _trackablesToRemove.Clear();
                return trackableChanges;
            }

            public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
            {
                if (!_availableAnchorIds.TryDequeue(out TrackableId id))
                {
                    Debug.LogWarning("Can not create any more spatial anchors");
                    anchor = XRAnchor.defaultValue;
                    return false;
                }
                // We're creating an anchor that has been loaded from the SpacesAnchorStore
                if (_persistentAnchorCanditate != null)
                {
                    var newPersistentAnchor = new SpatialAnchor(id,
                        _persistentAnchorCanditate.AnchorHandle,
                        _persistentAnchorCanditate.AnchorSpaceHandle,
                        _persistentAnchorCanditate.SubsystemAnchor.pose,
                        _persistentAnchorCanditate.SavedName);

                    _xrAnchorsToAdd.Add(newPersistentAnchor.SubsystemAnchor);
                    _activeSpatialAnchors.Add(newPersistentAnchor);
                    anchor = newPersistentAnchor.SubsystemAnchor;
                    _persistentAnchorCanditate = null;
                    return true;
                }

                // Setting the anchor handles to the max value, indicating that they are invalid as of now
                var newAnchor = new SpatialAnchor(id, ulong.MaxValue, ulong.MaxValue, pose);
                anchor = newAnchor.SubsystemAnchor;

                AddAnchorData anchorData = new AddAnchorData(newAnchor, pose);
                _anchorsToTryAdd.Enqueue(anchorData);

                // Start the 'add anchors' thread if it's not already running
                if (Interlocked.CompareExchange(ref _tryAddAnchorsWorkerRunning, 1, 0) == 0)
                {
                    // The thread has finished working, but is still alive.
                    if (_tryAddAnchorsWorker.IsAlive)
                    {
                        _tryAddAnchorsWorker.Join();
                    }
                    _tryAddAnchorsWorker.Start();
                }

                return true;
            }

            public void SetPersistentAnchorCandidate(SpatialAnchor persistentAnchorCandidate)
            {
                _persistentAnchorCanditate = persistentAnchorCandidate;
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                try
                {
                    var anchorToRemove = _activeSpatialAnchors.SingleOrDefault(anchor => anchor.SubsystemAnchor.trackableId == anchorId);
                    if (_underlyingFeature.TryDestroySpatialAnchor(anchorToRemove.AnchorHandle))
                    {
                        _trackablesToRemove.Add(anchorToRemove.SubsystemAnchor.trackableId);
                        _activeSpatialAnchors.Remove(anchorToRemove);
                        _availableAnchorIds.Enqueue(anchorToRemove.SubsystemAnchor.trackableId);
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    Debug.LogError("Trying to remove XRAnchor with an invalid Trackable ID: " + anchorId);
                }

                return false;
            }

            [Obsolete]
            public bool TryAddAnchorFromPersistentName(ulong spatialAnchorStore, string spatialAnchorName)
            {
                if (_underlyingFeature.TryCreateSpatialAnchorFromPersistedNameMSFT(spatialAnchorStore, spatialAnchorName, out ulong spatialAnchorHandle))
                {
                    ulong anchorSpaceHandle = _underlyingFeature.TryCreateSpatialAnchorSpaceHandle(spatialAnchorHandle);
                    if (anchorSpaceHandle != 0)
                    {
                        if (!_availableAnchorIds.TryDequeue(out TrackableId id))
                        {
                            Debug.LogWarning("Can not create any more spatial anchors");
                            return false;
                        }

                        var spatialAnchor = new SpatialAnchor(id, spatialAnchorHandle, anchorSpaceHandle, Pose.identity, spatialAnchorName);
                        _xrAnchorsToAdd.Add(spatialAnchor.SubsystemAnchor);
                        return true;
                    }
                }

                return false;
            }

            public string TryGetSavedNameFromTrackableId(TrackableId trackableId)
            {
                foreach (var activeAnchor in _activeSpatialAnchors)
                {
                    if (activeAnchor.SubsystemAnchor.trackableId == trackableId)
                    {
                        return activeAnchor.SavedName;
                    }
                }

                return string.Empty;
            }

            public ulong TryGetSpatialAnchorHandleFromTrackableId(TrackableId trackableId)
            {
                foreach (var anchor in _activeSpatialAnchors)
                {
                    if (anchor.SubsystemAnchor.trackableId == trackableId)
                    {
                        return anchor.AnchorHandle;
                    }
                }

                return 0;
            }

            public void UpdateAnchorSavedName(TrackableId anchorId, string savedName)
            {
                foreach (var activeAnchor in _activeSpatialAnchors)
                {
                    if (activeAnchor.SubsystemAnchor.trackableId == anchorId)
                    {
                        activeAnchor.SavedName = savedName;
                        return;
                    }
                }
            }

            public void ClearAllAnchorSavedNames()
            {
                foreach (var activeAnchor in _activeSpatialAnchors)
                {
                    activeAnchor.SavedName = string.Empty;
                }
            }
        }

        public const string ID = "Spaces-SpatialAnchorsSubsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(SpatialAnchorsProvider),
                subsystemTypeOverride = typeof(SpatialAnchorsSubsystem),
                supportsTrackableAttachments = false
            });
        }
    }
}
