/******************************************************************************
 * File: HandTrackingSubsystem.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;
using Random = UnityEngine.Random;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html", false)]
    internal class HandTrackingSubsystem : XRHandTrackingSubsystem
    {
        private class HandTrackingProvider : Provider
        {
            private HandTrackingFeature _underlyingFeature;
            private Hand _leftHand;
            private Hand _rightHand;

            public override void Start()
            {
                _underlyingFeature = OpenXRSettings.Instance.GetFeature<HandTrackingFeature>();
                if (!FeatureUseCheckUtility.IsFeatureUseable(_underlyingFeature))
                {
#if !UNITY_EDITOR
                    Debug.LogError("Spaces HandTrackingFeature is missing or not enabled.");
#endif
                    Destroy();
                    return;
                }

                _underlyingFeature.TryCreateHandTracking();
                var randomNumber = (ulong)Random.Range(42, ulong.MaxValue);
                _leftHand = new Hand(new XRTrackedHand(new TrackableId(randomNumber, 42)), true);
                _rightHand = new Hand(new XRTrackedHand(new TrackableId(randomNumber, 42 + 1)), false);
            }

            public override void Stop()
            {
                Destroy();
            }

            public override void Destroy()
            {
                if (_underlyingFeature)
                {
                    _underlyingFeature.TryDestroyHandTracking();
                }
            }

            public override XRTrackedHand GetLeftHand()
            {
                return _leftHand != null ? _leftHand.TrackedHand : XRTrackedHand.defaultValue;
            }

            public override XRTrackedHand GetRightHand()
            {
                return _rightHand != null ? _rightHand.TrackedHand : XRTrackedHand.defaultValue;
            }

            public override TrackableChanges<XRTrackedHand> GetChanges(XRTrackedHand defaultHand, Allocator allocator)
            {
                var addedHands = new List<XRTrackedHand>();
                var updatedHands = new List<XRTrackedHand>();
                var removedHands = new List<TrackableId>();
                UpdateTrackableChangesLists(ref _leftHand, ref addedHands, ref updatedHands, ref removedHands);
                UpdateTrackableChangesLists(ref _rightHand, ref addedHands, ref updatedHands, ref removedHands);
                var trackableChanges = TrackableChanges<XRTrackedHand>.CopyFrom(new NativeArray<XRTrackedHand>(addedHands.ToArray(), allocator),
                    new NativeArray<XRTrackedHand>(updatedHands.ToArray(), allocator),
                    new NativeArray<TrackableId>(removedHands.ToArray(), allocator),
                    allocator);
                return trackableChanges;
            }

            public override void GetJoints(TrackableId trackableId, ref Pose[] joints)
            {
                var hand = GetHandByTrackableID(trackableId);
                if (hand != null)
                {
                    joints = hand.Joints;
                }
            }

            public override void GetGestureData(TrackableId trackableId, ref int gesture, ref float gestureRatio, ref float flipRatio)
            {
                var hand = GetHandByTrackableID(trackableId);
                if (hand != null)
                {
                    gesture = hand.Gesture;
                    gestureRatio = hand.GestureRatio;
                    flipRatio = hand.FlipRatio;
                }
            }

            private void UpdateTrackableChangesLists(ref Hand hand, ref List<XRTrackedHand> added, ref List<XRTrackedHand> updated, ref List<TrackableId> removed)
            {
                if (_underlyingFeature && hand != null)
                {
                    var isLeft = hand.IsLeft;
                    var jointsAndTrackingState = _underlyingFeature.TryGetHandTrackingJointsAndTrackingState(isLeft);
                    var currentTrackingState = hand.TrackedHand.trackingState;
                    hand.UpdateTrackedHandPoseAndTrackingState(jointsAndTrackingState.Item1.ElementAtOrDefault((int)XrHandJointEXT.XR_HAND_JOINT_WRIST_EXT), jointsAndTrackingState.Item2);
                    if (jointsAndTrackingState.Item2 == TrackingState.Tracking)
                    {
                        if (currentTrackingState == TrackingState.None)
                        {
                            added.Add(hand.TrackedHand);
                        }
                        else
                        {
                            updated.Add(hand.TrackedHand);
                        }
                    }
                    else
                    {
                        if (currentTrackingState == TrackingState.Tracking)
                        {
                            removed.Add(hand.TrackedHand.trackableId);
                        }

                        return;
                    }

                    hand.Joints = jointsAndTrackingState.Item1.ToArray();
                    var gestureData = _underlyingFeature.TryGetHandGestureData(isLeft);
                    hand.Gesture = gestureData.Item1;
                    hand.GestureRatio = gestureData.Item2;
                    hand.FlipRatio = gestureData.Item3;
                }
                else
                {
                    if (hand != null && hand.TrackedHand.trackableId != TrackableId.invalidId)
                    {
                        removed.Add(hand.TrackedHand.trackableId);
                    }
                }
            }

            private Hand GetHandByTrackableID(TrackableId trackableId)
            {
                if (_leftHand.TrackedHand.trackingState != TrackingState.None && _leftHand.TrackedHand.trackableId == trackableId)
                {
                    return _leftHand;
                }

                if (_rightHand.TrackedHand.trackingState != TrackingState.None && _rightHand.TrackedHand.trackableId == trackableId)
                {
                    return _rightHand;
                }

                return null;
            }
        }

        public const string ID = "Spaces-HandTrackingSubsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRHandTrackingSubsystemDescriptor.Create(new XRHandTrackingSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(HandTrackingProvider),
                subsystemTypeOverride = typeof(HandTrackingSubsystem)
            });
        }
    }
}
