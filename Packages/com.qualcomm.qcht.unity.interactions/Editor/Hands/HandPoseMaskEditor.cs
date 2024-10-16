// /******************************************************************************
//  * File: HandPoseMaskEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Editor;
using UnityEditor;

namespace QCHT.Interactions.Hands.Editor
{
    [CustomEditor(typeof(HandPoseMask))]
    public class HandPoseMaskEditor : UnityEditor.Editor
    {
        public static HandPoseMask CreateNewHandPoseMaskAsset()
        {
            var handPoseMask = CreateInstance<HandPoseMask>();
            AssetUtils.CreateAssetInSettingsFromObj(handPoseMask, "HandPoseMasks", "NewHandPoseMask");
            return handPoseMask;
        }

        public static HandPoseMask DuplicatePoseMaskAsset(HandPoseMask handPoseMask)
        {
            var newMask = Instantiate(handPoseMask);
            AssetUtils.CreateAssetInSettingsFromObj(handPoseMask, "HandPoseMasks", handPoseMask.name);
            return newMask;
        }
    }
}