/******************************************************************************
 * File: DualRenderFusionGameObjectHelper.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public class DualRenderFusionGameObjectHelper : MonoBehaviour
    {
        [MenuItem("GameObject/XR/Dual Render Fusion/Dynamic OpenXR Loader", false, 5)]
        public static void AddDynamicOpenXRLoaderGameObjectToScene(MenuCommand mc)
        {
            DynamicOpenXRLoader oldDynamicOpenXRLoader = FindObjectOfType<DynamicOpenXRLoader>(true);
            if (oldDynamicOpenXRLoader != null)
            {
                Debug.LogWarning($"There is a Dynamic OpenXR Loader component already present in the scene on the Game Object {oldDynamicOpenXRLoader.gameObject.name}. Skipping adding a new one.");
                return;
            }

            GameObject dynamicOpenXRLoaderGO = new GameObject("Dynamic OpenXR Loader");
            SpacesGlassStatus glassStatus = FindObjectOfType<SpacesGlassStatus>(true);
            if (glassStatus != null)
            {
                Debug.LogWarning("Dynamic OpenXR Loader created a Spaces Glass Status component, but one already exists in the scene. Remove the Spaces Glass Status component you have in your scene on the Game Object: " + glassStatus.gameObject.name);
            }

            DynamicOpenXRLoader dynamicOpenXRLoader = dynamicOpenXRLoaderGO.AddComponent<DynamicOpenXRLoader>();
            GameObjectUtility.SetParentAndAlign(dynamicOpenXRLoaderGO, mc.context as GameObject);
            Undo.RegisterCreatedObjectUndo(dynamicOpenXRLoaderGO, "Create " + dynamicOpenXRLoaderGO.name);
            Selection.activeObject = dynamicOpenXRLoaderGO;
        }

        [MenuItem("GameObject/XR/Dual Render Fusion/Spaces Glass Status", false, 10)]
        public static void AddSpacesGlassStatusGameObjectToScene(MenuCommand mc)
        {
            SpacesGlassStatus oldSpacesGlassStatus = FindObjectOfType<SpacesGlassStatus>(true);
            if (oldSpacesGlassStatus != null)
            {
                Debug.LogWarning($"There is a Spaces Glass Status component already present in the scene on the Game Object {oldSpacesGlassStatus.gameObject.name}. Skipping adding a new one.");
                return;
            }

            GameObject spacesGlassStatusGO = new GameObject("Spaces Glass Status");
            SpacesGlassStatus glassStatus = spacesGlassStatusGO.AddComponent<SpacesGlassStatus>();
            GameObjectUtility.SetParentAndAlign(spacesGlassStatusGO, mc.context as GameObject);
            Undo.RegisterCreatedObjectUndo(spacesGlassStatusGO, "Create " + spacesGlassStatusGO.name);
            Selection.activeObject = spacesGlassStatusGO;
        }

        [MenuItem("GameObject/XR/Dual Render Fusion/Host View", false, 10)]
        public static void AddSpacesHostViewGameObjectToScene(MenuCommand mc)
        {
            SpacesHostView oldSpacesHostView = FindObjectOfType<SpacesHostView>(true);
            if (oldSpacesHostView != null)
            {
                Debug.LogWarning($"There is a Spaces Host View component already present in the scene on the Game Object {oldSpacesHostView.gameObject.name}. Skipping adding a new one.");
                return;
            }

            GameObject spacesHostViewGO = new GameObject("Spaces Host View");
            SpacesHostView spacesHostView = spacesHostViewGO.AddComponent<SpacesHostView>();
            GameObjectUtility.SetParentAndAlign(spacesHostViewGO, mc.context as GameObject);
            Undo.RegisterCreatedObjectUndo(spacesHostViewGO, "Create " + spacesHostViewGO.name);
            Selection.activeObject = spacesHostViewGO;
        }

        [MenuItem("GameObject/XR/Dual Render Fusion/Fusion Simulator", false, 10)]
        public static void AddFusionSimulator(MenuCommand mc)
        {
            FusionSimulator oldfFusionSimulator = FindObjectOfType<FusionSimulator>(true);
            if (oldfFusionSimulator != null)
            {
                Debug.LogWarning($"There is a Fusion Simulator component already present in the scene on the Game Object {oldfFusionSimulator.gameObject.name}. Skipping adding a new one.");
                return;
            }

            GameObject fusionSimulatorGO = new GameObject("Fusion Simulator");
            fusionSimulatorGO.AddComponent<FusionSimulator>();
            GameObjectUtility.SetParentAndAlign(fusionSimulatorGO, mc.context as GameObject);
            Undo.RegisterCreatedObjectUndo(fusionSimulatorGO, "Create " + fusionSimulatorGO.name);
            Selection.activeObject = fusionSimulatorGO;
        }

        [MenuItem("GameObject/XR/Dual Render Fusion/Lifecycle Events", false, 10)]
        public static void AddLifecycleEvents(MenuCommand mc)
        {
            FusionLifecycleEvents oldFusionLifecycleEvents = FindObjectOfType<FusionLifecycleEvents>(true);
            if (oldFusionLifecycleEvents != null)
            {
                Debug.LogWarning($"There is a Fusion Lifecycle Events component already present in the scene on the Game Object {oldFusionLifecycleEvents.gameObject.name}. Skipping adding a new one.");
                return;
            }

            GameObject fusionLifecycleEventsGO = new GameObject("Fusion Lifecycle Events");
            fusionLifecycleEventsGO.AddComponent<FusionLifecycleEvents>();
            GameObjectUtility.SetParentAndAlign(fusionLifecycleEventsGO, mc.context as GameObject);
            Undo.RegisterCreatedObjectUndo(fusionLifecycleEventsGO, "Create " + fusionLifecycleEventsGO.name);
            Selection.activeObject = fusionLifecycleEventsGO;
        }
    }
}
#endif
