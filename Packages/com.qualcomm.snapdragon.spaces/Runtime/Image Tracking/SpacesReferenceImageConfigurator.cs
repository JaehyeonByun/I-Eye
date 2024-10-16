/******************************************************************************
 * File: SpacesReferenceImageConfigurator.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [Serializable]
    [DefaultExecutionOrder(int.MinValue)]
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class SpacesReferenceImageConfigurator : MonoBehaviour
    {
        [SerializeField]
        private SpacesReferenceImageTrackingModes _trackingModes;

        private ImageTrackingSubsystem.ImageTrackingProvider _imageTrackingProvider;
        private ImageTrackingSubsystem _imageTrackingSubsystem;
        private ARTrackedImageManager _trackedImageManager;

        private SpacesReferenceImageConfigurator()
        {
            _trackingModes = new SpacesReferenceImageTrackingModes();
        }

        /// <summary>
        ///     Set the tracking mode for the given reference name.
        ///     Fails if the referenceImageName is not a valid name for an image in the AR Tracked Image Manager's reference
        ///     library.
        /// </summary>
        /// <param name="referenceImageName">Name of the reference image in the reference library</param>
        /// <param name="spacesImageTrackingMode">Tracking mode to set for the reference image</param>
        public void SetTrackingModeForReferenceImage(string referenceImageName, SpacesImageTrackingMode spacesImageTrackingMode)
        {
            bool found = false;
            for (var index = 0; index < _trackedImageManager.referenceLibrary.count; ++index)
            {
                if (_trackedImageManager.referenceLibrary[index].name == referenceImageName)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarning($"Called set tracking mode for reference image name '{referenceImageName}' not in tracked image manager reference library.");
                return;
            }

            if (spacesImageTrackingMode == SpacesImageTrackingMode.INVALID)
            {
                Debug.LogError($"Cannot set tracking mode to Invalid. Called with reference image '{referenceImageName}'.");
                return;
            }

            _trackingModes.SetTrackingModeForReferenceImage(referenceImageName, (XrImageTargetTrackingModeQCOM)spacesImageTrackingMode);
            if (_imageTrackingSubsystem.running)
            {
                _imageTrackingProvider.SetTrackingModes(_trackingModes);
            }
        }

        /// <summary>
        ///     Get the tracking mode for the given reference image
        /// </summary>
        /// <param name="referenceImageName">The name of the reference image to get the tracking mode for</param>
        /// <returns>the tracking mode if the reference image exists, INVALID otherwise.</returns>
        public SpacesImageTrackingMode GetTrackingModeForReferenceImage(string referenceImageName)
        {
            return (SpacesImageTrackingMode)_trackingModes.GetTrackingModeForReferenceImage(referenceImageName);
        }

        public bool HasReferenceImageTrackingMode(string referenceImageName)
        {
            return _trackingModes.ReferenceImageNames.Contains(referenceImageName);
        }

        public void StopTrackingImageInstance(string referenceImageName, TrackableId trackableId)
        {
            _imageTrackingProvider.StopTrackingImageInstance(referenceImageName, (uint)trackableId.subId2);
        }

        private void Awake()
        {
            _trackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        private void OnEnable()
        {
            var subsystems = new List<ImageTrackingSubsystem>();
            SubsystemManager.GetInstances(subsystems);
            if (subsystems.Count > 0)
            {
                _imageTrackingSubsystem = subsystems[0];
                _imageTrackingProvider = (ImageTrackingSubsystem.ImageTrackingProvider)_imageTrackingSubsystem.GetProvider();
            }
            else
            {
                Debug.LogError("Failed to get ImageTrackingSubsystem instance. Aborting SpacesReferenceImageTrackingModeConfigurator initialization!");
                return;
            }

            _imageTrackingProvider.SetInitialTrackingModesDelegate(GetSpacesReferenceImageTrackingModes);
        }

        private void OnDisable()
        {
            _imageTrackingProvider.SetInitialTrackingModesDelegate(null);
        }

        internal SpacesReferenceImageTrackingModes GetSpacesReferenceImageTrackingModes()
        {
            return _trackingModes;
        }

        public Dictionary<string, SpacesImageTrackingMode> CreateTrackingModesDictionary()
        {
            var trackingModesDict = new Dictionary<string, SpacesImageTrackingMode>();

            var targetModes = _trackingModes.TrackingModes;
            for (int index = 0; index < _trackingModes.Count; ++index)
            {
                trackingModesDict[_trackingModes.ReferenceImageNames[index]] = targetModes[index];
            }

            return trackingModesDict;
        }

        public void SyncTrackingModes(Dictionary<string, SpacesImageTrackingMode> trackingModesDict)
        {
            _trackingModes.Clear();
            foreach (var trackingModeKvp in trackingModesDict)
            {
                _trackingModes.AddTrackingModeForReferenceImage(trackingModeKvp.Key, (XrImageTargetTrackingModeQCOM)trackingModeKvp.Value);
            }
        }
    }
}
