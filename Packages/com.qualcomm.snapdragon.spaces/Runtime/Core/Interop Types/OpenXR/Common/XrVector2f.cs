/******************************************************************************
 * File: XrVector2f.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ******************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrVector2f
    {
        private float _x;
        private float _y;

        public XrVector2f(Vector2 position)
        {
            _x = position.x;
            _y = position.y;
        }

        public XrVector2f(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(_x, _y);
        }
    }
}
