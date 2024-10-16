/******************************************************************************
 * File: PlaneDetectionFeatureEditor.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [CustomEditor(typeof(PlaneDetectionFeature))]
    public class PlaneDetectionFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty _useSceneUnderstandingPlaneDetection;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_useSceneUnderstandingPlaneDetection, GUILayout.ExpandWidth(true));
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _useSceneUnderstandingPlaneDetection = serializedObject.FindProperty("UseSceneUnderstandingPlaneDetection");
        }
    }
}
