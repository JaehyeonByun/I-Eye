// /******************************************************************************
//  * File: MinMaxDrawer.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEditor;
using UnityEngine;

namespace QCHT.Interactions.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public sealed class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var attr = attribute as MinMaxAttribute;

            if (attr == null)
                return;

            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                EditorGUI.LabelField(position, label.text,
                    "Only Vector2 or Vector2Int properties supported for MinMax.");
                Debug.LogError("[MinMaxDrawer] Can't use other type than Vector2 or Vector2Int for MinMax");
                return;
            }

            Vector2 minMax;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    minMax = property.vector2Value;
                    break;

                case SerializedPropertyType.Vector2Int:
                    minMax = property.vector2IntValue;
                    break;

                default:
                    return;
            }

            var rect = EditorGUI.PrefixLabel(position, label);
            var split = SplitRect(rect, 3);

            EditorGUI.BeginChangeCheck();

            minMax.x = EditorGUI.FloatField(split[0], minMax.x);
            minMax.y = EditorGUI.FloatField(split[2], minMax.y);

            if (minMax.x < attr.Min)
                minMax.x = attr.Min;

            if (minMax.y > attr.Max)
                minMax.y = attr.Max;

            EditorGUI.MinMaxSlider(split[1], ref minMax.x, ref minMax.y, attr.Min, attr.Max);

            if (EditorGUI.EndChangeCheck())
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Vector2:
                        property.vector2Value = minMax;
                        break;

                    case SerializedPropertyType.Vector2Int:
                        property.vector2IntValue = new Vector2Int((int) minMax.x, (int) minMax.y);
                        break;

                    default:
                        return;
                }

                var target = property.serializedObject.targetObject;
                UnityEditor.EditorUtility.SetDirty(target);
                Undo.RecordObject(target, "Change serialized value");
            }

            EditorGUI.EndProperty();
        }

        private static Rect[] SplitRect(Rect rect, int n)
        {
            const int space = 5;
            var rects = new Rect[n];

            for (var i = 0; i < n; i++)
            {
                rects[i] = new Rect(rect.position.x + i * rect.width / n, rect.position.y, rect.width / n,
                    rect.height);
            }

            var padding = (int) rects[0].width - 40;

            rects[0].width -= padding + space;
            rects[2].width -= padding + space;
            rects[1].x -= padding;
            rects[1].width += padding * 2;
            rects[2].x += padding + space;

            return rects;
        }
    }
}