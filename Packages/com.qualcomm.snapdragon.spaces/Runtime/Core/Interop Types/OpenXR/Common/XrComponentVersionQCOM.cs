/******************************************************************************
 * File: XrComponentVersionQCOM.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrComponentVersionQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ComponentName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string VersionIdentifier;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string BuildIdentifier;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string BuildDateTime;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string SourceIdentifier;

        public XrComponentVersionQCOM(string componentName, string versionIdentifier, string buildIdentifier, string buildDateTime, string sourceIdentifier)
        {
            _type = XrStructureType.XR_TYPE_COMPONENT_VERSION_QCOM;
            _next = IntPtr.Zero;
            ComponentName = componentName;
            VersionIdentifier = versionIdentifier;
            BuildIdentifier = buildIdentifier;
            BuildDateTime = buildDateTime;
            SourceIdentifier = sourceIdentifier;
        }
    }
}
