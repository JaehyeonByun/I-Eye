/****************************************************************
 * Copyright (c) 2022 Qualcomm Technologies, Inc.
 * All Rights Reserved.
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrRayCollisionQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _targetId;
        private XrRayCastTargetTypeQCOM _targetType;
        private XrSceneObjectTypeMSFT _objectType;
        private XrVector3f _position;
        private XrVector3f _surfaceNormal;
        private long _time;
        public ulong TargetId => _targetId;
        public Vector3 Position => _position.ToVector3();
        public Vector3 SurfaceNormal => _surfaceNormal.ToVector3();

        public XrRayCollisionQCOM(ulong targetId, XrSceneObjectTypeMSFT objectType, XrRayCastTargetTypeQCOM targetType, XrVector3f position, XrVector3f surfaceNormal, long time)
        {
            _type = XrStructureType.XR_TYPE_RAY_COLLISION_QCOM;
            _next = IntPtr.Zero;
            _targetId = targetId;
            _objectType = objectType;
            _targetType = targetType;
            _position = position;
            _surfaceNormal = surfaceNormal;
            _time = time;
        }

        public new string ToString()
        {
            return "Structure Type: " + _type + "\n" +
                "Target Id: " + _targetId + "\n" +
                "XrRayCastTargetType: " + _targetType + "\n" +
                "Object Type: " + _objectType + "\n" +
                "Position: " + _position.ToVector3() + "\n" +
                "Surface Normal: " + _surfaceNormal.ToVector3() + "\n" +
                "Time: " + _time;
        }
    }
}
