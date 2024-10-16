// /******************************************************************************
//  * File: IHandPoke.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Hands
{
    public interface IHandPokeReceiver
    {
        public Vector3? PokePoint { get; set; }
    }
}