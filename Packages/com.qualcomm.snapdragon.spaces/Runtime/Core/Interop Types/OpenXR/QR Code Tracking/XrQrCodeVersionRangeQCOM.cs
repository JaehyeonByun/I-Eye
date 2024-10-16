/******************************************************************************
 * File: XrQrCodeVersionRangeQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrQrCodeVersionRangeQCOM
    {
        private XrQrCodeSymbolTypeQCOM _symbolType;
        private byte _minQrVersion;
        private byte _maxQrVersion;

        public XrQrCodeVersionRangeQCOM(XrQrCodeSymbolTypeQCOM type, byte minQrVersion, byte maxQrVersion)
        {
            _symbolType = type;
            _minQrVersion = minQrVersion;
            _maxQrVersion = maxQrVersion;
        }
    }
}
