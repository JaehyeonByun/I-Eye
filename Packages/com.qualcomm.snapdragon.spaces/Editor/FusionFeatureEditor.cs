/******************************************************************************
 * File: FusionFeatureEditor.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [CustomEditor(typeof(FusionFeature))]
    internal class FusionFeatureEditor : UnityEditor.Editor
    {
        private float _fixedSpaceWidth = 6f;
        private float _fixedToggleWidth = 15f;
        private SerializedProperty _simulateFusionDevice;
        private SerializedProperty _validateOpenScene;

        private void OnEnable()
        {
            _validateOpenScene = serializedObject.FindProperty("ValidateOpenScene");
            _simulateFusionDevice = serializedObject.FindProperty("SimulateFusionDevice");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // Because the checkbox is directly appended to the label, a manual spacing is added to the default label width.
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(_simulateFusionDevice.displayName)).x + _fixedSpaceWidth + _fixedToggleWidth;
            EditorGUILayout.PropertyField(_validateOpenScene);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_simulateFusionDevice);
            EditorGUILayout.Space();

            // Reset the original Editor label width in order to avoid broken UI.
            EditorGUIUtility.labelWidth = labelWidth;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
