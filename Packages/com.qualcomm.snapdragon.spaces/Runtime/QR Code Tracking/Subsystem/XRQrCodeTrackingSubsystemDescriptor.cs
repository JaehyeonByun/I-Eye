/******************************************************************************
 * File: XRQrCodeTrackingSubsystemDescriptor.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine.SubsystemsImplementation;

namespace Qualcomm.Snapdragon.Spaces
{
    public class XRQrCodeTrackingSubsystemDescriptor : SubsystemDescriptorWithProvider<XRQrCodeTrackingSubsystem, XRQrCodeTrackingSubsystem.Provider>
    {
        public struct Cinfo : IEquatable<Cinfo>
        {
            public string id { get; set; }
            public Type providerType { get; set; }
            public Type subsystemTypeOverride { get; set; }

            public override int GetHashCode()
            {
                int hashCode = id.GetHashCode();
                hashCode = (hashCode * 4999559) + providerType.GetHashCode();
                hashCode = (hashCode * 4999559) + subsystemTypeOverride.GetHashCode();
                return hashCode;
            }

            public bool Equals(Cinfo other)
            {
                return ReferenceEquals(id, other.id) && ReferenceEquals(providerType, other.providerType) && ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride);
            }

            public override bool Equals(object obj)
            {
                return obj is Cinfo other && Equals(other);
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

        private XRQrCodeTrackingSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
        }

        public static void Create(Cinfo cinfo)
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRQrCodeTrackingSubsystemDescriptor(cinfo));
        }
    }
}
