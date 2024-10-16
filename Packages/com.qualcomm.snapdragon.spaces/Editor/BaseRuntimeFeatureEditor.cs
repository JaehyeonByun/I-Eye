/******************************************************************************
 * File: BaseRuntimeFeatureEditor.cs
 * Copyright (c) 2021-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [CustomEditor(typeof(BaseRuntimeFeature))]
    internal class BaseRuntimeFeatureEditor : UnityEditor.Editor
    {
        private readonly string _controllerFoldoutEditorPrefsKey = "Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature.Settings.ControllerFoldoutOpen";
        private readonly string _advancedFoldoutEditorPrefsKey = "Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature.Settings.AdvancedFoldoutOpen";
        private SerializedProperty _checkInstalledRuntime;
        private SerializedProperty _launchAppOnViewer;
        private SerializedProperty _showSplashScreenOnHost;
        private SerializedProperty _showLaunchMessageOnHost;
        private SerializedProperty _preventSleepMode;
        private SerializedProperty _launchControllerOnHost;
        private SerializedProperty _useCustomController;
        private SerializedProperty _exportHeadless;
        private SerializedProperty _skipPermissionChecks;
        private bool _controllerFoldoutOpen;
        private bool _advancedFoldoutOpen;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // Because the checkbox is directly appended to the label, a manual spacing is added to the default label width.
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth + 80;
            EditorGUILayout.PropertyField(_checkInstalledRuntime);
            EditorGUILayout.Space();

            var openXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var fusionFeature = openXRSettings.GetFeature<FusionFeature>();
            bool fusionEnabled = fusionFeature?.enabled ?? false;

            using (new EditorGUI.DisabledScope(fusionEnabled))
            {
                EditorGUILayout.PropertyField(_launchAppOnViewer);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(_preventSleepMode);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_showSplashScreenOnHost);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(fusionEnabled))
            {
                bool tempControllerFoldoutOpen = EditorGUILayout.Foldout(_controllerFoldoutOpen, "Controller Settings", true);
                if (_controllerFoldoutOpen != tempControllerFoldoutOpen)
                {
                    _controllerFoldoutOpen = tempControllerFoldoutOpen;
                    EditorPrefs.SetBool(_controllerFoldoutEditorPrefsKey, _controllerFoldoutOpen);
                }

                if (_controllerFoldoutOpen)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_launchControllerOnHost);
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(_launchControllerOnHost.boolValue);
                    EditorGUILayout.PropertyField(_showLaunchMessageOnHost);
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(!_launchControllerOnHost.boolValue);
                    EditorGUILayout.PropertyField(_useCustomController);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("An Android archive including a custom controller implementation should be residing inside the Assets folder in order for this toggle to have any effect. Refer to the documentation on how to create a custom controller.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    {
                        DrawLinkButton("Link to documentation", "https://docs.spaces.qualcomm.com/common/designux/CustomControllerProject.html");
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();
            bool tempAdvancedSettingsFoldoutOpen = EditorGUILayout.Foldout(_advancedFoldoutOpen, "Advanced Settings", true);
            if (_advancedFoldoutOpen != tempAdvancedSettingsFoldoutOpen)
            {
                _advancedFoldoutOpen = tempAdvancedSettingsFoldoutOpen;
                EditorPrefs.SetBool(_advancedFoldoutEditorPrefsKey, _advancedFoldoutOpen);
            }

            if (_advancedFoldoutOpen)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_exportHeadless);
                EditorGUILayout.PropertyField(_skipPermissionChecks);
                EditorGUI.indentLevel--;
            }

            // Reset the original Editor label width in order to avoid broken UI.
            EditorGUIUtility.labelWidth = labelWidth;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _checkInstalledRuntime = serializedObject.FindProperty("CheckInstalledRuntime");
            _launchAppOnViewer = serializedObject.FindProperty("LaunchAppOnViewer");
            _showSplashScreenOnHost = serializedObject.FindProperty("ShowSplashScreenOnHost");
            _showLaunchMessageOnHost = serializedObject.FindProperty("ShowLaunchMessageOnHost");
            _preventSleepMode = serializedObject.FindProperty("PreventSleepMode");
            _controllerFoldoutOpen = EditorPrefs.GetBool(_controllerFoldoutEditorPrefsKey, true);
            _launchControllerOnHost = serializedObject.FindProperty("LaunchControllerOnHost");
            _useCustomController = serializedObject.FindProperty("UseCustomController");
            _advancedFoldoutOpen = EditorPrefs.GetBool(_advancedFoldoutEditorPrefsKey, true);
            _exportHeadless = serializedObject.FindProperty("ExportHeadless");
            _skipPermissionChecks = serializedObject.FindProperty("SkipPermissionChecks");
        }

        private void DrawLinkButton(string title, string url)
        {
            var linkButtonStyle = new GUIStyle(GUI.skin.label);
            linkButtonStyle.normal.textColor = new Color(0f, 0.5f, 0.95f, 1f);
            linkButtonStyle.hover.textColor = linkButtonStyle.normal.textColor;
            linkButtonStyle.fixedWidth = EditorStyles.label.CalcSize(new GUIContent(title + " ")).x;
            linkButtonStyle.margin = new RectOffset(50, 0, 0, 0);
            if (GUILayout.Button(title, linkButtonStyle))
            {
                Application.OpenURL(url);
            }

            var buttonRect = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(buttonRect.x, buttonRect.y + buttonRect.height, buttonRect.width, 2), GUIContent.none);
        }
    }
}
