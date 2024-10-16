/******************************************************************************
 * File: SpacesARMeshManagerConfig.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    // Configuration component which controls performance options for the meshing subsystem provider.
    // Using this component allows the developer to gain back some performance by asking the provider not to calculate the unnecessary data.
    [RequireComponent(typeof(ARMeshManager))]
    public class SpacesARMeshManagerConfig : MonoBehaviour
    {
        private const string Library = "libMeshingProvider";

        [SerializeField]
        [Tooltip("If enabled, cpu normals are calculated for each vertex.\n\n" +
            "Some vertices may be repeated in the mesh (using different indices). This can be caused by hard edges in detected geometry.\n\n" +
            "For every index in the mesh, each triangle referencing that index contributes to the resulting normal.\n\n" +
            "But different indices (of repeated vertices) can have different normals.")]
        private bool _calculateCpuNormals = true;

        [SerializeField]
        [Tooltip("If enabled, cpu normals are smoothed for each vertex. All neighbouring triangles referencing a vertex with the same space but a different index will be treated as if they were the same vertex\n\n" +
            "This can cause hard edges in detected geometry to be less well defined.\n\n" +
            "Calculating smoothed normals has an additional impact on performance.")]
        private bool _useSmoothedNormals;

        [SerializeField]
        [Tooltip("Epsilon value to use when comparing vertices when smoothing normals. The value given is the epsilon to use when comparing a coordinate value of 1, and is scaled relative to floating point size.\n\n" +
            "Typically a value of 1.0E-5 works well, although larger or smaller values may work for your needs.")]
        private float _smoothingEpsilon = 1.0E-5f;

        [Tooltip("If enabled, a color value is requested for each vertex.\n\nThis feature not currently implemented on this platform.")]
        private bool _colors = false;

        private MeshVertexAttributes _meshVertexAttributes;

        [Tooltip("If enabled, a tangent is requested for each vertex.\n\nThis feature not currently implemented on this platform.")]
        private bool _tangents = false;

        [Tooltip("If enabled, a UV texture coordinate is requested for each vertex.\n\nThis feature not currently implemented on this platform.")]
        private bool _textureCoordinates = false;

        private ARMeshManager _meshManager;

        private FieldInfo _meshManagerSubsystem;

        public bool CalculateCpuNormals
        {
            get => _calculateCpuNormals;
            set
            {
                if (value != _calculateCpuNormals)
                {
                    _calculateCpuNormals = value;
                    UpdateMeshVertexAttributes();
                }
            }
        }

        public bool UseSmoothedNormals
        {
            get => _useSmoothedNormals;
            set
            {
                _useSmoothedNormals = value;
                Internal_SetUseSmoothedNormals(_useSmoothedNormals, _smoothingEpsilon);
            }
        }

        // Not sure if this should be exposed to public API for now.
        private float SmoothingEpsilon
        {
            get => _smoothingEpsilon;
            set
            {
                _smoothingEpsilon = value;
                Internal_SetUseSmoothedNormals(_useSmoothedNormals, _smoothingEpsilon);
            }
        }

        private void Awake()
        {
            _meshManager = GetComponent<ARMeshManager>();
            _meshManagerSubsystem = typeof(ARMeshManager).GetField("m_Subsystem", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void OnEnable()
        {
            UpdateMeshVertexAttributes();
            Internal_SetUseSmoothedNormals(_useSmoothedNormals, _smoothingEpsilon);
            _meshManager.meshesChanged += UpdateMeshTransforms;
        }

        private void OnDisable()
        {
            _meshManager.meshesChanged -= UpdateMeshTransforms;

            ResetMeshManagerSubsystemProperty();
        }

        [DllImport(Library, EntryPoint = "SetMeshVertexAttributesFlags")]
        private static extern void Internal_SetMeshVertexAttributesFlags(int meshVertexAttributeFlags);

        [DllImport(Library, EntryPoint = "SetUseSmoothedNormals")]
        private static extern void Internal_SetUseSmoothedNormals(bool useSmoothedNormals, float epsilon);

        private void UpdateMeshVertexAttributes()
        {
            _meshVertexAttributes = MeshVertexAttributes.None;

            _meshVertexAttributes |= _calculateCpuNormals ? MeshVertexAttributes.Normals : MeshVertexAttributes.None;
            _meshVertexAttributes |= _colors ? MeshVertexAttributes.Colors : MeshVertexAttributes.None;
            _meshVertexAttributes |= _tangents ? MeshVertexAttributes.Tangents : MeshVertexAttributes.None;
            _meshVertexAttributes |= _textureCoordinates ? MeshVertexAttributes.UVs : MeshVertexAttributes.None;

            Internal_SetMeshVertexAttributesFlags((int)_meshVertexAttributes);
        }

        [DllImport(Library, EntryPoint = "FetchMeshLocations")]
        private static extern bool Internal_FetchMeshLocations(IntPtr numMeshes, IntPtr pose);

        public void UpdateMeshTransforms(ARMeshesChangedEventArgs args)
        {
            List<Transform> meshTransforms = _meshManager.meshes.ToList().ConvertAll(MeshFilter => MeshFilter.transform);

            using ScopePtr<uint> numMeshesPtr = new();
            numMeshesPtr.Copy(0);

            Internal_FetchMeshLocations(numMeshesPtr.Raw,  IntPtr.Zero);

            int numMeshes = (int)numMeshesPtr.AsStruct();
            if (numMeshes != meshTransforms.Count)
            {
                Debug.LogError($"Number of meshes returned by provider {numMeshes}, and number of transforms to update {meshTransforms.Count} do not match");
                return;
            }

            using ScopeArrayPtr<XrPosef> posesPtr = new(numMeshes);

            if (!Internal_FetchMeshLocations(numMeshesPtr.Raw, posesPtr.Raw))
            {
                Debug.LogError("Failed to fetch mesh locations");
                return;
            }

            for (int i = 0; i < meshTransforms.Count; ++i)
            {
                Pose pose = posesPtr.AtIndex(i).ToPose();

                meshTransforms[i].transform.localPosition = pose.position;
                meshTransforms[i].transform.localRotation = pose.rotation;
            }
        }

        private void ResetMeshManagerSubsystemProperty()
        {
            if (_meshManagerSubsystem != null && _meshManager != null)
            {
                _meshManagerSubsystem.SetValue(_meshManager, null);
            }
        }
    }
}
