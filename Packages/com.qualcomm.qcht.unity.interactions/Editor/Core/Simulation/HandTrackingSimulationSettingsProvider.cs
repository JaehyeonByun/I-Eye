// /******************************************************************************
//  * File: HandTrackingSimulationSettingsProvider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace QCHT.Interactions.Core.Editor
{
    public class HandTrackingSimulationSettingsProvider : SettingsProvider
    {
        private static class Decorator
        {
            public static readonly GUIContent handTrackingSimulationTitle = new GUIContent("Simulation Settings");

            public static readonly GUIStyle titleStyle = new GUIStyle("Label")
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
            };

            public class MarginScope : GUI.Scope
            {
                internal MarginScope()
                {
                    const float top = 10f;
                    const float left = 10f;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(left);
                    GUILayout.BeginVertical();
                    GUILayout.Space(top);
                }

                protected override void CloseScope()
                {
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
        }

        private const string SettingsPath = "Project/XR Plug-in Management/Hand Tracking Simulation";

        private UnityEditor.Editor _xrHandTrackingSettingsEditor;
        private UnityEditor.Editor _xrHandSimulationSettingsEditor;

        [SettingsProvider]
        private static SettingsProvider CreateXRHandSimulationSettingsProvider()
        {
            var keywordsList =
                GetSearchKeywordsFromPath(AssetDatabase.GetAssetPath(HandTrackingSimulationSettings.Instance))
                    .ToList();
            return new HandTrackingSimulationSettingsProvider {keywords = keywordsList};
        }

        private HandTrackingSimulationSettingsProvider(string path = SettingsPath,
            SettingsScope scopes = SettingsScope.Project,
            IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            _xrHandSimulationSettingsEditor =
                UnityEditor.Editor.CreateEditor(HandTrackingSimulationSettings.Instance);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            if (_xrHandTrackingSettingsEditor != null)
                Object.DestroyImmediate(_xrHandTrackingSettingsEditor);
            if (_xrHandSimulationSettingsEditor != null)
                Object.DestroyImmediate(_xrHandSimulationSettingsEditor);
        }

        private void DrawXRHandSimulationSettings()
        {
            if (_xrHandSimulationSettingsEditor)
            {
                GUILayout.Label(Decorator.handTrackingSimulationTitle, Decorator.titleStyle);
                _xrHandSimulationSettingsEditor.OnInspectorGUI();
            }
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            using (new Decorator.MarginScope())
            {
                DrawXRHandSimulationSettings();
            }
        }
    }
}