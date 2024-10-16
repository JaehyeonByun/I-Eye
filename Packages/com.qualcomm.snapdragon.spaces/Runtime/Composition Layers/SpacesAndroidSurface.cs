/******************************************************************************
 * File: SpacesAndroidSurface.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public class SpacesAndroidSurface
    {
        /// <summary>
        ///     The layerId of the SpacesCompositionLayer this surface was generated for.
        /// </summary>
        public ulong LayerId { get; private set; }

        /// <summary>
        ///     A pointer to a valid android.view.Surface object which can be used to render to the SpacesCompositionLayer.
        ///     This should be passed to Java code.
        /// </summary>
        public IntPtr ExternalSurface { get; private set; }

        public SpacesAndroidSurface(ulong layerId, IntPtr externalSurface)
        {
            LayerId = layerId;
            ExternalSurface = externalSurface;
        }
    }
}
