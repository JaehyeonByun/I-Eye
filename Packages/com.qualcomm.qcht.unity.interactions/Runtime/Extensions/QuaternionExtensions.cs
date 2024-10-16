// /******************************************************************************
//  * File: QuaternionExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class QuaternionExtensions
    {
        public static Quaternion FlipXAxis(this Quaternion q) => new(q.w, q.x, -q.y, -q.z);

        public static Quaternion FlipYAxis(this Quaternion q) => new(q.w, -q.x, q.y, -q.z);

        public static Quaternion FlipZAxis(this Quaternion q) => new(q.w, -q.x, -q.y, q.z);

        public static Quaternion FlipXYAxis(this Quaternion q) => new(q.z, q.y, q.x, q.w);

        public static Quaternion FlipXZAxis(this Quaternion q) => new(-q.y, q.z, -q.w, q.x);
        
        public static Quaternion FlipYXAxis(this Quaternion q) => new(q.z, -q.y, -q.x, q.w);

        public static Quaternion FlipYZAxis(this Quaternion q) => new(q.x, q.w, q.z, q.y);

        public static Quaternion FlipZXAxis(this Quaternion q) => new(q.y, q.z, q.w, q.x);

        public static Quaternion FlipZYAxis(this Quaternion q) => new(q.x, q.w, -q.z, -q.y);
    }
}