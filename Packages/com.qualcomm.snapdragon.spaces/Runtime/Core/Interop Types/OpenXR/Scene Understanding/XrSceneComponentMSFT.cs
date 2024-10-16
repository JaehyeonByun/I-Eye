/******************************************************************************
 * File: XrSceneComponentMSFT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSceneComponentMSFT
    {
        private XrSceneComponentTypeMSFT _componentType;
        private XrUuidMSFT _id;
        private XrUuidMSFT _parentId;
        private long _updateTime;
    }
}
