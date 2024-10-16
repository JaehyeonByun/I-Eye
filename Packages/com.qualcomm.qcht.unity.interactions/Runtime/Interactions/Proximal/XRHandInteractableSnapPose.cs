// /******************************************************************************
//  * File: XRHandInteractableSnapPose.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Proximal
{
    /// <summary>
    /// Apply pose snap on XRInteractable.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class XRHandInteractableSnapPose : MonoBehaviour, IHandPoseProvider
    {
        [SerializeField] private HandData handData = HandData.Default;

        public HandData Data
        {
            get => handData;
            set => handData = value;
        }

        public float Scale => transform.localScale.x;
    }
}