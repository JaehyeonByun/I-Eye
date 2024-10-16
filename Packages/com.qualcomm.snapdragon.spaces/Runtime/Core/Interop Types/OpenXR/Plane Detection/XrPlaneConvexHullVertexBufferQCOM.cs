/******************************************************************************
 * File: XrPlaneConvexHullVertexBufferQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrPlaneConvexHullVertexBufferQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _vertexCapacityInput;

        // uint
        private IntPtr _vertexCapacityOutput;

        // XrVector3f
        private IntPtr _vertices;

        public XrPlaneConvexHullVertexBufferQCOM(IntPtr vertexCapacityOutput)
        {
            _type = XrStructureType.XR_TYPE_PLANE_CONVEX_HULL_VERTEX_BUFFER_QCOM;
            _next = IntPtr.Zero;
            _vertexCapacityInput = 0;
            _vertexCapacityOutput = vertexCapacityOutput;
            _vertices = IntPtr.Zero;
        }

        public XrPlaneConvexHullVertexBufferQCOM(uint vertexCapacityInput, IntPtr vertexCapacityOutput, IntPtr vertices)
        {
            _type = XrStructureType.XR_TYPE_PLANE_CONVEX_HULL_VERTEX_BUFFER_QCOM;
            _next = IntPtr.Zero;
            _vertexCapacityInput = vertexCapacityInput;
            _vertexCapacityOutput = vertexCapacityOutput;
            _vertices = vertices;
        }
    }
}
