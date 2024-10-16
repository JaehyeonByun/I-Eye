// /******************************************************************************
//  * File: IHandGesture.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;

namespace QCHT.Interactions.Hands
{
    public interface IHandGesture
    {
        public XrHandGesture Gesture { get; }
    }
}