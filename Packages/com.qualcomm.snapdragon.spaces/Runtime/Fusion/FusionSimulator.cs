/******************************************************************************
 * File: FusionSimulator.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.XR.OpenXR;
using UnityEditor;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    public class FusionSimulator : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR
            var openXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var fusionFeature = openXRSettings.GetFeature<FusionFeature>();
            bool simulateInEditor = fusionFeature != null && fusionFeature.enabled && fusionFeature.SimulateFusionDevice;
#endif

            Camera xrCamera = OriginLocationUtility.GetOriginCamera(true);
            if (xrCamera != null)
            {
#if UNITY_EDITOR
                if (simulateInEditor)
                {
                    xrCamera.targetDisplay = 1;
                }
#else
                xrCamera.targetDisplay = 0;
#endif
            }

            Camera phoneCamera = FindObjectOfType<SpacesHostView>(true).phoneCamera;
            if (phoneCamera != null)
            {
                phoneCamera.targetDisplay = 0;
            }

#if UNITY_EDITOR
            var spacesHostView = SpacesHostView.Instance;
            if (spacesHostView != null)
            {
                if (!simulateInEditor )
                {
                    spacesHostView.OnHostViewDisabled?.Invoke();
                }
                else
                {
                    spacesHostView.OnHostViewEnabled?.Invoke();
                }
            }
#endif
        }
    }
}
