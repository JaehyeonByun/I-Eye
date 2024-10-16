// /******************************************************************************
//  * File: Vector3Extensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 Multiply(this Vector3 v, Vector3 mul) => new(v.x * mul.x, v.y * mul.y, v.z * mul.z);

        public static Vector3 Divide(this Vector3 v, Vector3 div) => new(v.x / div.x, v.y / div.y, v.z / div.z);

        public static Vector3 MidPoint(this Vector3 p1, Vector3 p2) => (p1 + p2) * 0.5f;

        public static Vector3 Abs(Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public static Vector3 FlipXAxis(this Vector3 v) => new(-v.x, v.y, v.z);

        public static Vector3 FlipYAxis(this Vector3 v) => new(v.x, -v.y, v.z);

        public static Vector3 FlipZAxis(this Vector3 v) => new(v.x, v.y, -v.z);

        public static Vector3 FlipXYAxis(this Vector3 v) => new(v.y, v.x, v.z);

        public static Vector3 FlipXZAxis(this Vector3 v) => new(v.z, v.y, v.x);

        public static Vector3 FlipYZAxis(this Vector3 v) => new(v.x, v.z, v.y);

        public static Vector3 FlipXYZToZXYAxis(this Vector3 v) => new(v.z, v.x, v.y);

        public static Vector3 FlipXYZToYZXAxis(this Vector3 v) => new(v.y, v.z, v.x);
    }
}