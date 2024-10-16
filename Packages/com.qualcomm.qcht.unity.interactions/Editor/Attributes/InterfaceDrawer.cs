// /******************************************************************************
//  * File: InterfaceDrawer.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QCHT.Interactions.Editor
{
    [CustomPropertyDrawer(typeof(InterfaceAttribute))]
    public class InterfaceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var t = GetTypeFromAttribute();
            var obj = property.objectReferenceValue;
            EditorGUI.BeginProperty(position, label, property);
            var newObj = EditorGUI.ObjectField(position, label, obj, t, true) as MonoBehaviour;
            if (newObj != obj)
            {
                if (newObj == null || CheckInterface(newObj))
                {
                    property.objectReferenceValue = newObj;
                }
            }

            EditorGUI.EndProperty();
        }

        private Type GetTypeFromAttribute()
        {
            var a = attribute as InterfaceAttribute;
            var t = a?.Type;
            return t ?? typeof(MonoBehaviour);
        }

        private bool CheckInterface(MonoBehaviour monoBehaviour)
        {
            var a = attribute as InterfaceAttribute;
            return monoBehaviour.GetType().GetInterfaces().Any(t => a != null && t == a.Type);
        }
    }
}