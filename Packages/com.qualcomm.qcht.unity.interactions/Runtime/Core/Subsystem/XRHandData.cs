// /******************************************************************************
//  * File: XRHandData.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

namespace QCHT.Interactions.Core
{
    public enum XrHandedness
    {
        XR_HAND_LEFT = 0,
        XR_HAND_RIGHT = 1
    }

    public enum XrHandGesture
    {
        XR_HAND_UNKNOWN = -1,
        XR_HAND_OPEN_HAND = 0,
        XR_HAND_PINCH = 1,
        XR_HAND_GRAB = 2
    }
    
    public enum XrHandJoint
    {
        XR_HAND_JOINT_PALM = 0,
        XR_HAND_JOINT_WRIST = 1,
        XR_HAND_JOINT_THUMB_METACARPAL = 2,
        XR_HAND_JOINT_THUMB_PROXIMAL = 3,
        XR_HAND_JOINT_THUMB_DISTAL = 4,
        XR_HAND_JOINT_THUMB_TIP = 5,
        XR_HAND_JOINT_INDEX_METACARPAL = 6,
        XR_HAND_JOINT_INDEX_PROXIMAL = 7,
        XR_HAND_JOINT_INDEX_INTERMEDIATE = 8,
        XR_HAND_JOINT_INDEX_DISTAL = 9,
        XR_HAND_JOINT_INDEX_TIP = 10,
        XR_HAND_JOINT_MIDDLE_METACARPAL = 11,
        XR_HAND_JOINT_MIDDLE_PROXIMAL = 12,
        XR_HAND_JOINT_MIDDLE_INTERMEDIATE = 13,
        XR_HAND_JOINT_MIDDLE_DISTAL = 14,
        XR_HAND_JOINT_MIDDLE_TIP = 15,
        XR_HAND_JOINT_RING_METACARPAL = 16,
        XR_HAND_JOINT_RING_PROXIMAL = 17,
        XR_HAND_JOINT_RING_INTERMEDIATE = 18,
        XR_HAND_JOINT_RING_DISTAL = 19,
        XR_HAND_JOINT_RING_TIP = 20,
        XR_HAND_JOINT_LITTLE_METACARPAL = 21,
        XR_HAND_JOINT_LITTLE_PROXIMAL = 22,
        XR_HAND_JOINT_LITTLE_INTERMEDIATE = 23,
        XR_HAND_JOINT_LITTLE_DISTAL = 24,
        XR_HAND_JOINT_LITTLE_TIP = 25,
        XR_HAND_JOINT_MAX = 26
    }

    public enum XrFinger
    {
        XR_HAND_FINGER_THUMB = 0,
        XR_HAND_FINGER_INDEX = 1,
        XR_HAND_FINGER_MIDDLE = 2,
        XR_HAND_FINGER_RING = 3,
        XR_HAND_FINGER_PINKY = 4,
        XR_HAND_FINGER_MAX = 5
    }
    
    public enum XrSpace
    {
        XR_HAND_LOCAL = 0,
        XR_HAND_WORLD = 1
    }
}