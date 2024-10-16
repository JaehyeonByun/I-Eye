/******************************************************************************
 * File: PlaneDataCollection.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public struct PlaneDataCollection
    {
        public List<Vector3> vertices;
        public List<uint> indices;
        public Vector2 extents;
        public bool reverseExtentPlaneWindingOrder;
    }
}
