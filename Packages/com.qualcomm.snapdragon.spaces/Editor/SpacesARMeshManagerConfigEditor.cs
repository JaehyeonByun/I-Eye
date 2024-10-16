/******************************************************************************
 * File: SpacesARMeshManagerConfigEditor.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    // Custom inspector for spaces ar mesh manager config.
    [CustomEditor(typeof(SpacesARMeshManagerConfig))]
    public class SpacesARMeshManagerConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _calculateCpuNormals;
        private SerializedProperty _smoothingEpsilon;
        private SerializedProperty _useSmoothedNormals;

        private void OnEnable()
        {
            _calculateCpuNormals = serializedObject.FindProperty("_calculateCpuNormals");
            _useSmoothedNormals = serializedObject.FindProperty("_useSmoothedNormals");
            _smoothingEpsilon = serializedObject.FindProperty("_smoothingEpsilon");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_calculateCpuNormals);
            serializedObject.ApplyModifiedProperties();

            using (new EditorGUI.DisabledGroupScope(!_calculateCpuNormals.boolValue))
            {
                EditorGUILayout.PropertyField(_useSmoothedNormals);
                //EditorGUILayout.PropertyField(_smoothingEpsilon);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
