// /******************************************************************************
//  * File: XRHandTrackingSubsystem.Provider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.SubsystemsImplementation;

namespace QCHT.Interactions.Core
{
    public partial class XRHandTrackingSubsystem
    {
        public abstract class Provider : SubsystemProvider<XRHandTrackingSubsystem>
        {
            /// <summary>
            /// Gets the space which provider hand data are formatted. 
            /// </summary>
            public XrSpace Space { get; protected set; } = XrSpace.XR_HAND_WORLD;

            /// <summary>
            /// Tries to get hand data from provider. 
            /// </summary>
            /// <param name="handedness"> Handedness </param>
            /// <param name="isTracked"> Is Tracked? </param>
            /// <param name="rootPose"> Root pose in origin space. </param>
            /// <param name="joints"> Joints poses either in local, world or xr origin space. </param>
            /// <param name="radiuses"> Hand joints radii. Wrist radius should be clamped between .5 and 1.6 </param>
            /// <returns></returns>
            public abstract HandTrackingStatus TryUpdateHandData(XrHandedness handedness, ref bool isTracked, ref Pose rootPose, ref Pose[] joints, ref float[] radiuses);

            public override void Start() { }

            public override void Stop() { }

            public override void Destroy() { }
        }
    }
}