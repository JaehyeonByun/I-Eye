/******************************************************************************
 * File: SpacesRuntimeControllerStartController.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public class SpacesRuntimeControllerStartController
    {
        //Checks are done in the Java static methods to start controller or finish the splashActivity if no controller present
        [RuntimeInitializeOnLoadMethod]
        static void StartControllerOnLoadIfNeeded()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        var controller = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.hostcontroller.HostController");
        controller.CallStatic("StartControllerActivityOnHandset", activity);
        // Since we have no controller, we need to finish the splash screen activity
        var splashActivity = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.splashscreen.SplashScreenActivity");
        splashActivity.CallStatic("finishSplashScreenActivity");
#endif
        }
    }
}
