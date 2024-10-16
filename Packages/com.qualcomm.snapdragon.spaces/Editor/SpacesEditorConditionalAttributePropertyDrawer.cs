/******************************************************************************
 * File: SpacesEditorConditionalAttributePropertyDrawer.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    // Custom property drawer which modifies the appearance of properties marked with the [SpacesEditorConditional] attribute.
    [CustomPropertyDrawer(typeof(SpacesEditorConditionalAttribute))]
    public class SpacesEditorConditionalAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var conditionalCheckAttributes = GetConditionalCheckAttributes(property);
            TryGetConditionalCheckResults(conditionalCheckAttributes, property, out var hideInInspector, out var enabled);

            if (hideInInspector)
            {
                return;
            }

            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var conditionalCheckAttributes = GetConditionalCheckAttributes(property);
            TryGetConditionalCheckResults(conditionalCheckAttributes, property, out var hideInInspector, out var enabled);

            if (hideInInspector)
            {
                return 0;
            }

            return base.GetPropertyHeight(property, label);
        }

        private static void TryGetConditionalCheckResults(List<SpacesEditorConditionalAttribute> conditionalChecks, SerializedProperty property, out bool hideInInspector, out bool enabled)
        {
            enabled = true;
            hideInInspector = false;

            foreach (var conditionalCheck in conditionalChecks)
            {
                SerializedProperty conditionalProperty = FindSerializableProperty(conditionalCheck, property);

                if (conditionalProperty == null)
                {
                    Debug.LogWarning($"Cannot find property {conditionalCheck.Property} referenced by SpacesEditorConditionalCheckAttribute");
                    continue;
                }

                object value = GetPropertyValue(conditionalProperty);
                bool result = value.ToString() == conditionalCheck.Value.ToString(); // comparing the objects alone doesnt work, so since we know these must be serialised compare the serialisations
                if (conditionalCheck.Inverse)
                {
                    result = !result;
                }

                if (result)
                {
                    if (conditionalCheck.HideInInspector)
                    {
                        hideInInspector = true;
                        break;
                    }

                    enabled = false;
                }
            }
        }

        private static SerializedProperty FindSerializableProperty(SpacesEditorConditionalAttribute conditionalCheck, SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            int ix = propertyPath.LastIndexOf('.');
            if (ix == -1)
            {
                return property.serializedObject.FindProperty(conditionalCheck.Property);
            }

            propertyPath = propertyPath.Substring(0, ix);
            return property.serializedObject.FindProperty(propertyPath).FindPropertyRelative(conditionalCheck.Property);
        }

        private static List<SpacesEditorConditionalAttribute> GetConditionalCheckAttributes(SerializedProperty serializedProperty)
        {
            if (serializedProperty == null)
            {
                return null;
            }

            var type = serializedProperty.serializedObject.targetObject.GetType();
            var fieldInfo = type.GetField(serializedProperty.propertyPath, (BindingFlags)(-1));
            if (fieldInfo == null)
            {
                return null;
            }

            return fieldInfo.GetCustomAttributes<SpacesEditorConditionalAttribute>().ToList();
        }

        private static object GetPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)property.intValue;
                case SerializedPropertyType.Enum:
                    if (property.enumNames.Length == 0)
                    {
                        return "undefined";
                    }
                    return property.enumNames[property.enumValueIndex];
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize;
                case SerializedPropertyType.Character:
                    return (char)property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Gradient:
                    return null;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue;
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceValue;
                case SerializedPropertyType.Hash128:
                    return property.hash128Value;
            }

            return null;
        }
    }
}
