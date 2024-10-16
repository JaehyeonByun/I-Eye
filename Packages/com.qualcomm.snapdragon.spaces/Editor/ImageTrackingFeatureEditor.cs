/******************************************************************************
 * File: ImageTrackingFeatureEditor.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [CustomEditor(typeof(ImageTrackingFeature))]
    public class ImageTrackingFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty _extendedRangeMode;
        private SerializedProperty _lowPowerMode;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_extendedRangeMode);
            EditorGUILayout.PropertyField(_lowPowerMode);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _extendedRangeMode = serializedObject.FindProperty("ExtendedRangeMode");
            _lowPowerMode = serializedObject.FindProperty("LowPowerMode");
        }
    }
}
