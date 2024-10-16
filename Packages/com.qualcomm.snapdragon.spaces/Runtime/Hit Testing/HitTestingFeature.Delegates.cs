/******************************************************************************
 * File: RaycastFeature_Delegates.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class HitTestingFeature
    {
        #region XR_QCOM_ray_casting bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateRayCastQCOMDelegate(ulong session, ref XrRayCastCreateInfoQCOM createInfo, IntPtr rayCast);

        private static CreateRayCastQCOMDelegate _xrCreateRayCastQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult DestroyRayCastQCOMDelegate(ulong rayCast);

        private static DestroyRayCastQCOMDelegate _xrDestroyRayCastQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetRayCastStateQCOMDelegate(ulong rayCast, ref XrRayCastStateQCOM state);

        private static GetRayCastStateQCOMDelegate _xrGetRayCastStateQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CastRayQCOMDelegate(ulong rayCast, ulong space, XrVector3f origin, XrVector3f direction, float maxDistance);

        private static CastRayQCOMDelegate _xrCastRayQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetRayCollisionsQCOMDelegate(ulong rayCast, ref XrRayCollisionsGetInfoQCOM getInfo, ref XrRayCollisionsQCOM collisions);

        private static GetRayCollisionsQCOMDelegate _xrGetRayCollisionsQCOM;

        #endregion
    }
}
