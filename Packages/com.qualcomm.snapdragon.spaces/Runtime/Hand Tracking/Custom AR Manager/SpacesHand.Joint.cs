/******************************************************************************
 * File: SpacesHand.Joint.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public sealed partial class SpacesHand
    {
        public class Joint
        {
            public JointType Type;
            public Pose Pose;

            public Joint(JointType jointType, Pose pose)
            {
                Type = jointType;
                Pose = pose;
            }
        }

        public enum JointType
        {
            PALM = 0,
            WRIST = 1,
            THUMB_METACARPAL = 2,
            THUMB_PROXIMAL = 3,
            THUMB_DISTAL = 4,
            THUMB_TIP = 5,
            INDEX_METACARPAL = 6,
            INDEX_PROXIMAL = 7,
            INDEX_INTERMEDIATE = 8,
            INDEX_DISTAL = 9,
            INDEX_TIP = 10,
            MIDDLE_METACARPAL = 11,
            MIDDLE_PROXIMAL = 12,
            MIDDLE_INTERMEDIATE = 13,
            MIDDLE_DISTAL = 14,
            MIDDLE_TIP = 15,
            RING_METACARPAL = 16,
            RING_PROXIMAL = 17,
            RING_INTERMEDIATE = 18,
            RING_DISTAL = 19,
            RING_TIP = 20,
            LITTLE_METACARPAL = 21,
            LITTLE_PROXIMAL = 22,
            LITTLE_INTERMEDIATE = 23,
            LITTLE_DISTAL = 24,
            LITTLE_TIP = 25
        }
    }
}
