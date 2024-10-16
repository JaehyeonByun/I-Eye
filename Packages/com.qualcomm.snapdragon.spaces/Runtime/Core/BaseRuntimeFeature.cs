/******************************************************************************
 * File: BaseRuntimeFeature.cs
 * Copyright (c) 2021-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = FeatureName,
        BuildTargetGroups = new[]
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.Standalone
        },
        CustomRuntimeLoaderBuildTargets = new[]
        {
            BuildTarget.Android
        },
        Company = "Qualcomm",
        Desc = "Enables base features like session tracking on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = true,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    public partial class BaseRuntimeFeature : SpacesOpenXRFeature
    {
        public delegate void OnSpacesAppSpaceChangeDelegate(ulong spaceHandle);

        public const string FeatureName = "Base Runtime";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.base";
        private const string ComponentVersioningExtension = "XR_QCOM_component_versioning";
        private const string DisplayRefreshRateExtension = "XR_FB_display_refresh_rate";
        private const string AndroidThreadSettingsExtension = "XR_KHR_android_thread_settings";
        private const string PerformanceSettingsExtension = "XR_EXT_performance_settings";
        public const string FeatureExtensions = ComponentVersioningExtension + " " + DisplayRefreshRateExtension + " " + AndroidThreadSettingsExtension + " " + PerformanceSettingsExtension;

        [Tooltip("Check the installation of the runtime and show a dialog if it isn't installed.")]
        [Obsolete("Checking of the installed runtime through the BaseRuntimeFeature is deprecated. This is now handled through the launcher activity, where the user will be prompted to install Snapdragon Spaces Services.")]
        [SpacesObsoleteVersion("0.16.0")]
        public bool CheckInstalledRuntime = true;

        [Tooltip("Start the application on the viewer device.")]
        public bool LaunchAppOnViewer = true;

        [Tooltip("Prevents the application to go into sleep mode")]
        public bool PreventSleepMode = true;

        [Tooltip("Show Splash screen on host when starting the application")]
        public bool ShowSplashScreenOnHost;

        [Tooltip("Show launch message on host when starting the application")]
        public bool ShowLaunchMessageOnHost;

        [Tooltip("Start the included Spaces Controller on the host device.")]
        public bool LaunchControllerOnHost = true;

        [Tooltip("Use a custom controller included in the asset on the host device instead of the default one in the package.")]
        public bool UseCustomController;

        [Tooltip("If this option is set to true, the exported application will be invisible on the device.")]
        public bool ExportHeadless;

        [Tooltip("If this option is set to a value, the defined activity will be started instead of the default Unity one.")]
        public string AlternateStartActivity = "";

        [Tooltip("If this option is set to true, no permission checks will be carried out during launch of the application.")]
        public bool SkipPermissionChecks;

        [Tooltip("Use this delegate to react when the device recenters its position.")]
        public OnSpacesAppSpaceChangeDelegate OnSpacesAppSpaceChange;

        [Tooltip("Fusion Support is only enabled on devices with a split viewer and host, such as a phone which can be connected to an external headset.\nThis does not guarantee that a viewer is currently connected, only that one is capable of being used.")]
        public bool FusionSupportEnabled
        {
            get => _fusionSupportEnabled;
        }

        public delegate void FusionEnabledDelegate(bool fusionSupportEnabled);
        public FusionEnabledDelegate OnFusionSupportEnabled;

        private bool _fusionSupportEnabled = false;
        private static readonly List<XRSessionSubsystemDescriptor> _sessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();


        protected override string GetXrLayersToLoad()
        {
            return "XR_APILAYER_retina_tracking";
        }

        protected override bool OnInstanceCreate(ulong instanceHandle)
        {
            base.OnInstanceCreate(instanceHandle);
            SetFrameStateCallback(OnFrameStateUpdate);
            SetPassthroughEnabled(true);
            SetPassthroughEnabled(false);
            SetSleepMode();

            if (SystemIDHandle != 0)
            {
                Internal_GetSystemProperties();
            }

            if (IsFusionSupported() && _xrGetUnityAndroidPresent != null)
            {
                _fusionSupportEnabled = (_xrGetUnityAndroidPresent(instanceHandle) == XrResult.XR_SUCCESS);
                OnFusionSupportEnabled?.Invoke(_fusionSupportEnabled);
            }

            Debug.Log($"FusionSupportEnabled: {FusionSupportEnabled}");

            return true;
        }

        protected override void OnSessionCreate(ulong sessionHandle)
        {
            base.OnSessionCreate(sessionHandle);

            EnumerateDisplayRefreshRates();
            ConfigureSpacesRenderEvents();
        }

        protected override void OnSystemChange(ulong systemIDHandle)
        {
            base.OnSystemChange(systemIDHandle);

            if (InstanceHandle != 0)
            {
                Internal_GetSystemProperties();
            }
        }

        protected override void OnAppSpaceChange(ulong spaceHandle)
        {
            base.OnAppSpaceChange(spaceHandle);
            OnSpacesAppSpaceChange?.Invoke(spaceHandle);
        }

        private void SetSleepMode()
        {
            if (PreventSleepMode)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate XrResult xrGetUnityAndroidPresentDelegate(ulong instance);

        internal static xrGetUnityAndroidPresentDelegate _xrGetUnityAndroidPresent;

        protected override void OnHookMethods()
        {
            base.OnHookMethods();
            HookMethod("xrGetComponentVersionsQCOM", out _xrGetComponentVersionsQCOM);
            HookMethod("xrGetUnityAndroidPresent", out _xrGetUnityAndroidPresent);
            HookMethod("xrSetAndroidApplicationThreadKHR", out _xrSetAndroidApplicationThreadKHR);
            HookMethod("xrEnumerateDisplayRefreshRatesFB", out _xrEnumerateDisplayRefreshRatesFB);
            HookMethod("xrRequestDisplayRefreshRateFB", out _xrRequestDisplayRefreshRateFB);
            HookMethod("xrGetDisplayRefreshRateFB", out _xrGetDisplayRefreshRateFB);
            HookMethod("xrPerfSettingsSetPerformanceLevelEXT", out _xrPerfSettingsSetPerformanceLevelEXT);
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(_sessionSubsystemDescriptors, SessionSubsystem.ID);
        }

        protected override void OnSubsystemStart()
        {
            StartSubsystem<XRSessionSubsystem>();
        }

        protected override void OnSessionBegin(ulong sessionHandle)
        {
            base.OnSessionBegin(sessionHandle);
            TryResetPose();
            ConfigureXRAndroidApplicationThreads();
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRSessionSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRSessionSubsystem>();
        }
    }
}
