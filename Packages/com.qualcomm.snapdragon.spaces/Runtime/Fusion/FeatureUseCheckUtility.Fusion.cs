/******************************************************************************
 * File: FeatureUseCheckUtility.Fusion.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    /// <summary>
    ///     Imposes feature checks on all features when openXR is not running.
    ///     Adds diagnostic messages at runtime which can expose scene setup errors only occurring as a result of late loading
    ///     of openXR.
    /// </summary>
    internal static class FeatureUseCheckUtilityFusion
    {
        private static readonly string _log_OpenXrNotRunning_NoFusion = "Dual Render Fusion feature is not enabled, and yet OpenXR is not running!";

        private static readonly string _log_OpenXrNotRunning_NoAutostart = "The application is not configured to launch openXr at start and it is not safe to access features until it has started." +
            "\nPlease check Project Settings > XR Plugin Management > Initialize XR on Startup if this was unintended." +
            "\nIn the event that this value doesn't respond to attempts to change it, try opening Assets / XR / XRGeneralSettings -> AndroidSettings and checking the Init Manager On Start field.";

        private static readonly string _log_OpenXrInitialiseAtStart_WithFusion = "Dual Render Fusion feature is enabled but OpenXR is set to initialize at startup." +
            "\nIt is likely that this will not behave as expected unless glasses are connected at application start.";

        private static readonly string _log_NoGlassStatus_WithFusion = "Dual Render Fusion feature is enabled and configured to not start automatically, but there is no Spaces Glass Status component found in the scene or it is disabled." +
            "\nInformation about glasses connection and disconnection events will not be received.";

        private static readonly string _log_NoDynamicLoader_WithFusion = "Dual Render Fusion feature is enabled and configured to not start automatically, and yet there is no Dynamic OpenXR Loader component found in the scene or it is disabled." +
            "\nIf not using the Dynamic OpenXR Loader component to control initialisation of OpenXR ensure that all objects which use OpenXR features are not actively using them until OpenXR starts!";

        private static readonly string _log_NoOpenXrAutostart_WithFusion = "The Dynamic OpenXR Loader was not configured to Auto Start XR On Display Connected." +
            "\nWhen using the Dynamic OpenXR Loader to manually control startup, it is still helpful to configure your other objects using the events for tracking the OpenXR lifecycle." +
            "\nWhatever is trying to access this feature might not be configured correctly." +
            "\nIt is likely that the object trying to get access should be enabled only when the Dynamic Open XR Loader is finished launching OpenXR." +
            "\nTry registering this object with the appropriate Dynamic OpenXR Loader events for tracking the OpenXR lifecycle (OnOpenXRAvailable, OnOpenXRUnavailable, OnOpenXRStarting, OnOpenXRStarted, OnOpenXRStopping, OnOpenXRStopped).";

        private static readonly string _log_NoGlassesNoOpenXr_WithFusion = "No glasses are connected and OpenXR is not running." +
            "\nWhatever is trying to access this feature might not be configured correctly." +
            "\nIt is likely that the object trying to get access should be enabled only when the Dynamic Open XR Loader is finished launching OpenXR." +
            "\nTry registering this object with the appropriate Dynamic OpenXR Loader events for tracking the OpenXR lifecycle (OnOpenXRAvailable, OnOpenXRUnavailable, OnOpenXRStarting, OnOpenXRStarted, OnOpenXRStopping, OnOpenXRStopped).";

        internal static void FusionChecksForOpenXRNotRunning(ref FeatureUseCheckUtility.CheckResult newResult)
        {
            if (newResult.FeatureType != typeof(FusionFeature))
            {
                var fusionFeature = OpenXRSettings.Instance.GetFeature<FusionFeature>();
                if (!fusionFeature || !fusionFeature.enabled)
                {
                    newResult.AddDiagnosticMessage(_log_OpenXrNotRunning_NoFusion);
                    if (!XRGeneralSettings.Instance.InitManagerOnStart)
                    {
                        newResult.AddDiagnosticMessage(_log_OpenXrNotRunning_NoAutostart);
                    }

                    return;
                }

                if (XRGeneralSettings.Instance.InitManagerOnStart)
                {
                    newResult.AddDiagnosticMessage(_log_OpenXrInitialiseAtStart_WithFusion);
                    return;
                }

                bool isGlassConnected = false;
                var glassStatusComponent = SpacesGlassStatus.Instance;
                if (!glassStatusComponent || !glassStatusComponent.enabled)
                {
                    newResult.AddDiagnosticMessage(_log_NoGlassStatus_WithFusion);
                }
                else
                {
                    isGlassConnected = glassStatusComponent.GlassConnectionState == SpacesGlassStatus.ConnectionState.Connected;
                }

                var dynamicLoader = Object.FindObjectOfType<DynamicOpenXRLoader>(true);
                if (!dynamicLoader || !dynamicLoader.enabled)
                {
                    newResult.AddDiagnosticMessage(_log_NoDynamicLoader_WithFusion);
                }

                if (dynamicLoader && dynamicLoader.enabled && !dynamicLoader.AutoStartXROnDisplayConnected)
                {
                    newResult.AddDiagnosticMessage(_log_NoOpenXrAutostart_WithFusion);
                }

                if (dynamicLoader && dynamicLoader.enabled && !isGlassConnected && glassStatusComponent && glassStatusComponent.enabled)
                {
                    newResult.AddDiagnosticMessage(_log_NoGlassesNoOpenXr_WithFusion);
                }
            }
        }
    }
}
