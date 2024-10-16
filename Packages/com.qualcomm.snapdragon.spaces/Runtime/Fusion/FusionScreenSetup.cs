/******************************************************************************
 * File: FusionScreenSetup.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 ******************************************************************************/
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public class FusionScreenSetup : MonoBehaviour
    {
        enum OrientationType
        {
            None,
            Portrait,
            Landscape
        }

        [SerializeField]
        OrientationType ForcedOrientation = OrientationType.None;

        void Awake()
        {
            if (OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>()?.PreventSleepMode ?? false)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            // If fusion is not enabled the screen orientation should not be changed (to Portrait).
            // Doing so on an Aio device causes the headset display orientation to be changed which causes it to render incorrectly.
            if (OpenXRSettings.Instance.GetFeature<FusionFeature>()?.enabled ?? false)
            {
                switch (ForcedOrientation)
                {
                    case OrientationType.Portrait:
                        Screen.orientation = ScreenOrientation.Portrait;
                        break;

                    case OrientationType.Landscape:
                        Screen.orientation = ScreenOrientation.LandscapeLeft;
                        break;
                }
            }
        }
    }
}
