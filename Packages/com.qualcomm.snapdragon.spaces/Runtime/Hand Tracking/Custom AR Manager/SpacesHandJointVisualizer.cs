/******************************************************************************
 * File: SpacesHandJointVisualizer.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete]
    [RequireComponent(typeof(SpacesHand))]
    public class SpacesHandJointVisualizer : MonoBehaviour
    {
        [Serializable]
        public struct JointMaterialFilter
        {
            [Tooltip("Filter will only apply if Name Filter is a sub-string of the XrHandJointEXT joint name.")]
            public string nameFilter;

            [Tooltip("Material to use for filtered joints.")]
            public Material material;
        }

        [Tooltip("Mesh to be used for all joints displayed.")]
        public Mesh JointMesh;

        [Tooltip("Default material to be used for all joints displayed. Can be overriden using the Joint Material Filters list.")]
        public Material DefaultMaterial;

        [Tooltip("Scale to be used for all joints displayed.")] [Range(0.005f, 0.05f)]
        public float JointMeshScale = 0.01f;

        [Tooltip("List of name filters to set joint materials individually or by group. Filters are applied sequentially, top-to-bottom.")]
        public JointMaterialFilter[] jointMaterialFilters;

        private readonly int _color = Shader.PropertyToID("_Color");
        private readonly Matrix4x4[] _pointsMatrix = new Matrix4x4[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
        private readonly MaterialPropertyBlock[] _propertyBlock = new MaterialPropertyBlock[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
        private SpacesHand _spacesHand;
        private Material[] _jointMaterials;
        private bool _usingMaterialFilters;
        private bool _universalRenderingPipeline;

        private void Start()
        {
            _spacesHand = GetComponent<SpacesHand>();
            _universalRenderingPipeline = GraphicsSettings.currentRenderPipeline != null;
            _usingMaterialFilters = jointMaterialFilters.Length > 0;
            if (_usingMaterialFilters || _universalRenderingPipeline)
            {
                GenerateJointMaterials();
            }
        }

        private void Update()
        {
            UpdatePointMatrices(_spacesHand);
            if (_usingMaterialFilters || _universalRenderingPipeline)
            {
                for (var i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                {
                    Material currentJointMaterial = _jointMaterials[i];
                    if (currentJointMaterial != null)
                    {
                        Graphics.DrawMesh(JointMesh, _pointsMatrix[i], currentJointMaterial, 0, null);
                    }
                }
            }
            else
            {
                /* Directly renders instances in batch. */
                Graphics.DrawMeshInstanced(JointMesh, 0, DefaultMaterial, _pointsMatrix, _pointsMatrix.Length);
            }
        }

        private void UpdatePointMatrices(SpacesHand hand)
        {
            for (var i = 0; i < _pointsMatrix.Length; i++)
            {
                var pose = hand.Joints[i].Pose;
                _pointsMatrix[i].SetTRS(pose.position, pose.rotation, Vector3.one * JointMeshScale);
            }
        }

        private void GenerateJointMaterials()
        {
            _jointMaterials = new Material[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            /* Apply name filters sequentially */
            foreach (var filter in jointMaterialFilters)
            {
                for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                {
                    if (Enum.GetName(typeof(XrHandJointEXT), (XrHandJointEXT)i).Contains(filter.nameFilter))
                    {
                        _jointMaterials[i] = filter.material;
                    }
                }
            }

            /* Replace unassigned joint materials with default material */
            for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
            {
                _jointMaterials[i] ??= DefaultMaterial;
            }
        }
    }
}
