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
    internal struct XrRayCastCreateInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private XrRayTypeQCOM _rayType;
        private uint _targetTypeCount;
        private XrRayCastTargetTypeQCOM[] _targetTypes;

        public XrRayCastCreateInfoQCOM(XrRayTypeQCOM rayType, uint targetTypeCount, XrRayCastTargetTypeQCOM[] targetTypes)
        {
            _type = XrStructureType.XR_TYPE_RAY_CAST_CREATE_INFO_QCOM;
            _next = IntPtr.Zero;
            _rayType = rayType;
            _targetTypeCount = targetTypeCount;
            _targetTypes = targetTypes;
        }
    }
}
