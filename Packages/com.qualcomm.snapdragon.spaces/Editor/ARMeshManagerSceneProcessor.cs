/******************************************************************************
 * File: ARMeshManagerSceneProcessor.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    // Process each scene to find AR Mesh Manager.
    // If an AR Mesh Manager is found and there is no Spaces AR Mesh Manager Config report it.
    // Or, validate the values for the config and the ar mesh manager to ensure that they are setup sensibly for use with spaces mesh provider.
    internal class ARMeshManagerSceneProcessor : OpenXRFeatureBuildHooks, IProcessSceneWithReport
    {
        private bool isSpatialMeshingFeatureEnabled;
        public override Type featureType => typeof(SpatialMeshingFeature);
        public override int callbackOrder => 0;

        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {
            var activeLoaders = XRGeneralSettings.Instance?.Manager?.activeLoaders;
            if (activeLoaders?.Any(loader => loader.GetType() == typeof(OpenXRLoader)) != true)
            {
                // No OpenXR Loader enabled. Don't need to process this
                return;
            }

            if (!isSpatialMeshingFeatureEnabled)
            {
                // Not attempting to use the spatial meshing feature so don't need to validate
                return;
            }

            var sceneContainsMeshManager = false;
            var sceneContainsSpacesConfigComponent = false;

            ARMeshManager foundMeshManager = null;
            SpacesARMeshManagerConfig foundConfig = null;

            foreach (var component in UnityEngine.Object.FindObjectsOfType(typeof(ARMeshManager)))
            {
                var monoBehaviour = component as MonoBehaviour;
                if (monoBehaviour != null && monoBehaviour.enabled)
                {
                    sceneContainsMeshManager = true;
                    foundMeshManager = component as ARMeshManager;
                    break;
                }
            }

            foreach (var component in UnityEngine.Object.FindObjectsOfType(typeof(SpacesARMeshManagerConfig)))
            {
                var monoBehaviour = component as MonoBehaviour;
                if (monoBehaviour != null && monoBehaviour.enabled)
                {
                    sceneContainsSpacesConfigComponent = true;
                    foundConfig = component as SpacesARMeshManagerConfig;
                    break;
                }
            }

            if (sceneContainsMeshManager && !sceneContainsSpacesConfigComponent)
            {
                Debug.LogWarning($"Scene {scene.name} contains an AR Mesh Manager component without an accompanying Spaces AR Mesh Manager Config component.\n" +
                    "The spaces meshing provider can perform better when the optional Spaces AR Mesh Manager Config component is used to control the setting of Mesh Vertex Attributes (Normals, Colors, Tangents, Texture Coordinates) for generated meshes.");
            }

            if (sceneContainsMeshManager)
            {
                if (sceneContainsSpacesConfigComponent)
                {
                    if (foundConfig.CalculateCpuNormals && !foundMeshManager.normals)
                    {
                        Debug.LogWarning("Configuration value from Spaces AR Mesh Manager Config: Normals are being calculated, but the AR Mesh Manager is configured to not forward these normals. The results of querying meshFilter.mesh.normals array will be empty or null.");
                    }
                    else if (foundMeshManager.normals && !foundConfig.CalculateCpuNormals)
                    {
                        Debug.LogWarning("Configuration value from AR Mesh Manager: Normals were requested from the AR Mesh Manager, but the option to Calculate CPU Normals in Spaces AR Mesh Manager Config is disabled. The result of querying the meshFilter.mesh.normals array will be empty or null.");
                    }
                }

                if (foundMeshManager.tangents)
                {
                    Debug.LogWarning("Configuration value from AR Mesh Manager: Tangents not currently supported by spaces meshing provider.");
                }

                if (foundMeshManager.textureCoordinates)
                {
                    Debug.LogWarning("Configuration value from AR Mesh Manager: Texture Coordinates not currently supported by spaces meshing provider.");
                }

                if (foundMeshManager.colors)
                {
                    Debug.LogWarning("Configuration value from AR Mesh Manager: Colors not currently supported by spaces meshing provider.");
                }

                if (foundMeshManager.density != 0.5f)
                {
                    Debug.LogWarning("Configuration value from AR Mesh Manager: Density is not currently supported by spaces meshing provider.");
                }

                if (foundMeshManager.concurrentQueueSize != 4)
                {
                    Debug.LogWarning("Configuration value from AR Mesh Manager: Concurrent Queue Size is not currently supported by spaces meshing provider. Only returns 1 mesh.");
                }
            }
        }

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
            isSpatialMeshingFeatureEnabled = true;
        }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }
    }
}
