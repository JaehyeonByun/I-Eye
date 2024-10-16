// /******************************************************************************
//  * File: IHandedness.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;

namespace QCHT.Interactions.Hands
{
    public interface IHandedness
    {
        public XrHandedness Handedness { get; }
    }
}