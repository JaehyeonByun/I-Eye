// /******************************************************************************
//  * File: HandExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using QCHT.Interactions.Core;
using Hand = QCHT.Interactions.Core.XRHandTrackingSubsystem.Hand;

namespace QCHT.Interactions.Extensions
{
    public static class HandExtensions
    {
        private static readonly XrHandJoint[] s_fingerTip =
        {
            XrHandJoint.XR_HAND_JOINT_THUMB_TIP,
            XrHandJoint.XR_HAND_JOINT_INDEX_TIP,
            XrHandJoint.XR_HAND_JOINT_MIDDLE_TIP,
            XrHandJoint.XR_HAND_JOINT_RING_TIP,
            XrHandJoint.XR_HAND_JOINT_LITTLE_TIP
        };

        private static readonly XrHandJoint[][] s_fingerJoints =
        {
            new[]
            {
                // Thumb
                XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL,
                XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL,
                XrHandJoint.XR_HAND_JOINT_THUMB_DISTAL,
                XrHandJoint.XR_HAND_JOINT_THUMB_TIP,
            },
            new[]
            {
                // Index
                XrHandJoint.XR_HAND_JOINT_INDEX_METACARPAL,
                XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL,
                XrHandJoint.XR_HAND_JOINT_INDEX_INTERMEDIATE,
                XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL,
                XrHandJoint.XR_HAND_JOINT_INDEX_TIP,
            },
            new[] // Middle
            {
                XrHandJoint.XR_HAND_JOINT_MIDDLE_METACARPAL,
                XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL,
                XrHandJoint.XR_HAND_JOINT_MIDDLE_INTERMEDIATE,
                XrHandJoint.XR_HAND_JOINT_MIDDLE_DISTAL,
                XrHandJoint.XR_HAND_JOINT_MIDDLE_TIP,
            },
            new[] // Ring
            {
                XrHandJoint.XR_HAND_JOINT_RING_METACARPAL,
                XrHandJoint.XR_HAND_JOINT_RING_PROXIMAL,
                XrHandJoint.XR_HAND_JOINT_RING_INTERMEDIATE,
                XrHandJoint.XR_HAND_JOINT_RING_DISTAL,
                XrHandJoint.XR_HAND_JOINT_RING_TIP,
            },
            new[] // Pinky
            {
                XrHandJoint.XR_HAND_JOINT_LITTLE_METACARPAL,
                XrHandJoint.XR_HAND_JOINT_LITTLE_PROXIMAL,
                XrHandJoint.XR_HAND_JOINT_LITTLE_INTERMEDIATE,
                XrHandJoint.XR_HAND_JOINT_LITTLE_DISTAL,
                XrHandJoint.XR_HAND_JOINT_LITTLE_TIP,
            }
        };

        /// <summary>
        /// Max distance from thumb tip to finger tip
        /// Based on the open hand pose as reference with scale of 1f
        /// </summary>
        private static readonly float[] s_maxDistanceThumbTipToFingerTip =
        {
            0f, // THUMB
            0.15f, // INDEX
            0.19f, // MIDDLE
            0.22f, // RING
            0.24f, // PINKY
        };

        /// <summary>
        /// Min distance from each finger tip to finger proximal
        /// Based fully curled fingers as reference with scale of 1f
        /// </summary>
        private static readonly float[] s_minDistanceFingerTipToProximal =
        {
            0.075f, // THUMB
            0.025f, // INDEX
            0.032f, // MIDDLE
            0.026f, // RING
            0.02f, // PINKY
        };

        /// <summary>
        /// Max distance from each finger tip to finger proximal
        /// Based on the open hand pose as reference with scale of 1f
        /// </summary>
        private static readonly float[] s_maxDistanceFingerTipToProximal =
        {
            0.114f, // THUMB
            0.078f, // INDEX
            0.089f, // MIDDLE
            0.083f, // RING
            0.067f, // PINKY
        };

        /// <summary>
        /// Min adduction angle of each finger to an other
        /// In degree
        /// </summary>
        private static readonly float[] s_minAngleAbduction =
        {
            0.44f, // THUMB
            7.03f, // INDEX
            7.25f, // MIDDLE
            6.33f // RING
        };

        /// <summary>
        /// Max abduction angle of each finger to an other
        /// In degree
        /// </summary>
        private static readonly float[] s_maxAngleAbduction =
        {
            63.15f, // THUMB
            37.16f, // INDEX
            28.61f, // MIDDLE
            36.33f // RING
        };

        /// <summary>
        /// Returns the opposition value of a finger from the thumb.
        /// It corresponds to the distance between the thumb tip and the performed finger tip.
        /// The opposition value of the thumb will therefore always return 0.
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger id to perform calculation</param>
        /// <returns>The distance between thumb tip and finger tip</returns>
        public static float GetFingerOppositionValue(this ref Hand hand, XrFinger finger)
        {
            if (finger <= XrFinger.XR_HAND_FINGER_THUMB || finger >= XrFinger.XR_HAND_FINGER_MAX)
                return 0f;

            var thumbTip = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_THUMB_TIP).position;
            var fingerTip = hand.GetHandJoint(s_fingerTip[(int) finger]).position;
            var distance = Vector3.Magnitude(fingerTip - thumbTip);
            return distance;
        }

        /// <summary>
        /// Returns the distance from the thumb tip to closest other finger tip 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <returns></returns>
        public static float GetMinDistToThumbTip(this ref Hand hand)
        {
            var minDist = float.MaxValue;
            for (var i = 1; i < s_fingerTip.Length; i++) // skip thumb
            {
                var dist = GetFingerOppositionValue(ref hand, (XrFinger) i);
                if (dist < minDist) minDist = dist;
            }

            return minDist;
        }

        /// <summary>
        /// Returns the flexion degree of a finger.
        /// It corresponds to the signed angle value between the palm and the finger first bone. 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger id to perform calculation</param>
        /// <returns>0f flexion opened - 1f flexion closed</returns>
        public static float GetFingerFlexionValue(this ref Hand hand, XrFinger finger)
        {
            if (finger < XrFinger.XR_HAND_FINGER_THUMB || finger >= XrFinger.XR_HAND_FINGER_MAX)
                return 0f;

            var joints = s_fingerJoints[(int) finger];
            var bone1 = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_WRIST).position -
                        hand.GetHandJoint(joints[0]).position;
            var bone2 = hand.GetHandJoint(joints[2]).position - hand.GetHandJoint(joints[1]).position;
            var angle = Vector3.SignedAngle(bone1, bone2, hand.GetHandJoint(joints[1]).rotation * Vector3.right);
            if (angle < 0f) angle += 360f;
            return Mathf.Abs((angle - 180f) / 90f);
        }

        /// <summary>
        /// Returns the curl value of a finger.
        /// It corresponds to the distal to tip composed flexion value.
        /// For example a fist gestures occurs when all the finger flexion values and the curl values return 1f. 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger id to perform calculation</param>
        /// <returns>0f when distal/tip rotation is open and 1f when distal/tip rotation is closed</returns>
        public static float GetFingerCurlValue(this ref Hand hand, XrFinger finger)
        {
            if (finger < XrFinger.XR_HAND_FINGER_THUMB || finger >= XrFinger.XR_HAND_FINGER_MAX)
                return 0f;

            var joints = s_fingerJoints[(int) finger];

            // First bone joint angle
            var pos1 = hand.GetHandJoint(joints[0]).position;
            var pos2 = hand.GetHandJoint(joints[1]).position;
            var pos3 = hand.GetHandJoint(joints[2]).position;
            var rot1 = hand.GetHandJoint(joints[1]).rotation;
            var bone1 = pos1 - pos2;
            var bone2 = pos3 - pos2;
            var a1 = Vector3.SignedAngle(bone1, bone2, rot1 * Vector3.right);
            if (a1 < 0f) a1 += 360f;
            var sum = a1;

            if (finger > XrFinger.XR_HAND_FINGER_THUMB)
            {
                // Second bone joint angle
                var pos4 = hand.GetHandJoint(joints[3]).position;
                var rot2 = hand.GetHandJoint(joints[2]).rotation;
                var bone3 = pos4 - pos3;
                bone2 = -bone2; // reflected bone2
                var a2 = Vector3.SignedAngle(-bone2, bone3, rot2 * Vector3.right);
                if (a2 < 0f) a2 += 360f;
                sum += a2;
                sum *= .5f;
            }

            return 1f - Mathf.Clamp01(Mathf.Abs((sum - 180f) / 90f));
        }

        /// <summary>
        /// Gets the curl value base on the distance from finger tip to its proximal joint 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">Finger id which tip to proximal distance will be calculated.</param>
        /// <returns></returns>
        public static float GetFingerCurlDistanceValue(this ref Hand hand, XrFinger finger)
        {
            if (finger < XrFinger.XR_HAND_FINGER_THUMB || finger >= XrFinger.XR_HAND_FINGER_MAX)
                return 0f;

            var joints = s_fingerJoints[(int) finger];

            Vector3 fingerTip;
            Vector3 fingerProximal;

            if (finger == XrFinger.XR_HAND_FINGER_THUMB)
            {
                fingerTip = hand.GetHandJoint(joints[3]).position;
                fingerProximal = hand.GetHandJoint(joints[0]).position;
            }
            else
            {
                fingerTip = hand.GetHandJoint(joints[4]).position;
                fingerProximal = hand.GetHandJoint(joints[1]).position;
            }

            return Vector3.Distance(fingerTip, fingerProximal);
        }

        /// <summary>
        /// Returns the abduction value of a finger.
        /// It corresponds to the angle value between the finger and the next finger.
        /// For example the abduction value of the index will return the angle between the base bone of the index and the base bone of the middle finger.
        /// The abduction value of the pinky finger will always return 0. 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger id to perform calculation</param>
        /// <returns>Signed angle of abduction, 0f for adduction >0f for abduction</returns>
        public static float GetFingerAbductionValue(this ref Hand hand, XrFinger finger)
        {
            if (finger < XrFinger.XR_HAND_FINGER_THUMB || finger >= XrFinger.XR_HAND_FINGER_PINKY)
                return 0f;

            var nextFingerId = finger + 1;
            var fJoints = s_fingerJoints[(int) finger];
            var nfJoints = s_fingerJoints[(int) nextFingerId];
            var fBase = hand.GetHandJoint(fJoints[0]).position;
            var nfBase = hand.GetHandJoint(nfJoints[0]).position;
            var fTip = hand.GetHandJoint(fJoints[fJoints.Length - 1]).position;
            var nfTip = hand.GetHandJoint(nfJoints[nfJoints.Length - 1]).position;
            var baseMidPoint = Vector3.Lerp(fBase, nfBase, 0.5f);

            Vector3 n1;

            if (finger == XrFinger.XR_HAND_FINGER_THUMB)
            {
                n1 = fTip - fBase;
            }
            else
            {
                n1 = fTip - baseMidPoint;
            }

            var n2 = nfTip - baseMidPoint;
            var axis = Vector3.Cross(n1, n2);
            return Vector3.SignedAngle(n1, n2, axis);
        }

        /// <summary>
        /// Gets finger pinching value.
        /// From 0f opened to 1f pinching
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger to check</param>
        /// <returns>Pinching value</returns>
        public static float GetFingerPinching(this ref Hand hand, XrFinger finger)
        {
            var distance = hand.GetFingerOppositionValue(finger);
            var maxDistance = s_maxDistanceThumbTipToFingerTip[(int) finger] * hand.Scale;
            return maxDistance <= Mathf.Epsilon ? 0f : 1f - Mathf.Clamp01(distance / maxDistance);
        }

        /// <summary>
        /// Gets the finger flexion value.
        /// From 0f opened to 1f flexing
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger to check</param>
        /// <returns>Flexing value</returns>
        public static float GetFingerFlexing(this ref Hand hand, XrFinger finger)
        {
            var flexion = hand.GetFingerFlexionValue(finger);
            return Mathf.Clamp01(flexion);
        }

        /// <summary>
        /// Gets the finger curling value.
        /// from 0f opened to 1f curling
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger to check</param>
        /// <returns>Curling value</returns>
        public static float GetFingerCurling(this Hand hand, XrFinger finger)
        {
            var distance = hand.GetFingerCurlDistanceValue(finger);
            var minDist = s_minDistanceFingerTipToProximal[(int) finger] * hand.Scale;
            var maxDist = s_maxDistanceFingerTipToProximal[(int) finger] * hand.Scale;
            if (maxDist - minDist <= Mathf.Epsilon)
                return 0f;
            return 1f - Mathf.Clamp01((distance - minDist) / (maxDist - minDist));
        }

        /// <summary>
        /// Gets the finger abducting value
        /// From 0f closed to 1f abducting 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="finger">The finger to check</param>
        /// <returns>Abducting value</returns>
        public static float GetFingerAbducting(this ref Hand hand, XrFinger finger)
        {
            var angle = hand.GetFingerAbductionValue(finger);
            return (angle - s_minAngleAbduction[(int) finger]) / (s_maxAngleAbduction[(int) finger] -
                                                                  s_minAngleAbduction[(int) finger]);
        }
    }
}