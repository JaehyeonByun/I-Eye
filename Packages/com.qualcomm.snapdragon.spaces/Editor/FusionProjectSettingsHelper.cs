/******************************************************************************
* File: FusionProjectSettingsHelper.cs
* Copyright (c)2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
*
* Confidential and Proprietary - Qualcomm Technologies, Inc.
*
******************************************************************************/

using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    public static class FusionProjectSettingsHelper
    {
        #region Backed-up project settings

        private static bool _projectSettingsBackedUp;
        private static string _productName;
        private static string _bundleVersion;
        private static string _applicationIdentifier;
        private static int _activeInputHandler;
        private static bool _initializeXrOnStartup;
        private static bool _fusionValidateOpenScene;
        private static bool _launchAppOnViewer;
        private static bool _launchControllerOnHost;
        private static bool _exportHeadless;

        #endregion

        [MenuItem("Snapdragon Spaces/Dual Render Fusion/Configure Fusion project")]
        public static bool ConfigureFusionProject()
        {
            Debug.Log("Configuring Spaces Fusion project...");

            // Discard backed-up values to avoid con(fusion) if configuration fails
            _projectSettingsBackedUp = false;
            if (!ApplyFusionProjectSettings(out string result))
            {
                Debug.LogError("Failed to apply Dual Render Fusion project settings: " + result);
                return false;
            }

            Debug.Log("Spaces Fusion project configured.");
            return true;
        }

        [MenuItem("Snapdragon Spaces/Dual Render Fusion/Restore project configuration", true)]
        public static bool CanRestoreProjectConfiguration()
        {
            return _projectSettingsBackedUp;
        }

        [MenuItem("Snapdragon Spaces/Dual Render Fusion/Restore project configuration")]
        public static bool RestoreProjectConfiguration()
        {
            Debug.Log("Restoring project configuration...");
            string result;
            if (!ApplySavedProjectSettings(out result))
            {
                Debug.LogError("Failed to apply saved project settings: " + result);
                return false;
            }

            Debug.Log("Project configuration restored.");
            return true;
        }

        private static bool ApplyFusionProjectSettings(out string message)
        {
            message = "";

            // Check settings can be applied before changing anything
            var activeInputHandlerProperty = GetPlayerSettingsPropertyOrNull("activeInputHandler");
            if (activeInputHandlerProperty == null)
            {
                message = "\"activeInputHandler\" property not found in PlayerSettings.";
                return false;
            }

            var baseRuntimeFeature = OpenXRSettings.ActiveBuildTargetInstance.GetFeature<BaseRuntimeFeature>();
            if (baseRuntimeFeature == null)
            {
                message = "Base Runtime Feature not found in OpenXRSettings.";
                return false;
            }

            var fusionFeature = OpenXRSettings.ActiveBuildTargetInstance.GetFeature<FusionFeature>();
            if (fusionFeature == null)
            {
                message = "Fusion Feature not found in OpenXRSettings.";
                return false;
            }

            // 1. Player settings

            // Check "Active Input Handling" property:
            // 0 = Legacy, 1 = New, 2 = Both
            _activeInputHandler = activeInputHandlerProperty.intValue;
            if (_activeInputHandler != 2)
            {
                // Note (CH): Changing activeInputHandler restarts the editor when done manually. When done via script,
                // there is a chance any previous project settings changes will be lost. Therefore, MUST be done at the beginning.
                activeInputHandlerProperty.intValue = 2;
                activeInputHandlerProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            // Android package properties
            _productName = PlayerSettings.productName;
            if (!_productName.EndsWith("-Fusion"))
            {
                PlayerSettings.productName = _productName + "-Fusion";
            }

            _bundleVersion = PlayerSettings.bundleVersion;
            if (!_bundleVersion.EndsWith("-Fusion"))
            {
                PlayerSettings.bundleVersion = _bundleVersion + "-Fusion";
            }

            _applicationIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            if (!_applicationIdentifier.Contains("fusion"))
            {
                var applicationIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, applicationIdentifier + ".fusion");
            }

            // 2. XR and OpenXR settings

            // XR plugin (Android) settings
            _initializeXrOnStartup = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android).InitManagerOnStart;
            XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android).InitManagerOnStart = false;

            // Base Runtime Feature settings
            _launchAppOnViewer = baseRuntimeFeature.LaunchAppOnViewer;
            baseRuntimeFeature.LaunchAppOnViewer = false;
            _launchControllerOnHost = baseRuntimeFeature.LaunchControllerOnHost;
            baseRuntimeFeature.LaunchControllerOnHost = false;
            _exportHeadless = baseRuntimeFeature.ExportHeadless;
            baseRuntimeFeature.ExportHeadless = false;

            // Dual Render Fusion settings
            fusionFeature.enabled = true;
            _fusionValidateOpenScene = fusionFeature.ValidateOpenScene;
            fusionFeature.ValidateOpenScene = false;
            _projectSettingsBackedUp = true;
            message = "Success applying Dual Render Fusion project settings.";
            return true;
        }

        private static bool ApplySavedProjectSettings(out string message)
        {
            message = "";
            if (!_projectSettingsBackedUp)
            {
                message = "No project settings to restore to. \"Restore project settings\" is intended after configuring a Fusion project.";
                return false;
            }

            // Check settings can be applied before changing anything
            var activeInputHandlerProperty = GetPlayerSettingsPropertyOrNull("activeInputHandler");
            if (activeInputHandlerProperty == null)
            {
                message = "\"activeInputHandler\" property not found in PlayerSettings.";
                return false;
            }

            var baseRuntimeFeature = OpenXRSettings.ActiveBuildTargetInstance.GetFeature<BaseRuntimeFeature>();
            if (baseRuntimeFeature == null)
            {
                message = "Base Runtime Feature not found in OpenXRSettings.";
                return false;
            }

            var fusionFeature = OpenXRSettings.ActiveBuildTargetInstance.GetFeature<FusionFeature>();
            if (fusionFeature == null)
            {
                message = "Fusion Feature not found in OpenXRSettings.";
                return false;
            }

            // 1. Player Settings

            // Note (CH): Changing activeInputHandler restarts the editor when done manually. When done via script,
            // there is a chance any previous project settings changes will be lost. Therefore, MUST be done at the beginning.
            activeInputHandlerProperty.intValue = _activeInputHandler;
            activeInputHandlerProperty.serializedObject.ApplyModifiedProperties();
            PlayerSettings.productName = _productName;
            PlayerSettings.bundleVersion = _bundleVersion;
            PlayerSettings.applicationIdentifier = _applicationIdentifier;

            // 2. XR and OpenXR settings
            // XR plugin (Android) settings
            XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android).InitManagerOnStart = _initializeXrOnStartup;

            // Base Runtime Feature settings
            baseRuntimeFeature.LaunchAppOnViewer = _launchAppOnViewer;
            baseRuntimeFeature.LaunchControllerOnHost = _launchControllerOnHost;
            baseRuntimeFeature.ExportHeadless = _exportHeadless;

            // Dual Render Fusion settings
            fusionFeature.enabled = false;
            fusionFeature.ValidateOpenScene = _fusionValidateOpenScene;
            return true;
        }


        // Note (CH): The following function allows retrieving PlayerSettings properties which are not exposed through Unity's API.
        // No other way to do it. See 'Packages/Input System/InputSystem/Editor/Settings/EditorPlayerSettingsHelper.cs'
        // The correct PropertyName can be retrieved in the Player Settings window, right-clicking and selecting "Copy Property Path".
        private static SerializedProperty GetPlayerSettingsPropertyOrNull(string propertyName)
        {
            var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();
            if (playerSettings == null)
            {
                return null;
            }

            var playerSettingsObject = new SerializedObject(playerSettings);
            return playerSettingsObject.FindProperty(propertyName);
        }
    }
}
