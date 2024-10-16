/******************************************************************************
 * File: SpacesCameraPoseProvider.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public class SpacesCameraPoseProvider : BasePoseProvider
    {
        private CameraAccessFeature _cameraAccess;

        private void Start()
        {
            _cameraAccess = OpenXRSettings.Instance.GetFeature<CameraAccessFeature>();
            if (!FeatureUseCheckUtility.IsFeatureUseable(_cameraAccess))
            {
#if !UNITY_EDITOR
                Debug.LogError("Could not get valid camera access feature");
#endif
                return;
            }
        }

        public override PoseDataFlags GetPoseFromProvider(out Pose output)
        {
            output = default;
            if (!FeatureUseCheckUtility.IsFeatureUseable(_cameraAccess))
            {
                return PoseDataFlags.NoData;
            }
            output = _cameraAccess.LastFramePose;
            return PoseDataFlags.Position | PoseDataFlags.Rotation;
        }
    }
}
