/******************************************************************************
 * File: SpacesHand.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html", false)]
    [DisallowMultipleComponent]
    public sealed partial class SpacesHand : ARTrackable<XRTrackedHand, SpacesHand>
    {
        private readonly Pose[] _emptyJoints = Enumerable.Repeat(new Pose(), (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT).ToArray();
        private Pose[] _rawJoints = Enumerable.Repeat(new Pose(), (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT).ToArray();
        private Joint[] _cachedTransformedJoints;
        private int _gesture;
        private float _gestureRatio;
        private float _flipRatio;
        private Transform _arSessionTransform;
        internal Pose[] RawJoints => trackingState != TrackingState.None ? _rawJoints : _emptyJoints;
        internal int RawGesture => trackingState != TrackingState.None ? _gesture : -1;
        internal float RawGestureRatio => trackingState != TrackingState.None ? _gestureRatio : 0f;
        internal float RawFlipRatio => trackingState != TrackingState.None ? _flipRatio : 0f;
        public Pose Pose => sessionRelativeData.pose;
        public bool IsLeft { get; private set; }

        public Joint[] Joints
        {
            get
            {
                _arSessionTransform ??= OriginLocationUtility.GetOriginTransform();
                _cachedTransformedJoints ??= _rawJoints.Select((pose, index) => new Joint((JointType)index, new Pose(_arSessionTransform.TransformPoint(pose.position), _arSessionTransform.rotation * pose.rotation))).ToArray();
                return _cachedTransformedJoints;
            }
        }

        public Gesture CurrentGesture => new Gesture((GestureType)RawGesture, RawGestureRatio, RawFlipRatio);

        internal void UpdateHandData(XRHandTrackingSubsystem subsystem)
        {
            IsLeft = subsystem.LeftHand.trackableId == trackableId;
            subsystem.GetJoints(trackableId, ref _rawJoints);
            _cachedTransformedJoints = null;
            subsystem.GetGestureData(trackableId, ref _gesture, ref _gestureRatio, ref _flipRatio);
        }
    }
}
