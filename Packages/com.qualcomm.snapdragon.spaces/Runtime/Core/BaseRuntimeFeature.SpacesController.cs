/******************************************************************************
 * File: BaseRuntimeFeature.RuntimeCheck.cs
 * Copyright (c) 2022-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        /// <summary>
        ///     Reset the head pose from Spaces Host Controller.
        /// </summary>
        public bool TryResetPose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (LaunchControllerOnHost)
            {
                var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                var context = activity.Call<AndroidJavaObject>("getApplicationContext");
                var controller = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.hostcontroller.HostController");
                controller.CallStatic("ResetPose");
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Hook to startup the controller, if enabled to run on host, AFTER the scene has loaded
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public bool TryStartControllerOnHandset()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (LaunchControllerOnHost) {
                var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                var controller = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.hostcontroller.HostController");
                controller.CallStatic("StartControllerActivityOnHandset", activity);
                return true;
            }
#endif
            return false;
        }
    }
}
