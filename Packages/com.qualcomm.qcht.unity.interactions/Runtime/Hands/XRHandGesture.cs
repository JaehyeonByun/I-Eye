// /******************************************************************************
//  * File: XRHandGesture.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    /// <summary>
    /// Simple component serializing XrHandGesture
    /// Used to filter select gesture without using XRIT Interaction layer masks.
    /// See XRHandFilter, as an example of use. 
    /// </summary>
    public class XRHandGesture : MonoBehaviour, IHandGesture
    {
        [SerializeField] private XrHandGesture gesture;

        public XrHandGesture Gesture
        {
            get => gesture;
            set => gesture = value;
        }
    }
}