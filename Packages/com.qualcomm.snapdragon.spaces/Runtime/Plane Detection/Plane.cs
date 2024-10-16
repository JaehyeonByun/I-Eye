/******************************************************************************
 * File: Plane.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    public class Plane
    {
        public BoundedPlane BoundedPlane;
        public ulong ConvexHullId;

        public Plane(BoundedPlane boundedPlane, ulong convexHullId)
        {
            BoundedPlane = boundedPlane;
            ConvexHullId = convexHullId;
        }
    }
}
