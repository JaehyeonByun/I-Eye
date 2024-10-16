/******************************************************************************
 * File: XRHandTrackingSubsystemDescriptor.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine.SubsystemsImplementation;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html", false)]
    public class XRHandTrackingSubsystemDescriptor : SubsystemDescriptorWithProvider<XRHandTrackingSubsystem, XRHandTrackingSubsystem.Provider>
    {
        public struct Cinfo : IEquatable<Cinfo>
        {
            public string id { get; set; }
            public Type providerType { get; set; }
            public Type subsystemTypeOverride { get; set; }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = id.GetHashCode();
                    hashCode = (hashCode * 4999559) + providerType.GetHashCode();
                    hashCode = (hashCode * 4999559) + subsystemTypeOverride.GetHashCode();
                    return hashCode;
                }
            }

            public bool Equals(Cinfo other)
            {
                return ReferenceEquals(id, other.id) && ReferenceEquals(providerType, other.providerType) && ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride);
            }

            public override bool Equals(object obj)
            {
                return obj is Cinfo && Equals((Cinfo)obj);
            }

            public static bool operator ==(Cinfo lhs, Cinfo rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(Cinfo lhs, Cinfo rhs)
            {
                return !lhs.Equals(rhs);
            }
        }

        private XRHandTrackingSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
        }

        public static void Create(Cinfo cinfo)
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRHandTrackingSubsystemDescriptor(cinfo));
        }
    }
}
