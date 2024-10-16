/******************************************************************************
 * File: BaseRuntimeFeature.ComponentVersioning.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    public readonly struct ComponentVersion
    {
        public readonly string ComponentName;
        public readonly string VersionIdentifier;
        public readonly string BuildIdentifier;
        public readonly string BuildDateTime;
        public readonly string SourceIdentifier;

        public ComponentVersion(string name, string versionIdentifier, string buildIdentifier, string buildDateTime, string sourceIdentifier)
        {
            ComponentName = name;
            VersionIdentifier = versionIdentifier;
            BuildIdentifier = buildIdentifier;
            BuildDateTime = buildDateTime;
            SourceIdentifier = sourceIdentifier;
        }
    }

    public partial class BaseRuntimeFeature
    {
        private readonly List<ComponentVersion> _componentVersions = new List<ComponentVersion>();

        public List<ComponentVersion> ComponentVersions
        {
            get
            {
                if (_componentVersions.Count != 0)
                {
                    return _componentVersions;
                }

                TryRetrieveComponentVersions();
                return _componentVersions;
            }
        }

        private bool TryRetrieveComponentVersions()
        {
            if (_xrGetComponentVersionsQCOM == null)
            {
                Debug.LogError("xrGetComponentVersionsQCOM method not found!");
                return false;
            }

            uint componentVersionCountOutput = 0;
            XrResult result = _xrGetComponentVersionsQCOM(InstanceHandle, 0, ref componentVersionCountOutput, IntPtr.Zero);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get componentVersionCountOutput: " + result);
                return false;
            }

            if (componentVersionCountOutput == 0)
            {
                Debug.Log("No components found.");
                return true;
            }

            using ScopeArrayPtr<XrComponentVersionQCOM> componentVersionsPtr = new((int)componentVersionCountOutput);
            var defaultComponentVersion = new XrComponentVersionQCOM(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);
            for (int i = 0; i < componentVersionCountOutput; i++)
            {
                componentVersionsPtr.Copy(defaultComponentVersion, i);
            }

            result = _xrGetComponentVersionsQCOM(InstanceHandle, componentVersionCountOutput, ref componentVersionCountOutput, componentVersionsPtr.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get component versions: " + result);
                return false;
            }

            for (int i = 0; i < componentVersionCountOutput; i++)
            {
                var componentVersion = componentVersionsPtr.AtIndex(i);
                _componentVersions.Add(new ComponentVersion(componentVersion.ComponentName, componentVersion.VersionIdentifier, componentVersion.BuildIdentifier, componentVersion.BuildDateTime, componentVersion.SourceIdentifier));
            }

            string componentVersionsString = "Enumerating component information from Spaces Services:";
            foreach (var componentVersion in _componentVersions)
            {
                componentVersionsString += "\nComponent '" +
                    componentVersion.ComponentName +
                    "'\n    Version Identifier: " +
                    componentVersion.VersionIdentifier;
                if (componentVersion.BuildIdentifier != String.Empty)
                {
                    componentVersionsString += "\n    Build Identifier: " + componentVersion.BuildIdentifier;
                }

                if (componentVersion.BuildDateTime != String.Empty)
                {
                    componentVersionsString += "\n    Build Date Time: " + componentVersion.BuildDateTime;
                }

                componentVersionsString += "\n";
            }

            Debug.Log(componentVersionsString);
            return true;
        }

        #region XR_QCOM_component_versioning bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrGetComponentVersionsQCOMDelegate(ulong instance, uint componentVersionCapacityInput, ref uint componentVersionCountOutput, IntPtr /* XrComponentVersionQCOM */ componentVersions);

        private static xrGetComponentVersionsQCOMDelegate _xrGetComponentVersionsQCOM;

        #endregion
    }
}
