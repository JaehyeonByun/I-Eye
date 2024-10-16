// /******************************************************************************
//  * File: QCHTControlBoxSettings.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.Distal.ControlBox
{
    [CreateAssetMenu(menuName = "QCHT/ControlBoxSettings")]
    public class QCHTControlBoxSettings : ScriptableObject
    {
        public enum ControlBoxDisplayType
        {
            Never,
            WhenHovered,
            Always
        }

        [Serializable]
        public class HandleState
        {
            public Color color = Color.white;
            public float size = 0.05f;
        }

        [Header("Control Box")]
        [Layer, Tooltip("Layer of the interaction grid")]
        public int Layer;
        [Range(0f, 0.5f)] public float ScaleOffset = 0.05f;
        public float MinScale = 0.5f;
        public float MaxScale = 1.5f;

        [Header("Grid settings")]
        [ColorUsage(true, true)] public Color HoverColor = new Color(3.56416106f, 0.857415974f, 1.83230662f, 1);
        [ColorUsage(true, true)] public Color SelectedColor = new Color(3.56416106f, 0.857415974f, 1.83230662f, 1);

        [Header("Handles settings")]
        [Tooltip("When display handles")] public ControlBoxDisplayType DisplayType = ControlBoxDisplayType.WhenHovered;
        [Tooltip("Idle state")] public HandleState NormalState = new HandleState();
        [Tooltip("Highlighted state")] public HandleState HoverState = new HandleState();
        [Tooltip("Selected state")] public HandleState SelectedState = new HandleState();
    }
}