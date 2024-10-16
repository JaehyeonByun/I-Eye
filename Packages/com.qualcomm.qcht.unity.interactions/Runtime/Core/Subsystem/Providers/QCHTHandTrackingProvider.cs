// /******************************************************************************
//  * File: QCHTHandTrackingProvider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using Unity.Profiling;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    public class QCHTHandTrackingProvider : XRHandTrackingSubsystem.Provider
    {
        public const string ID = "Qualcomm-HandTracking-Native";

        private XrPosef[] _leftPoses = new XrPosef[(int) XrHandJoint.XR_HAND_JOINT_MAX];
        private XrPosef[] _rightPoses = new XrPosef[(int) XrHandJoint.XR_HAND_JOINT_MAX];
        
        private static readonly ProfilerMarker s_tryUpdateHandDataMarker = new ProfilerMarker("[QCHT] QCHTHandTrackingProvider.TryUpdateHandData");
        private static readonly ProfilerMarker s_tryLocateHandJointsMarker = new ProfilerMarker("[QCHT] QCHTHandTrackingProvider.TryUpdateHandData.TryLocateHandJoints");

        public override HandTrackingStatus TryUpdateHandData(XrHandedness handedness, ref bool isTracked, ref Pose rootPose, ref Pose[] joints, ref float[] radiuses)
        {
            using (s_tryUpdateHandDataMarker.Auto())
            {
                var isLeft = handedness == XrHandedness.XR_HAND_LEFT;
                ref var poses = ref isLeft ? ref _leftPoses : ref _rightPoses;
                var handSide = isLeft ? XrHandEXT.XR_HAND_LEFT : XrHandEXT.XR_HAND_RIGHT;

                XrResult result;

                using (s_tryLocateHandJointsMarker.Auto())
                {
                    result = QCHTOpenXRPlugin.TryLocateHandJoints(handSide, ref isTracked, poses, radiuses);
                }

                // In this case just return idle status and wait for next frames
                if (result == XrResult.XR_ERROR_TIME_INVALID)
                {
                    Debug.LogWarning("[QCHTHandTrackingProvider:TryUpdateHandData] OpenXR predicted display time");
                    return HandTrackingStatus.Idle;
                }

                if (result < XrResult.XR_SUCCESS)
                {
                    return HandTrackingStatus.Error;
                }
                
                for (var i = 0; i < (int) XrHandJoint.XR_HAND_JOINT_MAX; i++)
                {
                    joints[i] = poses[i].ToPose();
                }

                rootPose = joints[(int) XrHandJoint.XR_HAND_JOINT_WRIST];
            }

            return HandTrackingStatus.Running;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRHandTrackingSubsystemDescriptor.Create(new XRHandTrackingSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(QCHTHandTrackingProvider)
            });
        }
    }
}