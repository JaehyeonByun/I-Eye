// /******************************************************************************
// * File: XRPassthroughUtility.cs
// * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
// *
// * Confidential and Proprietary - Qualcomm Technologies, Inc.
// *
// ******************************************************************************/

#if SPACES
using Qualcomm.Snapdragon.Spaces;
#endif
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace QCHT.Interactions.Core
{
    public static class XRPassthroughUtility
    {
        private struct CameraState
        {
            public CameraClearFlags Flags;
            public Color Color;
        }

        private static bool s_passthroughWasEnabled; // Stores if passthrough has been activated once during this session
        private static CameraState s_cameraSaveState;

        public static void SetPassthroughEnabled(bool enable)
        {
            if (!IsPassthroughSupported())
            {
                Debug.LogWarning("[XRPassthroughUtility] Passthrough feature is not supported.");
                return;
            }
            
            var camera = XROriginUtility.GetOriginCamera();
            if (enable)
            {
                s_cameraSaveState.Flags = camera.clearFlags;
                s_cameraSaveState.Color = camera.backgroundColor;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor =
                    new Color(s_cameraSaveState.Color.r, s_cameraSaveState.Color.g, s_cameraSaveState.Color.b, 0f);
                s_passthroughWasEnabled = true;
            }
            else
            {
                if (s_passthroughWasEnabled)
                {
                    camera.clearFlags = s_cameraSaveState.Flags;
                    camera.backgroundColor = s_cameraSaveState.Color;
                }
            }
#if SPACES
            var baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            if (baseRuntimeFeature != null)
            {
                baseRuntimeFeature.SetPassthroughEnabled(enable);
                return;
            }
#endif
            QCHTOpenXRPlugin.SetPassthroughEnabled(enable);
        }

        public static bool GetPassthroughEnabled()
        {
#if SPACES
            var baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            if (baseRuntimeFeature != null)
            {
                return baseRuntimeFeature.GetPassthroughEnabled();
            }
#endif
            return QCHTOpenXRPlugin.GetPassthroughEnabled();
        }

        public static bool IsPassthroughSupported()
        {
#if SPACES
            var baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            if (baseRuntimeFeature != null)
            {
                return baseRuntimeFeature.IsPassthroughSupported();
            }
#endif
            return QCHTOpenXRPlugin.IsPassthroughSupported();
        }
    }
}