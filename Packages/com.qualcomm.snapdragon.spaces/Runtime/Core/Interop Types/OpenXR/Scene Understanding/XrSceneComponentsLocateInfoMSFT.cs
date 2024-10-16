/******************************************************************************
 * File: XrSceneComponentsLocateInfoMSFT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;

namespace Qualcomm.Snapdragon.Spaces
{
    internal struct XrSceneComponentsLocateInfoMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _baseSpace;
        private long _time;
        private uint _componentIdCount;

        // XrUuidMSFT[]
        private IntPtr _componentIds;
    }
}
