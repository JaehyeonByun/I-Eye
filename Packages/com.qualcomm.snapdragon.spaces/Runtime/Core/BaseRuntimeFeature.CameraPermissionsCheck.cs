/******************************************************************************
 * File: BaseRuntimeFeature.CameraPermissionsCheck.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        public bool CheckServicesCameraPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var runtimeChecker = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.serviceshelper.RuntimeChecker");

            if (runtimeChecker.CallStatic<bool>("CheckCameraPermissions", new object[] { activity }))
            {
                return true;
            }
#endif
            return false;
        }
    }
}
