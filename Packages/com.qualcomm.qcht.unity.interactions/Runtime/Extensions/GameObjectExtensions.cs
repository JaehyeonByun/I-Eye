// /******************************************************************************
//  * File: GameObjectExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class GameObjectExtensions
    {
        public static Bounds GetMeshBoundingBox(this GameObject go)
        {
            var mfs = go.GetComponentsInChildren<MeshRenderer>();

            if (mfs.Length <= 0) return new Bounds();

            var b = mfs[0].bounds;
            for (var i = 1; i < mfs.Length; i++)
            {
                b.Encapsulate(mfs[i].bounds);
            }

            return b;
        }

#if UNITY_2021_1_OR_NEWER
        public static Bounds GetMeshLocalBoundingBox(this GameObject go)
        {
            var mfs = go.GetComponentsInChildren<MeshRenderer>();

            if (mfs.Length <= 0) return new Bounds();

            var b = mfs[0].localBounds;
            for (var i = 1; i < mfs.Length; i++)
            {
                b.Encapsulate(mfs[i].localBounds);
            }

            return b;
        }
#endif
    }
}