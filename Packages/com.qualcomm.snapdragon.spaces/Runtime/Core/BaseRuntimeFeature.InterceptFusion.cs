/******************************************************************************
 * File: BaseRuntimeFeature.InterceptFusion.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetFusionSupported")]
        private static extern void SetFusionSupported_Internal(bool enable);

        public bool IsFusionSupported()
        {
            if (OpenXRSettings.ActiveBuildTargetInstance.GetFeature<FusionFeature>().enabled)
            {
#if UNITY_EDITOR
                return OpenXRSettings.ActiveBuildTargetInstance.GetFeature<FusionFeature>().SimulateFusionDevice;
#endif
                return DeviceAccessHelper.GetDeviceType() != DeviceTypes.Aio;
            }

            return false;
        }
    }
}
