/******************************************************************************
 * File: XrPosef.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrPosef
    {
        private XrQuaternionf _orientation;
        private XrVector3f _position;

        public XrPosef(Pose pose)
        {
            _orientation = new XrQuaternionf(pose.rotation);
            _position = new XrVector3f(pose.position);
        }

        public XrPosef(XrQuaternionf orientation, XrVector3f position)
        {
            _orientation = orientation;
            _position = position;
        }

        public Pose ToPose()
        {
            return new Pose(_position.ToVector3(), _orientation.ToQuaternion());
        }

        public override string ToString()
        {
            return String.Join("\n",
                "[XrPosef]",
                $"Orientation:\t{_orientation}",
                $"Position:\t{_position}");
        }
    }
}
