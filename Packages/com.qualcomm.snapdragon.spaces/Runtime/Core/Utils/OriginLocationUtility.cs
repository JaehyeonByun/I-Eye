/******************************************************************************
 * File: OriginLocationUtility.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.ARFoundation;
#if AR_FOUNDATION_5_0_OR_NEWER
using Unity.XR.CoreUtils;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    public static class OriginLocationUtility
    {
#if AR_FOUNDATION_5_0_OR_NEWER
        public static XROrigin FindXROrigin(bool includeInactive = false)
        {
            return GameObject.FindObjectOfType<XROrigin>(includeInactive);
        }
#endif

        // disable deprecated warning for accessing ARSessionOrigin for backwards compatibility
#pragma warning disable CS0618
        public static ARSessionOrigin FindARSessionOrigin(bool includeInactive = false)
        {
            return Object.FindObjectOfType<ARSessionOrigin>(includeInactive);
        }
#pragma warning restore CS0618

        public static Camera GetOriginCamera(bool includeInactive = false)
        {
#if AR_FOUNDATION_5_0_OR_NEWER
            return FindXROrigin(includeInactive)?.Camera;
#endif
            // disable deprecated warning for accessing .camera for backwards compatibility and unreachable code warning
#pragma warning disable CS0618, CS0162
            return FindARSessionOrigin(includeInactive)?.camera;
#pragma warning restore CS0618,  CS0162
        }

        public static Transform GetOriginTransform(bool includeInactive = false)
        {
#if AR_FOUNDATION_5_0_OR_NEWER
            return FindXROrigin(includeInactive)?.transform;
#endif
            // disable warning for unreachable code
#pragma warning disable CS0162
            return FindARSessionOrigin(includeInactive)?.transform;
#pragma warning restore CS0162
        }
    }
}
