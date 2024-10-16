// /******************************************************************************
//  * File: XRHandSimulationProvider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace QCHT.Interactions.Core
{
    public class XRHandSimulationProvider : XRHandTrackingSubsystem.Provider
    {
        private enum HandGesture
        {
            Unknown = -1,
            OpenHand = 0,
            Pinch = 1,
            Grab = 2
        }

        public const string ID = "Qualcomm-HandTracking-Simulation";

        public float HandScale { get; internal set; } = 1f;

        public override HandTrackingStatus TryUpdateHandData(XrHandedness handedness, ref bool isTracked,
            ref Pose rootPose,
            ref Pose[] joints, ref float[] radiuses)
        {
            radiuses[(int)XrHandJoint.XR_HAND_JOINT_WRIST] = HandScale;

            var ctrl = handedness == XrHandedness.XR_HAND_LEFT
                ? InputSystem.GetDevice<XRSimulatedController>(CommonUsages.LeftHand)
                : InputSystem.GetDevice<XRSimulatedController>(CommonUsages.RightHand);

            var gesture = HandGesture.Unknown;

            if (ctrl != null)
            {
                rootPose.position = ctrl.devicePosition.ReadValue();
                rootPose.rotation = ctrl.deviceRotation.ReadValue();
                rootPose.rotation *= Quaternion.AngleAxis(90f, Vector3.left);

                if (ctrl.trigger.IsPressed())
                {
                    gesture = HandGesture.Pinch;
                }
                else if (ctrl.grip.IsPressed())
                {
                    gesture = HandGesture.Grab;
                }
            }

            var eyeCtrl = InputSystem.GetDevice<XRSimulatedHMD>();
            if (eyeCtrl != null)
            {
                eyeCtrl.centerEyePosition.ReadValue();
                eyeCtrl.centerEyeRotation.ReadValue();
            }

            isTracked = ctrl == null || ctrl.isTracked.IsPressed();

            if (isTracked)
            {
                var poseAsset = GetPoseAsset(gesture, handedness);
                AssignPose(ref joints, poseAsset, rootPose, HandScale);
            }

            return HandTrackingStatus.Running;
        }

        private static void AssignPose(ref Pose[] joints, HandPose poseAsset, Pose wrist, float scale)
        {
            ConvertHandPoseToOpenXRData(ref joints, poseAsset);
            wrist.rotation *= Quaternion.AngleAxis(90f, Vector3.right);
            for (var i = 0; i < joints.Length; i++)
            {
                joints[i].rotation = wrist.rotation * joints[i].rotation;
                joints[i].position = wrist.position + wrist.rotation * joints[i].position * scale;
            }
        }

        private static HandPose GetPoseAsset(HandGesture gesture, XrHandedness handedness) =>
            gesture switch
            {
                HandGesture.Unknown => handedness == XrHandedness.XR_HAND_LEFT
                    ? XRHandSimulationHandPosesSettings.Instance.leftOpenHand
                    : XRHandSimulationHandPosesSettings.Instance.rightOpenHand,
                HandGesture.OpenHand => handedness == XrHandedness.XR_HAND_LEFT
                    ? XRHandSimulationHandPosesSettings.Instance.leftOpenHand
                    : XRHandSimulationHandPosesSettings.Instance.rightOpenHand,
                HandGesture.Pinch => handedness == XrHandedness.XR_HAND_LEFT
                    ? XRHandSimulationHandPosesSettings.Instance.leftPinchHand
                    : XRHandSimulationHandPosesSettings.Instance.rightPinchHand,
                HandGesture.Grab => handedness == XrHandedness.XR_HAND_LEFT
                    ? XRHandSimulationHandPosesSettings.Instance.leftGrabHand
                    : XRHandSimulationHandPosesSettings.Instance.rightGrabHand,
                _ => throw new ArgumentOutOfRangeException(nameof(gesture), gesture, null)
            };

        private static void ConvertHandPoseToOpenXRData(ref Pose[] joints, HandPose pose)
        {
            if (!pose) return;

            // Root
            joints[(int)XrHandJoint.XR_HAND_JOINT_WRIST].position = pose.Root.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_WRIST].rotation = pose.Root.Rotation;

            // Thumb
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL].position = pose.Thumb.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL].rotation = pose.Thumb.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL].position = pose.Thumb.MiddleData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL].rotation = pose.Thumb.MiddleData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_DISTAL].position = pose.Thumb.TopData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_DISTAL].rotation = pose.Thumb.TopData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_TIP].position = pose.Thumb.TopData.Position +
                                                                        pose.Thumb.TopData.Rotation *
                                                                        s_fingerTips[
                                                                            (int)XrFinger.XR_HAND_FINGER_THUMB];
            joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_TIP].rotation = pose.Thumb.TopData.Rotation;

            // Index
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_METACARPAL].position = pose.Index.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_METACARPAL].rotation = pose.Index.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL].position = pose.Index.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL].rotation = pose.Index.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_INTERMEDIATE].position = pose.Index.MiddleData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_INTERMEDIATE].rotation = pose.Index.MiddleData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL].position = pose.Index.TopData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL].rotation = pose.Index.TopData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_TIP].position = pose.Index.TopData.Position +
                                                                        pose.Index.TopData.Rotation *
                                                                        s_fingerTips[
                                                                            (int)XrFinger.XR_HAND_FINGER_INDEX];
            joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_TIP].rotation = pose.Index.TopData.Rotation;

            // Middle
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_METACARPAL].position = pose.Middle.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_METACARPAL].rotation = pose.Middle.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL].position = pose.Middle.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL].rotation = pose.Middle.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_INTERMEDIATE].position = pose.Middle.MiddleData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_INTERMEDIATE].rotation = pose.Middle.MiddleData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_DISTAL].position = pose.Middle.TopData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_DISTAL].rotation = pose.Middle.TopData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_TIP].position = pose.Middle.TopData.Position +
                                                                         pose.Middle.TopData.Rotation *
                                                                         s_fingerTips[
                                                                             (int)XrFinger.XR_HAND_FINGER_MIDDLE];
            joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_TIP].rotation = pose.Middle.TopData.Rotation;

            // Ring
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_METACARPAL].position = pose.Ring.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_METACARPAL].rotation = pose.Ring.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_PROXIMAL].position = pose.Ring.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_PROXIMAL].rotation = pose.Ring.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_INTERMEDIATE].position = pose.Ring.MiddleData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_INTERMEDIATE].rotation = pose.Ring.MiddleData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_DISTAL].position = pose.Ring.TopData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_DISTAL].rotation = pose.Ring.TopData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_TIP].position = pose.Ring.TopData.Position +
                                                                       pose.Ring.TopData.Rotation *
                                                                       s_fingerTips[
                                                                           (int)XrFinger.XR_HAND_FINGER_RING];
            joints[(int)XrHandJoint.XR_HAND_JOINT_RING_TIP].rotation = pose.Ring.TopData.Rotation;

            // Pinky
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_METACARPAL].position = pose.Pinky.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_METACARPAL].rotation = pose.Pinky.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_PROXIMAL].position = pose.Pinky.BaseData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_PROXIMAL].rotation = pose.Pinky.BaseData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_INTERMEDIATE].position = pose.Pinky.MiddleData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_INTERMEDIATE].rotation = pose.Pinky.MiddleData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_DISTAL].position = pose.Pinky.TopData.Position;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_DISTAL].rotation = pose.Pinky.TopData.Rotation;
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_TIP].position = pose.Pinky.TopData.Position +
                                                                         pose.Pinky.TopData.Rotation *
                                                                         s_fingerTips[
                                                                             (int)XrFinger.XR_HAND_FINGER_PINKY];
            joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_TIP].rotation = pose.Pinky.TopData.Rotation;

            // Palm
            joints[(int)XrHandJoint.XR_HAND_JOINT_PALM].position =
                (joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL].position +
                 joints[(int)XrHandJoint.XR_HAND_JOINT_WRIST].position) / 2f;
            joints[(int)XrHandJoint.XR_HAND_JOINT_PALM].rotation =
                Quaternion.AngleAxis(-90f, Vector3.right) * pose.Palm.Rotation;
        }

        private static readonly Vector3[] s_fingerTips =
        {
            new Vector3(0f, 0f, 0.0301627349f), // Thumb
            new Vector3(0f, 0f, 0.0219157897f), // Index
            new Vector3(0f, 0f, 0.0246319901f), // Middle
            new Vector3(0f, 0f, 0.0232594f), // Ring
            new Vector3(0f, 0f, 0.02121437f), // Pinky
        };
    }
}