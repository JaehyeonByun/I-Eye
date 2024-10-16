/******************************************************************************
 * File: SpacesEditorConditionalAttribute.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    // A property attribute which allows a property or field to be marked as conditionally visible in the editor, based on the value of another property/field without writing a complete Editor for a class
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    internal class SpacesEditorConditionalAttribute : PropertyAttribute
    {
        /// <summary>
        ///     If true, element will be hidden in inspector. If false, element will be disabled in Inspector (visible, but not
        ///     editable)
        /// </summary>
        public readonly bool HideInInspector;

        /// <summary>
        ///     Inverse the check -> not equals Value to determine whether the element with this attribute will be shown.
        /// </summary>
        public readonly bool Inverse;

        /// <summary>
        ///     The property which will be compared to Value to determine whether the element with this attribute will be shown.
        /// </summary>
        public readonly string Property;

        /// <summary>
        ///     The value which when matched in Property, will determine whether the element with this attribute will be shown.
        /// </summary>
        public readonly object Value;

        public SpacesEditorConditionalAttribute(string Property, object Value, bool HideInInspector = false, bool Inverse = false)
        {
            this.Property = Property;
            this.Value = Value ?? true;
            this.Inverse = Inverse;
            this.HideInInspector = HideInInspector;
        }

        public SpacesEditorConditionalAttribute(string Property, bool Value = true, bool HideInInspector = false, bool Inverse = false)
        {
            this.Property = Property;
            this.Value = Value;
            this.Inverse = Inverse;
            this.HideInInspector = HideInInspector;
        }
    }
}
