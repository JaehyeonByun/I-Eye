// /******************************************************************************
//  * File: HandTrackingSimulationSettingsEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using System.Reflection;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace QCHT.Interactions.Core.Editor
{
    [CustomEditor(typeof(HandTrackingSimulationSettings))]
    public class HandTrackingSimulationSettingsEditor : UnityEditor.Editor, IPreprocessBuildWithReport,
        IPostprocessBuildWithReport
    {
        private SerializedProperty _enabledProperty;
        
        public void OnEnable()
        {
            _enabledProperty = serializedObject.FindProperty("enabled");
        }

        public override void OnInspectorGUI()
        {
            // Start on load
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_enabledProperty);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (_enabledProperty.boolValue)
                {
                    if (EditorUtility.DisplayDialog("Qualcomm Hand Tracking",
                            "The XR Device Simulator should be enabled in ProjectSettings/XR Interaction Toolkit in order to allow the simulation working properly.\n" +
                            "Please make sure to enable it before entering in playmode.",
                            "Ok"))
                    {
                        if (TryGetAutomaticallyInstantiateField(out var settings, out var enableField))
                        {
                            enableField?.SetValue(settings, true);

                            if (TryGetXRDeviceSimulator(out var simulator))
                            {
                                var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                                var prefabField = settings.GetType().GetField("m_SimulatorPrefab", bindingFlags);
                                prefabField?.SetValue(settings, simulator);
                            }
                        }
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            // Data source
            EditorGUI.BeginChangeCheck();
            var dataSource = serializedObject.FindProperty("dataSource");
            EditorGUILayout.PropertyField(dataSource);
            if (EditorGUI.EndChangeCheck())
            {
            }
        }

        private static bool TryGetXRDeviceSimulator(out GameObject simulator)
        {
            simulator = null;

            const string packageName = "com.unity.xr.interaction.toolkit";
            const string deviceSimulatorName = "XR Device Simulator";
            var package = Sample.FindByPackage(packageName, string.Empty);
            if (package == null)
                return false;

            foreach (var sample in package)
            {
                if (sample.displayName != deviceSimulatorName)
                    continue;

                if (!sample.isImported)
                    sample.Import(Sample.ImportOptions.OverridePreviousImports);

                break;
            }

            const string filter = "\"" + deviceSimulatorName + "\"";
            foreach (var guid in AssetDatabase.FindAssets(filter))
            {
                var asset = AssetDatabase.GUIDToAssetPath(guid);
                simulator = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                if (simulator != null && simulator.TryGetComponent<XRDeviceSimulator>(out _))
                    return true;
            }

            return false;
        }

        private bool _wasXRDeviceSimulatorActiveBeforeBuild;

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_ANDROID
            if (TryGetAutomaticallyInstantiateField(out var settings, out var enableField))
            {
                _wasXRDeviceSimulatorActiveBeforeBuild = (bool) enableField?.GetValue(settings)!;
                enableField.SetValue(settings, false);
            }
#endif
        }

        public void OnPostprocessBuild(BuildReport report)
        {
#if UNITY_ANDROID
            if (_wasXRDeviceSimulatorActiveBeforeBuild)
            {
                if (TryGetAutomaticallyInstantiateField(out var settings, out var enableField))
                {
                    enableField.SetValue(settings, true);
                }
            }
#endif
        }

        private static bool TryGetAutomaticallyInstantiateField(out ScriptableObject settings,
            out FieldInfo enableField)
        {
            enableField = null;

            const string deviceSimulatorSettings = "XRDeviceSimulatorSettings";
            const string automaticallyInstantiateSimulator = "m_AutomaticallyInstantiateSimulatorPrefab";
            settings = Resources.Load<ScriptableObject>(deviceSimulatorSettings);
            if (!settings)
                return false;

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            enableField = settings.GetType().GetField(automaticallyInstantiateSimulator, bindingFlags);

            return enableField != null;
        }
    }
}