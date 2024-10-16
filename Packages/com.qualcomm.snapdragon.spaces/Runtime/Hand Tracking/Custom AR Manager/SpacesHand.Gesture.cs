/******************************************************************************
 * File: SpacesHand.Gesture.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;

namespace Qualcomm.Snapdragon.Spaces
{
    public sealed partial class SpacesHand
    {
        public class Gesture
        {
            public GestureType Type;
            public float GestureRatio;
            public float FlipRatio;

            public Gesture(GestureType gestureType, float gestureRatio, float flipRatio)
            {
                Type = gestureType;
                GestureRatio = gestureRatio;
                FlipRatio = flipRatio;
            }
        }

        public enum GestureType
        {
            UNKNOWN = -1,
            OPEN_HAND = 0,

            [Obsolete("The FLIP gesture is not used and will be removed in the future.", false)]
            FLIP = 1,
            GRAB = 2,

            [Obsolete("The UP gesture is not used and will be removed in the future.", false)]
            UP = 3,

            [Obsolete("The DOWN gesture is not used and will be removed in the future.", false)]
            DOWN = 4,

            [Obsolete("The SWIPE gesture is not used and will be removed in the future.", false)]
            SWIPE = 5,

            [Obsolete("The SWIPE_OUT gesture is not used and will be removed in the future.", false)]
            SWIPE_OUT = 6,
            PINCH = 7,

            [Obsolete("The POINT gesture is not used and will be removed in the future.", false)]
            POINT = 8,

            [Obsolete("The VICTORY gesture is not used and will be removed in the future.", false)]
            VICTORY = 9,

            [Obsolete("The CALL gesture is not used and will be removed in the future.", false)]
            CALL = 10,

            [Obsolete("The METAL gesture is not used and will be removed in the future.", false)]
            METAL = 11
        }
    }
}
