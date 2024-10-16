// /******************************************************************************
//  * File: XRHandedness.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using QCHT.Interactions.Core;

namespace QCHT.Interactions.Hands
{
    /// <summary>
    /// Simple component serializing XrHandedness
    /// Used to filter hover and select hand without using XRIT Interaction layer masks.
    /// See XRHandFilter, as an example of use.
    /// </summary>
    public class XRHandedness : MonoBehaviour, IHandedness
    {
        [SerializeField] private XrHandedness handedness;

        public XrHandedness Handedness
        {
            set => handedness = value;
            get => handedness;
        }
    }
}