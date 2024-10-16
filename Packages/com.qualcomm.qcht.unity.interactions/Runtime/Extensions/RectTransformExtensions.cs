// /******************************************************************************
//  * File: RectTransformExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class RectTransformExtensions
    {
        public static float DistanceFromArea(this RectTransform rectTransform, Vector2 enter,
            Vector3[] fourCorners = null)
        {
            if (fourCorners == null || fourCorners.Length < 4)
            {
                fourCorners = new Vector3[4];
                rectTransform.GetLocalCorners(fourCorners);
            }

            var b = new Vector2()
            {
                x = (Mathf.Abs(fourCorners[3].x) + Mathf.Abs(fourCorners[0].x)) / 2f,
                y = (Mathf.Abs(fourCorners[1].y) + Mathf.Abs(fourCorners[0].y)) / 2f
            };

            return SdBox2D(enter, b);
        }

        private static float SdBox2D(Vector2 p, Vector2 b)
        {
            p.x = Mathf.Abs(p.x) / b.x;
            p.y = Mathf.Abs(p.y) / b.y;

            var q = p - Vector2.one;

            q.x = Mathf.Max(q.x, 0f);
            q.y = Mathf.Max(q.y, 0f);

            var m = Mathf.Max(q.x, q.y);
            m = Mathf.Min(m, 0);

            return q.magnitude + m;
        }
    }
}