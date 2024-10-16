/******************************************************************************
 * File: CameraAccessFeatureEditor.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [CustomEditor(typeof(CameraAccessFeature))]
    public class CameraAccessFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty _directAccessConversion;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // Because the checkbox is directly appended to the label, a manual spacing is added to the default label width.
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth + 80;
            EditorGUILayout.PropertyField(_directAccessConversion);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _directAccessConversion = serializedObject.FindProperty("DirectMemoryAccessConversion");
        }
    }
}
