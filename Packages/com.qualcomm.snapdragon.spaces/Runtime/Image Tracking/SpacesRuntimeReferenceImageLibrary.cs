/******************************************************************************
 * File: SpacesRuntimeReferenceImageLibrary.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    public class SpacesRuntimeReferenceImageLibrary : RuntimeReferenceImageLibrary
    {
        private readonly List<XRReferenceImage> _images = new List<XRReferenceImage>();

        internal SpacesRuntimeReferenceImageLibrary(XRReferenceImageLibrary serializedLibrary, SpacesReferenceImageTrackingModes trackingModes)
        {
            if (serializedLibrary == null)
            {
                return;
            }

            if (trackingModes.Count != serializedLibrary.count)
            {
                Debug.LogWarning("Number of tracking modes defined does not match the number of images in the reference library!");
            }

            for (int i = 0; i < serializedLibrary.count; i++)
            {
                if (serializedLibrary[i].texture == null)
                {
                    Debug.LogWarning("XRReferenceImage '" + serializedLibrary[i].name + "' has no valid texture set.");
                    continue;
                }

                if (serializedLibrary[i].texture.format != TextureFormat.RGB24)
                {
                    Debug.LogWarning("XRReferenceImage '" + serializedLibrary[i].name + "' has an invalid texture format (" + serializedLibrary[i].texture.format + "). Image targets must be set to RGB24 bit format.");
                    continue;
                }

                if (!serializedLibrary[i].specifySize || serializedLibrary[i].size == Vector2.zero)
                {
                    Debug.LogWarning("XRReferenceImage '" + serializedLibrary[i].name + "' does not have a specified physical size.");
                    continue;
                }

                if (!trackingModes.ReferenceImageNames.Contains(serializedLibrary[i].name))
                {
                    Debug.LogWarning($"No tracking mode defined for {serializedLibrary[i].name}. Using Dynamic by default.");
                    TrackingModes.AddTrackingModeForReferenceImage(serializedLibrary[i].name, XrImageTargetTrackingModeQCOM.XR_IMAGE_TARGET_TRACKING_MODE_DYNAMIC_QCOM);
                }
                else
                {
                    TrackingModes.AddTrackingModeForReferenceImage(serializedLibrary[i].name, trackingModes.GetTrackingModeForReferenceImage(serializedLibrary[i].name));
                }

                _images.Add(serializedLibrary[i]);
            }
        }

        public override int count => _images.Count;
        internal SpacesReferenceImageTrackingModes TrackingModes { get; } = new SpacesReferenceImageTrackingModes();

        protected override XRReferenceImage GetReferenceImageAt(int index)
        {
            return _images[index];
        }
    }
}
