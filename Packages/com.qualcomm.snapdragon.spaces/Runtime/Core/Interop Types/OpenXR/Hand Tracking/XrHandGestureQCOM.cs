/******************************************************************************
 * File: XrHandGestureQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete]
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrHandGestureQCOM
    {
        private int _gesture;
        private float _gestureRatio;
        private float _flipRatio;

        public XrHandGestureQCOM(int gesture, float gestureRatio, float flipRatio)
        {
            _gesture = gesture;
            _gestureRatio = gestureRatio;
            _flipRatio = flipRatio;
        }

        public int Gesture => _gesture;
        public float GestureRatio => _gestureRatio;
        public float FlipRatio => _flipRatio;
    }
}
