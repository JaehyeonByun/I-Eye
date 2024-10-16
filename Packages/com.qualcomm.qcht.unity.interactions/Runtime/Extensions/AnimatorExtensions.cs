// /******************************************************************************
//  * File: AnimatorExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class AnimatorExtensions
    {
        public static int GetTotalFrameCount(this Animator animator)
        {
            var clipsInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipsInfo != null && clipsInfo.Length > 0)
            {
                var clipInfo = clipsInfo[0];
                return (int)Mathf.Ceil(clipInfo.clip.length * clipInfo.clip.frameRate);
            }
            return -1;
        }

        public static int GetCurrentFrameIndex(this Animator animator)
        {
            var clipsInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipsInfo != null && clipsInfo.Length > 0)
            {
                var clipInfo = clipsInfo[0];
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                int currentFrame = (int)(clipInfo.clip.length * (stateInfo.normalizedTime % 1) * clipInfo.clip.frameRate);
                return currentFrame;
            }
            return -1;
        }
        
        public static float GetCurrentAnimatorTime(this Animator animator, int layer = 0)
        {
            AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(layer);
            float currentTime = animState.normalizedTime % 1;
            return currentTime;
        }

        public static float GetTotalClipTime(this Animator animator, int layer = 0)
        {
            AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(layer);
            return animState.length;
        }
    }
}

