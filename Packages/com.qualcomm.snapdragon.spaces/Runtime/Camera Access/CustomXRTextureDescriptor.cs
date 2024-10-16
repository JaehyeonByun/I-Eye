/******************************************************************************
 * File: CustomXRTextureDescriptor.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Qualcomm.Snapdragon.Spaces
{
    // NOTE(PA): This struct is a copy of XRTextureDescriptor.cs and exists only to copy data to it
    // via marshaling. This struct should NOT be used as a replacement for XRTextureDescriptor.
    [StructLayout(LayoutKind.Sequential)]
    internal struct CustomXRTextureDescriptor
    {
        public IntPtr nativeTexture;
        public int width;
        public int height;
        public int mipmapCount;
        public TextureFormat format;
        public int propertyNameId;
        public bool valid{
            get { return (nativeTexture != IntPtr.Zero) && (width > 0) && (height > 0); }
        }
        public int depth;
        public TextureDimension dimension;
    }
}
