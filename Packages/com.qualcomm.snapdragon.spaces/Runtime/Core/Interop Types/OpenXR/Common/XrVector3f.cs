/******************************************************************************
 * File: XrVector3f.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrVector3f
    {
        private float _x;
        private float _y;
        private float _z;

        /// <summary>
        /// This constructor converts the values from Unity to OpenXR space.
        /// </summary>
        public XrVector3f(Vector3 position)
        {
            _x = position.x;
            _y = position.y;
            _z = -position.z;
        }

        /// <summary>
        /// This constructor does not convert the values from Unity to OpenXR space.
        /// It constructs the XrVector3f with given values.
        /// </summary>
        public XrVector3f(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public static XrVector3f zero => new(0, 0, 0);

        public Vector3 ToVector3()
        {
            return new Vector3(_x, _y, -_z);
        }

        public override string ToString()
        {
            return $"[XrVector3f]{{{_x},{_y},{_z}}}";
        }
    }
}
