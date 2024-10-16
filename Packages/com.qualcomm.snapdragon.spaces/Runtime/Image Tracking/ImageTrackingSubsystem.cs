/******************************************************************************
 * File: ImageTrackingSubsystem.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    internal class ImageTarget
    {
        public XRTrackedImage SubsystemImageTarget;

        public ImageTarget(ulong id, XRTrackedImage trackedImage)
        {
            SubsystemImageTarget = trackedImage;
            ImageTargetHandle = id;
        }

        public ulong ImageTargetHandle { get; }
    }

    internal class ImageTrackingSubsystem : XRImageTrackingSubsystem
    {
        internal class ImageTrackingProvider : Provider
        {
            internal delegate SpacesReferenceImageTrackingModes SpacesInitialTrackingModesDelegate();

            private readonly List<ImageTarget> _activeTrackedImages = new List<ImageTarget>();
            private ulong _imageTrackerHandle;
            private ImageTrackingFeature _underlyingFeature;
            private RuntimeReferenceImageLibrary _imageLibrary;
            private int _maxNumberOfMovingImages;
            private SpacesReferenceImageTrackingModes _imageTrackingModes;
            private SpacesInitialTrackingModesDelegate _getInitialTrackingModes;

            public override RuntimeReferenceImageLibrary imageLibrary
            {
                set
                {
                    if (value != null)
                    {
                        _imageLibrary = value;
                        if (_imageTrackerHandle != 0)
                        {
                            Debug.LogWarning("Can't change the image library while subsystem is running.");
                        }
                    }
                    else
                    {
                        _imageLibrary = null;
                        DestroyImageTracker();
                    }
                }
            }

            public override int requestedMaxNumberOfMovingImages
            {
                get => _maxNumberOfMovingImages;
                set => _maxNumberOfMovingImages = value;
            }

            public void SetInitialTrackingModesDelegate(SpacesInitialTrackingModesDelegate del)
            {
                _getInitialTrackingModes = del;
            }

            public override void Start()
            {
                _underlyingFeature = OpenXRSettings.Instance.GetFeature<ImageTrackingFeature>();
                _underlyingFeature.TryCreateImageTracker(out _imageTrackerHandle, _imageLibrary, _maxNumberOfMovingImages);
            }

            public override void Stop()
            {
                DestroyImageTracker();
            }

            public override void Destroy() { }

            public override TrackableChanges<XRTrackedImage> GetChanges(XRTrackedImage defaultTrackedImage, Allocator allocator)
            {
                // NOTE: Since method is called frequently, move initialization of lists to Start
                var addedTrackedImages = new List<XRTrackedImage>();
                var updatedTrackedImages = new List<XRTrackedImage>();
                var removedTrackableIds = new List<TrackableId>();
                if (!_underlyingFeature.TryLocateImageTargets(_imageTrackerHandle, _maxNumberOfMovingImages, out var newTrackedImages))
                {
                    return new TrackableChanges<XRTrackedImage>(0, 0, 0, allocator, defaultTrackedImage);
                }

                foreach (var newImageTarget in newTrackedImages)
                {
                    bool canAdd = true;
                    foreach (var activeImageTarget in _activeTrackedImages)
                    {
                        // Check for added list.
                        if (newImageTarget.trackableId == activeImageTarget.SubsystemImageTarget.trackableId)
                        {
                            canAdd = false;
                            // Check for updated list.
                            if (newImageTarget.pose != activeImageTarget.SubsystemImageTarget.pose)
                            {
                                updatedTrackedImages.Add(newImageTarget);
                                break;
                            }
                        }
                    }

                    if (canAdd)
                    {
                        addedTrackedImages.Add(newImageTarget);
                        _activeTrackedImages.Add(new ImageTarget(newImageTarget.trackableId.subId2, newImageTarget));
                    }
                }

                // Check for removed list
                var toRemoveImages = new List<ImageTarget>();
                foreach (var activeImageTarget in _activeTrackedImages)
                {
                    bool canRemove = newTrackedImages.All(newImageTarget => activeImageTarget.SubsystemImageTarget.trackableId != newImageTarget.trackableId);
                    if (canRemove)
                    {
                        removedTrackableIds.Add(activeImageTarget.SubsystemImageTarget.trackableId);
                        toRemoveImages.Add(activeImageTarget);
                    }
                }

                foreach (var imageToRemove in toRemoveImages)
                {
                    _activeTrackedImages.Remove(imageToRemove);
                }

                return TrackableChanges<XRTrackedImage>.CopyFrom(new NativeArray<XRTrackedImage>(addedTrackedImages.ToArray(), allocator),
                    new NativeArray<XRTrackedImage>(updatedTrackedImages.ToArray(), allocator),
                    new NativeArray<TrackableId>(removedTrackableIds.ToArray(), allocator),
                    allocator);
            }

            public override RuntimeReferenceImageLibrary CreateRuntimeLibrary(XRReferenceImageLibrary serializedLibrary)
            {
                if (_imageTrackingModes == null)
                {
                    if (_getInitialTrackingModes == null)
                    {
                        Debug.LogWarning("No delegate to retrieve image tracking modes defined");
                        _imageTrackingModes = new SpacesReferenceImageTrackingModes();
                    }
                    else
                    {
                        _imageTrackingModes = _getInitialTrackingModes.Invoke();
                    }
                }

                return new SpacesRuntimeReferenceImageLibrary(serializedLibrary, _imageTrackingModes);
            }

            public void SetTrackingModes(SpacesReferenceImageTrackingModes trackingModes)
            {
                if (!_underlyingFeature.TrySetTrackingModes(_imageTrackerHandle, trackingModes.ReferenceImageNames, trackingModes.TrackingModes))
                {
                    Debug.LogWarning("Failed to set image tracking mode on provider");
                }
            }

            public void StopTrackingImageInstance(string referenceImageName, uint trackableId)
            {
                if (!_underlyingFeature.TryStopTrackingImageInstance(referenceImageName, trackableId))
                {
                    Debug.LogWarning("Failed to stop tracking image");
                }
            }

            private void DestroyImageTracker()
            {
                if (_imageTrackerHandle != 0 && _underlyingFeature.TryDestroyImageTracker(_imageTrackerHandle))
                {
                    _imageTrackerHandle = 0;
                    _activeTrackedImages.Clear();
                }
            }
        }

        public const string ID = "Spaces-ImageTrackingSubsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRImageTrackingSubsystemDescriptor.Create(new XRImageTrackingSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(ImageTrackingProvider),
                subsystemTypeOverride = typeof(ImageTrackingSubsystem),
                supportsMovingImages = true,
                supportsMutableLibrary = false
            });
        }
    }
}
