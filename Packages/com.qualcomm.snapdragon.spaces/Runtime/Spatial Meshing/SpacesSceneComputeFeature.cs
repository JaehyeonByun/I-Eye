/******************************************************************************
 * File: SpacesSceneComputeFeature.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.ComponentModel;

namespace Qualcomm.Snapdragon.Spaces
{
    internal enum SpacesSceneComputeFeature
    {
        [Description("Specifies that plane data for objects should be included in the resulting scene. Requests XR_SCENE_COMPUTE_FEATURE_PLANE_MSFT feature to be computed.")]
        PLANE = 1,
        [Description("Specifies that planar meshes for objects should be included in the resulting scene. Requests XR_SCENE_COMPUTE_FEATURE_PLANE_MESH_MSFT feature to be computed.")]
        PLANE_MESH = 1 << 1,
        [Description("Specifies that 3D visualization meshes for objects should be included in the resulting scene. Requests XR_SCENE_COMPUTE_FEATURE_VISUAL_MESH_MSFT feature to be computed.")]
        VISUAL_MESH = 1 << 2,
        [Description("Specifies that 3D collider meshes for objects should be included in the resulting scene. Requests XR_SCENE_COMPUTE_FEATURE_COLLIDER_MESH_MSFT feature to be computed.")]
        COLLIDER_MESH = 1 << 3
    }
}
