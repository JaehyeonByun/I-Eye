/******************************************************************************
 * File: XrExtent2Di.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrExtent2Di
    {
        uint _width;
        uint _height;

        public XrExtent2Di(Vector2Int extent)
        {
            _width = extent.x > 0 ? (uint)extent.x : 0;
            _height = extent.y > 0 ? (uint)extent.y : 0;
        }

        public XrExtent2Di(int width, int height)
        {
            _width = width > 0 ? (uint)width : 0;
            _height = height > 0 ? (uint)height : 0;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(Convert.ToInt32(_width), Convert.ToInt32(_height));
        }

        public uint Width => _width;
        public uint Height => _height;

        public override string ToString()
        {
            return String.Join("\n",
                "## XrExtent2Di ## ",
                "Width:\t" + _width,
                "Height:\t" + _height);
        }
    }
}
