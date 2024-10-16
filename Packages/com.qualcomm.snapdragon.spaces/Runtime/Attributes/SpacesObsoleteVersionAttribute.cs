/******************************************************************************
 * File: SpacesObsoleteVersionAttribute.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    // A property attribute containing information about when a property was marked obsolete, and when it is planned to be removed completely (if known).
    internal class SpacesObsoleteVersionAttribute : PropertyAttribute
    {
        // Should the property be drawn with additional emphasis (add icon, adjust tooltip).
        public readonly bool ShowObsoletePropertyEmphasis = true;

        // The version in which this property was marked obsolete.
        public string ObsoleteSinceVersion;

        // The version in which this property is planned to be removed.
        public string PlannedForRemovalInVersion = String.Empty;

        public SpacesObsoleteVersionAttribute(string obsoleteSinceVersion)
        {
            ObsoleteSinceVersion = obsoleteSinceVersion;
        }

        public SpacesObsoleteVersionAttribute(string obsoleteSinceVersion, bool showObsoletePropertyEmphasis)
        {
            ShowObsoletePropertyEmphasis = showObsoletePropertyEmphasis;
            ObsoleteSinceVersion = obsoleteSinceVersion;
        }

        public SpacesObsoleteVersionAttribute(string obsoleteSinceVersion, string plannedForRemovalInVersion)
        {
            ObsoleteSinceVersion = obsoleteSinceVersion;
            PlannedForRemovalInVersion = plannedForRemovalInVersion;
        }

        public SpacesObsoleteVersionAttribute(string obsoleteSinceVersion, string plannedForRemovalInVersion, bool showObsoletePropertyEmphasis)
        {
            ShowObsoletePropertyEmphasis = showObsoletePropertyEmphasis;
            ObsoleteSinceVersion = obsoleteSinceVersion;
            PlannedForRemovalInVersion = plannedForRemovalInVersion;
        }
    }
}
