// /******************************************************************************
//  * File: HandPoseMask.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;
using QCHT.Interactions.Core;

namespace QCHT.Interactions.Hands
{
    public enum HandMaskState
    {
        Required,
        Free
    }
    
    [Serializable]
    public struct HandMask
    {
        public HandMaskState Thumb;
        public HandMaskState Index;
        public HandMaskState Middle;
        public HandMaskState Ring;
        public HandMaskState Pinky;

        public HandMaskState GetMaskStateForFinger(XrFinger id)
        {
            return id switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => Thumb,
                XrFinger.XR_HAND_FINGER_INDEX => Index,
                XrFinger.XR_HAND_FINGER_MIDDLE => Middle,
                XrFinger.XR_HAND_FINGER_RING => Ring,
                XrFinger.XR_HAND_FINGER_PINKY => Pinky,
                _ => HandMaskState.Free
            };
        }
    }
    
    [CreateAssetMenu(menuName = "QCHT/Interactions/HandPoseMask")]
    public sealed class HandPoseMask : ScriptableObject
    {
        public HandMaskState Thumb;
        public HandMaskState Index;
        public HandMaskState Middle;
        public HandMaskState Ring;
        public HandMaskState Pinky;

        public HandMaskState GetMaskStateForFinger(XrFinger id)
        {
            return id switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => Thumb,
                XrFinger.XR_HAND_FINGER_INDEX => Index,
                XrFinger.XR_HAND_FINGER_MIDDLE => Middle,
                XrFinger.XR_HAND_FINGER_RING => Ring,
                XrFinger.XR_HAND_FINGER_PINKY => Pinky,
                _ => HandMaskState.Free
            };
        }
    }

    public static class HandPoseMaskExtensions
    {
        public static HandMask ToHandMask(this HandPoseMask handPoseMask) => new HandMask()
        {
            Thumb = handPoseMask.Thumb,
            Index = handPoseMask.Index,
            Middle = handPoseMask.Middle,
            Ring = handPoseMask.Ring,
            Pinky = handPoseMask.Pinky,
        };
    }
}