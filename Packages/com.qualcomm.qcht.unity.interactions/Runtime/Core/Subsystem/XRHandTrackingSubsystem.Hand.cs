// /******************************************************************************
//  * File: XRHandTrackingSubsystem.Hand.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Linq;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    public partial class XRHandTrackingSubsystem
    {
        /// <summary>
        /// Hand data struct.
        /// </summary>
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
        public partial struct Hand
#pragma warning restore CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
        {
            /// <summary>
            /// Handedness
            /// </summary>
            internal XrHandedness _handedness;
            public XrHandedness Handedness => _handedness;

            /// <summary>
            /// Is hand tracked?
            /// </summary>
            internal bool _isTracked;
            public bool IsTracked => _isTracked;

            /// <summary>
            /// Spaces in which joints data are formatted.
            /// XR_HAND_LOCAL when joint pose is given locally, related to parent joint.
            /// XR_HAND_WORLD when joint pose is given in directly world.  
            /// </summary>
            internal XrSpace _space;
            public XrSpace Space => _space;

            /// <summary>
            /// Joints poses array for one hand.
            /// The size of array is XrHandJoint.XR_HAND_JOINT_MAX = 26 joints. 
            /// </summary>
            internal Pose[] _joints;
            public Pose[] Joints => _joints;

            /// <summary>
            /// Radii for each joint.
            /// The size of array is XrHandJoint.XR_HAND_JOINT_MAX = 26 joints. 
            /// </summary>
            internal float[] _radiuses;
            public float[] Radiuses => _radiuses;

            /// <summary>
            /// Hand scale
            /// Should be clamped between .5 and 1.6.
            /// </summary>
            public float Scale => _radiuses[(int) XrHandJoint.XR_HAND_JOINT_WRIST];

            /// <summary>
            /// Root pose.
            /// With Hand Tracking root corresponds to XrHandJoint.XR_HAND_JOINT_WRIST in origin space. 
            /// </summary>
            internal Pose _root;
            public Pose Root => _root;

            internal Hand(XrHandedness handedness)
            {
                _handedness = handedness;
                _joints = Enumerable.Repeat(Pose.identity, (int) XrHandJoint.XR_HAND_JOINT_MAX).ToArray();
                _radiuses = Enumerable.Repeat(0f, (int) XrHandJoint.XR_HAND_JOINT_MAX).ToArray();
                _root = Pose.identity;
                _isTracked = false;
                _space = XrSpace.XR_HAND_WORLD;

#pragma warning disable CS0612 // Type or member is obsolete
                _gesture = XrHandGesture.XR_HAND_UNKNOWN;
                _gestureRatio = 0f;
                _flipRatio = -1f;
#pragma warning restore CS0612 // Type or member is obsolete
            }

            /// <summary>
            /// Gets hand joint in joints array for given XrHandJoint.
            /// </summary>
            /// <param name="joint"> joints id to retrieve </param>
            /// <returns> Required joint pose. </returns>
            public Pose GetHandJoint(XrHandJoint joint) => _joints[(int) joint];
        }
    }
}