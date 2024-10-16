// /******************************************************************************
//  * File: TransformExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class TransformExtensions
    {
        public static bool IsPointInsideArea(this Transform transform, Vector3 areaSize, Vector3 point)
        {
            var xyz = Quaternion.Inverse(transform.rotation) * (point - transform.position);
            var size = Vector3.Scale(transform.lossyScale, areaSize) * .5f;
            return SdBox(xyz, size) <= 0f;
        }
        
        private static float SdBox(Vector3 p, Vector3 b)
        {
            p.x = Mathf.Abs(p.x);
            p.y = Mathf.Abs(p.y);
            p.z = Mathf.Abs(p.z);

            var q = p - b;

            q.x = Mathf.Max(q.x, 0f);
            q.y = Mathf.Max(q.y, 0f);
            q.z = Mathf.Max(q.z, 0f);

            return q.magnitude;
        }
    }
}