/******************************************************************************
 * File: XrSceneObserverCreateInfoMSFT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal class XrSceneObserverCreateInfoMSFT
    {
        private XrStructureType _type;
        private IntPtr _next;

        public XrSceneObserverCreateInfoMSFT()
        {
            _type = XrStructureType.XR_TYPE_SCENE_OBSERVER_CREATE_INFO_MSFT;
            _next = IntPtr.Zero;
        }
    }
}
