/******************************************************************************
 * File: XrSpaceLocation.cs
 * Copyright (c) 2021-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSpaceLocation
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _locationFlags;
        private XrPosef _pose;

        public void InitStructureType()
        {
            _type = XrStructureType.XR_TYPE_SPACE_LOCATION;
            _next = IntPtr.Zero;
        }

        public Pose GetPose()
        {
            return _pose.ToPose();
        }

        public TrackingState GetTrackingState()
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

        public override string ToString()
        {
            return "XrSpaceLocation: " + _type +
                " -- Tracking State: " + GetTrackingState() +
                " -- Pose: " + _pose.ToPose();
        }
    }
}
