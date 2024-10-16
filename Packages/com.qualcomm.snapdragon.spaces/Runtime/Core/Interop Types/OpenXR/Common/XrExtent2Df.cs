/******************************************************************************
 * File: XrExtent2Df.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrExtent2Df
    {
        private float _width;
        private float _height;

        public float Width => _width;
        public float Height => _height;

        public XrExtent2Df(float width, float height)
        {
            _width = width;
            _height = height;
        }
    }
}
