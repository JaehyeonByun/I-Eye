/******************************************************************************
 * File: SpacesHostView.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.OpenXR;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/Dual Render Fusion/Spaces Host View")]
    [DefaultExecutionOrder(int.MinValue + 1)]
    public class SpacesHostView : MonoBehaviour
    {
        internal UnityEvent OnHostViewEnabled;
        internal UnityEvent OnHostViewDisabled;

        public Camera phoneCamera;
        public static SpacesHostView Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);

            phoneCamera = GetComponent<Camera>();
        }

        private void Reset()
        {
            phoneCamera = GetComponent<Camera>();
            phoneCamera.depth = 1;
            phoneCamera.stereoTargetEye = StereoTargetEyeMask.None;
            phoneCamera.targetDisplay = 0;
            phoneCamera.enabled = enabled;
        }

        private void OnEnable()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            WarnIfMultipleObjects();
#endif

#if UNITY_EDITOR
            var openXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var fusionFeature = openXRSettings.GetFeature<FusionFeature>();

            OnFusionSupportEnabled(fusionFeature != null && fusionFeature.enabled && fusionFeature.SimulateFusionDevice);
#else
            FusionFeature fusionFeature = OpenXRSettings.Instance.GetFeature<FusionFeature>();
            // Accessing fusion feature early. It is very likely that it is not useable at this point, but we only care that it is intended to be enabled
            if (fusionFeature == null || !fusionFeature.enabled)
            {
                HostViewUnavailable();
                return;
            }

            BaseRuntimeFeature baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            if (baseRuntimeFeature != null)
            {
                baseRuntimeFeature.OnFusionSupportEnabled += OnFusionSupportEnabled;
                OnFusionSupportEnabled(baseRuntimeFeature.InstanceHandle == 0 || baseRuntimeFeature.FusionSupportEnabled);
            }
#endif
        }

        private void OnFusionSupportEnabled(bool fusionSupportEnabled)
        {
            if (fusionSupportEnabled)
            {
                HostViewAvailable();
            }
            else
            {
                HostViewUnavailable();
            }
        }

        private void OnDisable()
        {
            if (Instance != this)
            {
                return;
            }

            phoneCamera.enabled = false;

            BaseRuntimeFeature baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            if (baseRuntimeFeature)
            {
                baseRuntimeFeature.OnFusionSupportEnabled -= OnFusionSupportEnabled;
            }
        }

        private void HostViewUnavailable()
        {
            phoneCamera.enabled = false;
            OnHostViewDisabled?.Invoke();
        }

        private void HostViewAvailable()
        {
            phoneCamera.enabled = true;
            OnHostViewEnabled?.Invoke();
        }

        /// <summary>
        ///     Prints a warning to the console if more than one <see cref="SpacesHostView" />
        ///     component is active.
        /// </summary>
        private void WarnIfMultipleObjects()
        {
            var spacesHostViews = FindObjectsOfType<SpacesHostView>();
            if (spacesHostViews.Length > 1)
            {
                Debug.LogWarning("Multiple active Spaces Host Views objects found. You should only have one active Spaces Host View at a time.");
            }
        }
    }
}
