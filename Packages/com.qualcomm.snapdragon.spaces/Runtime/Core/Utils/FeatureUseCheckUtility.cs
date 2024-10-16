/******************************************************************************
 * File: FeatureUseCheckUtility.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR
using UnityEngine.XR.Management;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    /// <summary>
    ///     Utility class which unifies the checking of OpenXrFeature instances.
    /// </summary>
    public static class FeatureUseCheckUtility
    {
        /// <summary>
        ///     Some checks may be imposed on all features as a result of another feature being enabled
        ///     (e.g. Fusion requires additional checks about the state of openXr, or other components in the scene when it is enabled).
        ///     These checks can be added to the ImposedFeatureChecks delegates created here.
        /// </summary>
        internal delegate void ImposedFeatureChecks(ref CheckResult checkResult);

        /// <summary>
        ///     Invoked when OpenXR is not running, and a feature was requested.
        ///     Can be used to log at runtime some additional useful information to a developer, such as that they avoid doing this, or how to check the scene is setup correctly.
        ///     The results of these feature checks are cached for the lifetime of the openXr loader and are assumed to not change at runtime except when openXr is destroyed / created.
        /// </summary>
        internal static ImposedFeatureChecks ImposeFeatureChecks_OpenXrNotRunning = null;

        private static readonly Dictionary<Type, CheckResult> _cachedResults = new();
#if !UNITY_EDITOR
        // Do not access directly. This private field should be get/set from the private property IsLoaderLoaded.
        private static bool _isLoaderLoaded;

        private static bool IsLoaderLoaded
        {
            get => _isLoaderLoaded;
            set
            {
                if (_isLoaderLoaded == value)
                    return;

                _isLoaderLoaded = value;
                // Clear all cached results if the state of the loader changes
                _cachedResults.Clear();
            }
        }
#endif

#pragma warning disable CS0414
        private static readonly string _log_XrNotInitialised = "XR has not completed initialisation!";
        private static readonly string _log_NoActiveLoader = "No active XR loader exists!";
        private static readonly string _log_FeatureNotEnabled = "Feature is not enabled.";
#pragma warning restore CS0414

        internal class CheckResult
        {
            public bool Enabled { get; set; }

            private List<string> _diagnosticMessages;

            private readonly string _errorHeader;

            public readonly Type FeatureType;

            public CheckResult(Type featureType)
            {
                Enabled = true;
                FeatureType = featureType;
                _errorHeader = $"Failed to use feature of type {FeatureType} - (Diagnostic output is logged once per openXr session)";
                _diagnosticMessages = new List<string>();
            }

            public void LogResult(bool logDiagnosticMessages = false)
            {
                if (!Enabled)
                {
                    Debug.LogError(_errorHeader);

                    if (logDiagnosticMessages)
                    {
                        string finalMessage = "";
                        _diagnosticMessages.ForEach((message => finalMessage += message + "\n\n"));
                        Debug.LogWarning(finalMessage);
                    }
                }
            }

            public void AddDiagnosticMessage(string message)
            {
                _diagnosticMessages.Add(message);
            }
        }

        /// <summary>
        ///     Check to see if an instance of an OpenXR feature is currently usable.
        /// </summary>
        /// <param name="feature">Instance of the feature to check</param>
        /// <typeparam name="TFeature">Type of feature to be checked.</typeparam>
        /// <returns>True if the feature is usable: that is, the feature instance is non-null, the feature is enabled, OpenXr is running and (by default) there is a valid session.
        /// False if any of those conditions are not met. Additionally, once per session, this will output diagnostic logs (on a failure) attempting to explain why this did not succeed.
        /// This will (unless specific behaviour is overridden for the feature) return false for UNITY_EDITOR without logging.
        /// When handling a false result, consider carefully if you should log if in UNITY_EDITOR.
        /// </returns>
        public static bool IsFeatureUseable<TFeature>(TFeature feature)
            where TFeature : SpacesOpenXRFeature
        {
            // Immediate fail if the feature instance is null regardless of type
            if (!feature)
            {
#if !UNITY_EDITOR
                Debug.LogError($"Instance of feature of type {typeof(TFeature)} is null.");
#endif
                return false;
            }

            // If cached results exist, they will persist while the openxr loader remains in the same state - loaded or not loaded
            if (_cachedResults.TryGetValue(typeof(TFeature), out CheckResult result))
            {
                result.LogResult();
                return result.Enabled && feature.OnCheckIsFeatureUseable();
            }

            CheckResult newResult = new CheckResult(typeof(TFeature));
            // In the editor checking for an active loader is pointless
#if !UNITY_EDITOR
            // If there are no cached results for this feature type
            // run all checks - this should happen only once while the openxr loader remains in the same state - loaded or not loaded
            // If/when the session ends or a new session starts these checks will be run once more
            var managerSettings = XRGeneralSettings.Instance.Manager;
            IsLoaderLoaded = managerSettings.isInitializationComplete && managerSettings.activeLoader;
            if (!IsLoaderLoaded)
            {
                newResult.Enabled = false;
                if (!managerSettings.isInitializationComplete)
                {
                    newResult.AddDiagnosticMessage(_log_XrNotInitialised);
                }

                if (!managerSettings.activeLoader)
                {
                    newResult.AddDiagnosticMessage(_log_NoActiveLoader);
                }

                ImposeFeatureChecks_OpenXrNotRunning?.Invoke(ref newResult);
            }
#endif

            // A feature can be disabled in two cases:
            // (1) the feature checkbox is not checked (i.e. no intent to use the feature)
            // (2) the feature checkbox was checked, and intended for this feature to be enabled, but initialisation failed!
            // It will be true if the checkbox is checked, both if openXr is __not__ running, and if openXr __is__ running (and initialization succeeded).
            if (!feature.enabled)
            {
                newResult.Enabled = false;
                newResult.AddDiagnosticMessage(_log_FeatureNotEnabled);
            }
            else
            {
                newResult.Enabled &= feature.OnCheckIsFeatureUseable_Cached();
            }

            _cachedResults.Add(typeof(TFeature), newResult);

            newResult.LogResult(true);
            return newResult.Enabled && feature.OnCheckIsFeatureUseable();

            // NOTE: in UnityEditor this should cache a result with enabled == true, and not log anything
            // and then return false because there is not a valid openXR session (SessionHandle == 0 && SystemIDHandle == 0)
        }
    }
}
