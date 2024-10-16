// /******************************************************************************
//  * File: HandDriver.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using QCHT.Interactions.Core;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Hands
{
    public class HandDriver : MonoBehaviour, IHandPoseReceiver, IHandPokeReceiver
    {
        [field: SerializeField] public XrHandedness Handedness { get; private set; }

        [Header("Joints")]
        [SerializeField] private HandJointUpdater rootPart;

        [Space]

        // Thumb
        [SerializeField] private HandJointUpdater thumbBase;
        [SerializeField] private HandJointUpdater thumbMiddle;
        [SerializeField] private HandJointUpdater thumbTop;
        [SerializeField] private Transform thumbTip;

        [Space]

        // Index
        [SerializeField] private HandJointUpdater indexBase;
        [SerializeField] private HandJointUpdater indexMiddle;
        [SerializeField] private HandJointUpdater indexTop;
        [SerializeField] private Transform indexTip;
        
        [Space]

        // Middle
        [SerializeField] private HandJointUpdater middleBase;
        [SerializeField] private HandJointUpdater middleMiddle;
        [SerializeField] private HandJointUpdater middleTop;
        [SerializeField] private Transform middleTip;
        
        [Space]

        // Ring
        [SerializeField] private HandJointUpdater ringBase;
        [SerializeField] private HandJointUpdater ringMiddle;
        [SerializeField] private HandJointUpdater ringTop;
        [SerializeField] private Transform ringTip;
        
        [Space]

        // Pinky
        [SerializeField] private HandJointUpdater pinkyBase;
        [SerializeField] private HandJointUpdater pinkyMiddle;
        [SerializeField] private HandJointUpdater pinkyTop;
        [SerializeField] private Transform pinkyTip;
        
        // IHandPokeReceiver
        public Vector3? PokePoint { get; set; }
        
        // IHandPoseReceiver
        public Pose? RootPoseOverride { get; set; }
        public HandData? HandPoseOverride { get; set; }
        public HandMask? HandPoseMask { get; set; }

        protected Transform _origin;
        protected GameObject _cameraOffset; 
        protected XRHandTrackingSubsystem _subsystem;

        protected void OnEnable()
        {
            _origin = XROriginUtility.GetOriginTransform();
            _cameraOffset = XROriginUtility.GetCameraFloorOffsetObject();
            
            _subsystem ??= XRHandTrackingSubsystem.GetSubsystemInManager();
            
            if (_subsystem != null)
            {
                _subsystem.OnHandsUpdated += OnHandsUpdated;
            }
        }

        protected void OnDisable()
        {
            if (_subsystem != null)
            {
                _subsystem.OnHandsUpdated -= OnHandsUpdated;
                _subsystem = null;
            }
        }

        protected void OnHandsUpdated(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            // Update only joint transforms before rendering
            // as we don't depends on hand joints transform poses for interactions
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
                return;
            
            var hand = Handedness == XrHandedness.XR_HAND_LEFT ? _subsystem.LeftHand : _subsystem.RightHand;
            
            if (PokePoint.HasValue)
            {
                UpdateHandFromConstraintPoint(hand, PokePoint.Value, XrFinger.XR_HAND_FINGER_INDEX);
            }
            else
            {
                UpdateHandFree(hand);
            }
        }

        private void UpdateHandFromConstraintPoint(XRHandTrackingSubsystem.Hand hand, Vector3 point, XrFinger finger)
        {
            var tip = GetTipForFinger(finger);
            var (bottom, middle, top) = GetJointUpdatersForFinger(finger);
            var fingerData = GetFingerDataFromJoints(hand.Space, hand.Joints, finger);
            
            // Update finger
            fingerData.TopData.Position = point - fingerData.TopData.Rotation * Vector3.Scale(tip.transform.localPosition, tip.transform.lossyScale);
            fingerData.TopData.UpdatePosition = true;

            fingerData.MiddleData.Position = fingerData.TopData.Position - fingerData.MiddleData.Rotation * Vector3.Scale(top.transform.localPosition, top.transform.lossyScale);
            fingerData.MiddleData.UpdatePosition = true;

            fingerData.BaseData.Position = fingerData.MiddleData.Position - fingerData.BaseData.Rotation * Vector3.Scale(middle.transform.localPosition, middle.transform.lossyScale);
            fingerData.BaseData.UpdatePosition = true;
            
            // Calculate root pose using finger joint above
            var rootData = BoneData.DefaultRoot;
            rootData.UpdatePosition = true;
            rootData.UpdateRotation = true;
            rootData.Rotation = _origin.rotation * hand.Joints[(int) XrHandJoint.XR_HAND_JOINT_WRIST].rotation;
            rootData.Position = fingerData.BaseData.Position - rootData.Rotation * Vector3.Scale(bottom.transform.localPosition, bottom.transform.lossyScale);
            rootPart.UpdateJoint(hand.Space, rootData);
            
            if (bottom) bottom.UpdateJoint(hand.Space, fingerData.BaseData);
            if (middle) middle.UpdateJoint(hand.Space, fingerData.MiddleData);
            if (top) top.UpdateJoint(hand.Space, fingerData.TopData);

            for (var i = 0; i < (int) XrFinger.XR_HAND_FINGER_MAX; i++)
            {
                if (i == (int) finger) continue;
                (bottom, middle, top) = GetJointUpdatersForFinger((XrFinger) i);
                fingerData = GetFingerDataFromJoints(hand.Space, hand.Joints, (XrFinger) i);
                if (bottom) bottom.UpdateJoint(hand.Space, fingerData.BaseData);
                if (middle) middle.UpdateJoint(hand.Space, fingerData.MiddleData);
                if (top) top.UpdateJoint(hand.Space, fingerData.TopData);
            }
        }

        private void UpdateHandFree(XRHandTrackingSubsystem.Hand hand)
        {
            // Update root
            var rootBone = BoneData.DefaultRoot;
            var rootSpace = hand.Space;

            if (RootPoseOverride == null)
            {
                rootBone.UpdatePosition = true;
                rootBone.UpdateRotation = true;
                rootBone.Position = Vector3.zero;
                rootBone.Rotation = Quaternion.identity;
                rootSpace = XrSpace.XR_HAND_LOCAL;
            }
            else
            {
                rootBone.UpdatePosition = true;
                rootBone.UpdateRotation = true;
                rootBone.Position = RootPoseOverride.Value.position;
                rootBone.Rotation = RootPoseOverride.Value.rotation;
            }

            rootPart.transform.localScale = hand.Scale * Vector3.one;
            rootPart.UpdateJoint(rootSpace, rootBone);

            // Update fingers
            var space = hand.Space;
            var joints = hand.Joints;
            
            UpdateFreeFingerJoints(space, joints, XrFinger.XR_HAND_FINGER_THUMB);
            UpdateFreeFingerJoints(space, joints, XrFinger.XR_HAND_FINGER_INDEX);
            UpdateFreeFingerJoints(space, joints, XrFinger.XR_HAND_FINGER_MIDDLE);
            UpdateFreeFingerJoints(space, joints, XrFinger.XR_HAND_FINGER_RING);
            UpdateFreeFingerJoints(space, joints, XrFinger.XR_HAND_FINGER_PINKY);
        }
        

        private void UpdateFreeFingerJoints(XrSpace space, IReadOnlyList<Pose> joints, XrFinger id)
        {
            var maskState = GetMaskStateForFinger(id);
            var (bottom, middle, top) = GetJointUpdatersForFinger(id);
            var fingerData = maskState == HandMaskState.Required
                ? GetFingerDataFromHandPoseOverride(id)
                : GetFingerDataFromJoints(space, joints, id);
            
            if (HandPoseOverride != null)
            {
                space = maskState == HandMaskState.Required ? HandPoseOverride.Value.Space : space;
            }
            
            if (bottom) bottom.UpdateJoint(space, fingerData.BaseData);
            if (middle) middle.UpdateJoint(space, fingerData.MiddleData);
            if (top) top.UpdateJoint(space, fingerData.TopData);
        }

        private HandMaskState GetMaskStateForFinger(XrFinger id)
        {
            if (HandPoseMask == null)
                return HandMaskState.Free;

            return id switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => HandPoseMask.Value.Thumb,
                XrFinger.XR_HAND_FINGER_INDEX => HandPoseMask.Value.Index,
                XrFinger.XR_HAND_FINGER_MIDDLE => HandPoseMask.Value.Middle,
                XrFinger.XR_HAND_FINGER_RING => HandPoseMask.Value.Ring,
                XrFinger.XR_HAND_FINGER_PINKY => HandPoseMask.Value.Pinky,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private FingerData GetFingerDataFromHandPoseOverride(XrFinger id)
        {
            if (HandPoseOverride == null)
                return FingerData.Default;

            return id switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => HandPoseOverride.Value.Thumb,
                XrFinger.XR_HAND_FINGER_INDEX => HandPoseOverride.Value.Index,
                XrFinger.XR_HAND_FINGER_MIDDLE => HandPoseOverride.Value.Middle,
                XrFinger.XR_HAND_FINGER_RING => HandPoseOverride.Value.Ring,
                XrFinger.XR_HAND_FINGER_PINKY => HandPoseOverride.Value.Pinky,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private FingerData GetFingerDataFromJoints(XrSpace space, IReadOnlyList<Pose> joints, XrFinger finger)
        {
            var fingerData = finger == XrFinger.XR_HAND_FINGER_THUMB ? FingerData.DefaultThumb : FingerData.Default;
            var (baseJoint, middleJoint, topJoint) = GetJointsForFinger(finger);

            fingerData.BaseData.Position = joints[(int) baseJoint].position;
            fingerData.BaseData.Rotation = joints[(int) baseJoint].rotation;

            fingerData.MiddleData.Position = joints[(int) middleJoint].position;
            fingerData.MiddleData.Rotation = joints[(int) middleJoint].rotation;

            fingerData.TopData.Position = joints[(int) topJoint].position;
            fingerData.TopData.Rotation = joints[(int) topJoint].rotation;

            if (space == XrSpace.XR_HAND_WORLD)
            {
                if (_origin != null)
                {
                    var originRotation = _origin.rotation;

                    fingerData.BaseData.Position = _origin.TransformPoint(fingerData.BaseData.Position);
                    fingerData.BaseData.Rotation = originRotation * fingerData.BaseData.Rotation;

                    fingerData.MiddleData.Position = _origin.TransformPoint(fingerData.MiddleData.Position);
                    fingerData.MiddleData.Rotation = originRotation * fingerData.MiddleData.Rotation;

                    fingerData.TopData.Position = _origin.TransformPoint(fingerData.TopData.Position);
                    fingerData.TopData.Rotation = originRotation * fingerData.TopData.Rotation;
                }
                
                if (_cameraOffset != null)
                {
                    var camOffset = _cameraOffset.transform.localPosition;
                    fingerData.BaseData.Position += camOffset;
                    fingerData.MiddleData.Position += camOffset;
                    fingerData.TopData.Position += camOffset;
                }
            }
            
            return fingerData;
        }

        private (HandJointUpdater, HandJointUpdater, HandJointUpdater) GetJointUpdatersForFinger(XrFinger id) =>
            id switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => (thumbBase, thumbMiddle, thumbTop),
                XrFinger.XR_HAND_FINGER_INDEX => (indexBase, indexMiddle, indexTop),
                XrFinger.XR_HAND_FINGER_MIDDLE => (middleBase, middleMiddle, middleTop),
                XrFinger.XR_HAND_FINGER_RING => (ringBase, ringMiddle, ringTop),
                XrFinger.XR_HAND_FINGER_PINKY => (pinkyBase, pinkyMiddle, pinkyTop),
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };

        private Transform GetTipForFinger(XrFinger id) =>
            id switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => thumbTip,
                XrFinger.XR_HAND_FINGER_INDEX => indexTip,
                XrFinger.XR_HAND_FINGER_MIDDLE => middleTip,
                XrFinger.XR_HAND_FINGER_RING => ringTip,
                XrFinger.XR_HAND_FINGER_PINKY => pinkyTip,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };


        private static (XrHandJoint, XrHandJoint, XrHandJoint) GetJointsForFinger(XrFinger id) =>
            id switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => (
                    XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL,
                    XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL,
                    XrHandJoint.XR_HAND_JOINT_THUMB_DISTAL),

                XrFinger.XR_HAND_FINGER_INDEX => (
                    XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL,
                    XrHandJoint.XR_HAND_JOINT_INDEX_INTERMEDIATE,
                    XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL),

                XrFinger.XR_HAND_FINGER_MIDDLE => (
                    XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL,
                    XrHandJoint.XR_HAND_JOINT_MIDDLE_INTERMEDIATE,
                    XrHandJoint.XR_HAND_JOINT_MIDDLE_DISTAL),

                XrFinger.XR_HAND_FINGER_RING => (
                    XrHandJoint.XR_HAND_JOINT_RING_PROXIMAL,
                    XrHandJoint.XR_HAND_JOINT_RING_INTERMEDIATE,
                    XrHandJoint.XR_HAND_JOINT_RING_DISTAL),

                XrFinger.XR_HAND_FINGER_PINKY => (
                    XrHandJoint.XR_HAND_JOINT_LITTLE_PROXIMAL,
                    XrHandJoint.XR_HAND_JOINT_LITTLE_INTERMEDIATE,
                    XrHandJoint.XR_HAND_JOINT_LITTLE_DISTAL),

                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"WRIST : [ {rootPart} ]\n");
            sb.Append($"THUMB PROXIMAL : [ {thumbMiddle} ]\n");
            sb.Append($"THUMB DISTAL : [ {thumbTop} ]\n");
            sb.Append($"INDEX PROXIMAL : [ {indexBase} ]\n");
            sb.Append($"INDEX INTERMEDIATE : [ {indexMiddle} ]\n");
            sb.Append($"INDEX DISTAL : [ {indexTop} ]\n");
            sb.Append($"MIDDLE PROXIMAL : [ {middleBase} ]\n");
            sb.Append($"MIDDLE INTERMEDIATE : [ {middleMiddle} ]\n");
            sb.Append($"MIDDLE DISTAL : [ {middleTop} ]\n");
            sb.Append($"RING PROXIMAL : [ {ringBase} ]\n");
            sb.Append($"RING INTERMEDIATE : [ {ringMiddle} ]\n");
            sb.Append($"RING DISTAL : [ {ringTop} ]\n");
            sb.Append($"PINKY PROXIMAL : [ {pinkyBase} ]\n");
            sb.Append($"PINKY INTERMEDIATE : [ {pinkyMiddle} ]\n");
            sb.Append($"PINKY DISTAL : [ {pinkyTop} ]\n");
            return sb.ToString();
        }
    }
}