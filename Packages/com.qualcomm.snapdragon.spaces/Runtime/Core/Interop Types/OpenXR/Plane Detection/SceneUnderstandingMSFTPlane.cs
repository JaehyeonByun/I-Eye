/******************************************************************************
 * File: SceneUnderstandingMSFTPlane.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SceneUnderstandingMSFTPlane
    {
        private MeshId _id;
        private int _alignment;

        // XrExtent2Df -- size of this does not match XrExtent2DfQCOM! It is two floats, width and height.
        private XrVector2f _extent;
        private ulong _locationFlags;
        private XrPosef _pose;
        private uint _vertexCount;
        private uint _indexCount;

        public uint VertexCount => _vertexCount;
        public uint IndexCount => _indexCount;
        public Pose Pose => _pose.ToPose();

        public BoundedPlane GetBoundedPlane(Pose replacementPose)
        {
            return new BoundedPlane(new TrackableId(_id.ToString()),
                TrackableId.invalidId,
                replacementPose,
                Vector2.zero,
                _extent.ToVector2(),
                XrScenePlaneAlignmentTypeToPlaneAlignment((XrScenePlaneAlignmentTypeMSFT)_alignment),
                GetTrackingState(),
                IntPtr.Zero,
                PlaneClassification.None);
        }

        private PlaneAlignment XrScenePlaneAlignmentTypeToPlaneAlignment(XrScenePlaneAlignmentTypeMSFT type)
        {
            switch (type)
            {
                case XrScenePlaneAlignmentTypeMSFT.XR_SCENE_PLANE_ALIGNMENT_TYPE_VERTICAL_MSFT: return PlaneAlignment.Vertical;
                case XrScenePlaneAlignmentTypeMSFT.XR_SCENE_PLANE_ALIGNMENT_TYPE_HORIZONTAL_MSFT: return PlaneAlignment.HorizontalUp;
                case XrScenePlaneAlignmentTypeMSFT.XR_SCENE_PLANE_ALIGNMENT_TYPE_NON_ORTHOGONAL_MSFT: return PlaneAlignment.NotAxisAligned;
                default:
                    Debug.LogWarning("XrScenePlaneAlignmentTypeMSFT [ " + type + "]: not supported!");
                    return PlaneAlignment.None;
            }
        }

        private TrackingState GetTrackingState()
        {
            ulong validPoseFlags = (ulong)(XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT | XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT);
            ulong trackedPoseFlags = (ulong)(XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT | XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT);

            if (validPoseFlags == (_locationFlags & validPoseFlags))
            {
                if (trackedPoseFlags == (_locationFlags & trackedPoseFlags))
                {
                    return TrackingState.Tracking;
                }

                return TrackingState.Limited;
            }

            return TrackingState.None;
        }
    }
}
