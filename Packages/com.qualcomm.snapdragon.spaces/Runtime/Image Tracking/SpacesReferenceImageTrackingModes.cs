/******************************************************************************
 * File: SpacesReferenceImageTrackingModes.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [Serializable]
    internal class SpacesReferenceImageTrackingModes
    {
        [SerializeField]
        private List<XrImageTargetTrackingModeQCOM> _trackingModes = new List<XrImageTargetTrackingModeQCOM>();

        [SerializeField]
        private List<string> _referenceImageNames = new List<string>();

        public List<SpacesImageTrackingMode> TrackingModes => _trackingModes.ConvertAll(input => (SpacesImageTrackingMode)input);
        public List<string> ReferenceImageNames => _referenceImageNames;
        public int Count => _trackingModes.Count;

        public void SetTrackingModeForReferenceImage(string referenceImageName, XrImageTargetTrackingModeQCOM trackingMode)
        {
            var index = ReferenceImageNames.IndexOf(referenceImageName);
            if (index == -1)
            {
                Debug.LogWarning("Reference image name not found in tracking modes.");
            }
            else
            {
                _trackingModes[index] = trackingMode;
            }
        }

        public XrImageTargetTrackingModeQCOM GetTrackingModeForReferenceImage(string referenceImageName)
        {
            var index = ReferenceImageNames.IndexOf(referenceImageName);
            if (index == -1)
            {
                return XrImageTargetTrackingModeQCOM.XR_IMAGE_TARGET_TRACKING_MODE_MAX_ENUM_QCOM;
            }

            return _trackingModes[index];
        }

        internal void AddTrackingModeForReferenceImage(string referenceImageName, XrImageTargetTrackingModeQCOM trackingMode)
        {
            var index = ReferenceImageNames.IndexOf(referenceImageName);
            if (index != -1)
            {
                Debug.LogWarning($"Attempting to add tracking mode for reference image that already exists: {referenceImageName}. Each Xr Reference Image in Xr Reference Library must have a unique name.");
            }

            ReferenceImageNames.Add(referenceImageName);
            _trackingModes.Add(trackingMode);
        }

        internal void Clear()
        {
            _trackingModes.Clear();
            ReferenceImageNames.Clear();
        }
    }
}
