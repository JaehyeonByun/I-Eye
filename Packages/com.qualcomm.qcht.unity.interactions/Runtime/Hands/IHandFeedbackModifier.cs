// /******************************************************************************
//  * File: IHandFeedbackModifier.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;

namespace QCHT.Interactions.Hands
{
    public interface IHandFeedbackModifier
    {
        public List<HandPartState> Hovered { get; }
        public List<HandPartState> Selected { get; }
    }
}