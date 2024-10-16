// /******************************************************************************
//  * File: InterfaceAttribute.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions
{
    public class InterfaceAttribute : PropertyAttribute
    {
        public readonly Type Type;
        
        public InterfaceAttribute(Type type)
        {
            Type = type;
        }
    }
}