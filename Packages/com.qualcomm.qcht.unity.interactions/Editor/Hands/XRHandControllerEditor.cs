// /******************************************************************************
//  * File: XRHandControllerEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Hands.Editor
{
    [CustomEditor(typeof(XRHandController), true), CanEditMultipleObjects]
    public class XRHandControllerEditor : ActionBasedControllerEditor
    {
        private SerializedProperty m_optionalSelectActionProperty;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            m_optionalSelectActionProperty = serializedObject.FindProperty("m_OptionalSelectAction");
        }

        protected override void DrawOtherActions()
        {
            base.DrawOtherActions();
            EditorGUILayout.PropertyField(m_optionalSelectActionProperty);
        }
    }
}