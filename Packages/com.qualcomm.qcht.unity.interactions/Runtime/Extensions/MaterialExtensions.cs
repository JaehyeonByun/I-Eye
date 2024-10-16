// /******************************************************************************
//  * File: MaterialExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class MaterialExtensions
    {
        public static void SetAlpha(this Material material, float value)
        {
            Color color = material.color;
            color.a = value;
            material.color = color;
        }
    }
}