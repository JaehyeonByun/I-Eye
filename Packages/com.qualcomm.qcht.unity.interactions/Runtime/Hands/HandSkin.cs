// /******************************************************************************
//  * File: HandSkin.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [CreateAssetMenu(menuName = "QCHT/HandSkin")]
    public class HandSkin : ScriptableObject, IHandFeedbackModifier
    {
        [Tooltip("Main hand mesh")]
        public Mesh MainMesh;

        [Tooltip("Main hand material")]
        public Material MainMaterial;

        [Tooltip("Ghost mesh when vff is active. If null the mesh will be the same as MainMesh")]
        public Mesh GhostMesh;

        [Tooltip("Ghost material when vff is active. If null no material will be applied")]
        public Material GhostMaterial;

        [Tooltip("Does this skin reacts to interaction feedbacks?")]
        public bool hasFeedbacks;

        [Tooltip("Idle finger feedback color and behaviour")]
        [field: SerializeField]
        public List<HandPartState> Idle { get; private set; }

        [Tooltip(
            "Default hovered finger feedback color and behaviour. This could be overrode by HandFeedbackModifier")]
        [field: SerializeField]
        public List<HandPartState> Hovered { get; private set; }

        [Tooltip(
            "Default selected finger feedback color and behaviour. This could be overrode by HandFeedbackModifier")]
        [field: SerializeField]
        public List<HandPartState> Selected { get; private set; }
    }
}