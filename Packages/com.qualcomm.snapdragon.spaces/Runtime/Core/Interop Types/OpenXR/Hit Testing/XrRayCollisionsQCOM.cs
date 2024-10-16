/****************************************************************
 * Copyright (c) 2022 Qualcomm Technologies, Inc.
 * All Rights Reserved.
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrRayCollisionsQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _collisionsCapacityInput;

        // uint
        private IntPtr _collisionCapacityOutput;
        // XrRayCollisionQCOM
        private IntPtr  _collisions;

        public IntPtr Collisions => _collisions;

        public XrRayCollisionsQCOM(IntPtr collisionCapacityOutputPtr)
        {
            _type = XrStructureType.XR_TYPE_RAY_COLLISIONS_QCOM;
            _next = IntPtr.Zero;
            _collisionsCapacityInput = 0;
            _collisionCapacityOutput = collisionCapacityOutputPtr;
            _collisions = IntPtr.Zero;
        }

        public XrRayCollisionsQCOM(uint collisionsCapacityInput, IntPtr collisionsCapacityOutputPtr, IntPtr collisionsPtr)
        {
            _type = XrStructureType.XR_TYPE_RAY_COLLISIONS_QCOM;
            _next = IntPtr.Zero;
            _collisionsCapacityInput = collisionsCapacityInput;
            _collisionCapacityOutput = collisionsCapacityOutputPtr;
            _collisions = collisionsPtr;
        }
    }
}
