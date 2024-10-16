/******************************************************************************
 * File: XrOffset2Di.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ******************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrOffset2Di
    {
        private int _x;
        private int _y;

        public XrOffset2Di(Vector2Int offset)
        {
            _x = offset.x;
            _y = offset.y;
        }

        public XrOffset2Di(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(_x, _y);
        }
    }
}
