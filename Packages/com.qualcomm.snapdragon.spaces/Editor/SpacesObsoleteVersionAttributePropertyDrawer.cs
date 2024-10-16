/******************************************************************************
 * File: SpacesObsoleteVersionAttributePropertyDrawer.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    // Custom property drawer which modifies the appearance of properties marked with the [SpacesObsoleteVersion] attribute.
    [CustomPropertyDrawer(typeof(SpacesObsoleteVersionAttribute))]
    public class SpacesObsoleteVersionAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SpacesObsoleteVersionAttribute spacesObsoleteVersionAttribute = (SpacesObsoleteVersionAttribute)attribute;

            if (spacesObsoleteVersionAttribute.ShowObsoletePropertyEmphasis)
            {
                EditorGUI.PropertyField(
                    position,
                    property,
                    CreateObsoleteFieldContent(
                        property,
                        label,
                        spacesObsoleteVersionAttribute.ObsoleteSinceVersion,
                        spacesObsoleteVersionAttribute.PlannedForRemovalInVersion)
                );
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        internal static GUIContent CreateObsoleteFieldContent(SerializedProperty property, GUIContent label, string obsoleteVersion = "", string plannedRemovalVersion = "")
        {
            string obsoleteMessage = "";
            var obsoleteAttr = GetObsoleteAttribute(property);
            if (obsoleteAttr != null)
            {
                obsoleteMessage = obsoleteAttr.Message;
            }


            string obsoleteTime = obsoleteVersion != string.Empty ? $" since version {obsoleteVersion} " : " ";
            string removalTime = plannedRemovalVersion != String.Empty ? " in version " + plannedRemovalVersion + "." : " at a later date.";
            string obsoleteWhen = $"This field is marked obsolete{obsoleteTime}and will be removed{removalTime}";

            var obsoleteFieldContent = new GUIContent(
                // (Obsolete) is added to the end of the property label.
                label.text + " (Obsolete)",
                // A small grey warning triangle is added before the property label.
                EditorGUIUtility.IconContent("console.warnicon.inactive.sml").image,
                // - The editor tooltip for the property has additional information:
                // -  - the version in which the property was marked obsolete, and the version in which it is planned to be removed finally (if known)
                // -  - the normal tooltip description
                // -  - the [Obsolete] attribute message
                $"{obsoleteWhen}\n\n{label.tooltip}\n\n[Obsolete] {obsoleteMessage}");

            return obsoleteFieldContent;
        }

        internal static ObsoleteAttribute GetObsoleteAttribute(SerializedProperty serializedProperty)
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

            return fieldInfo.GetCustomAttribute<ObsoleteAttribute>();
        }
    }
}
