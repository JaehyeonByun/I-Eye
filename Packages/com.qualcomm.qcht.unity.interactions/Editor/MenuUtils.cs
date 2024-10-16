// /******************************************************************************
//  * File: MenuUtils.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using QCHT.Interactions.Proximal;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QCHT.Interactions.Editor
{
    public static class MenuUtils
    {
        [MenuItem("GameObject/QCHT/Hand Tracking Manager", false, 0)]
        private static void CreateHandTrackingManager(MenuCommand menuCommand)
        {
            XRHandTrackingManager.GetOrCreate(XRHandTrackingManager.DefaultLeftHandPrefab,
                XRHandTrackingManager.DefaultRightHandPrefab);
        }

        [MenuItem("GameObject/QCHT/Hand Tracking Controllers", false, 0)]
        private static void CreateHandTrackingControllers(MenuCommand menuCommand)
        {
            const string handTrackingControllerRight = "Qualcomm Hand Controller Right";
            const string handTrackingControllerLeft = "Qualcomm Hand Controller Left";

            TryInstantiateController(handTrackingControllerLeft);
            TryInstantiateController(handTrackingControllerRight);
        }

        [MenuItem("GameObject/QCHT/Controllers", false, 0)]
        private static void CreateControllers(MenuCommand menuCommand)
        {
            const string controllerRight = "Qualcomm Controller Right";
            const string controllerLeft = "Qualcomm Controller Left";

            TryInstantiateController(controllerLeft);
            TryInstantiateController(controllerRight);
        }

        [MenuItem("GameObject/QCHT/XR Gaze Interactor", false, 0)]
        private static void CreateXRGazeInteractor(MenuCommand menuCommand)
        {
            const string gazeInteractor = "XR Gaze Interactor";

            TryInstantiateController(gazeInteractor);
        }
        
        [MenuItem("GameObject/QCHT/Snap Pose Manager", false, 11)]
        private static void CreateSnapPoseManager(MenuCommand menuCommand)
        {
            var go = new GameObject(nameof(XRHandInteractableSnapPoseManager));
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            go.AddComponent<XRHandInteractableSnapPoseManager>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/QCHT/Snap Pose Provider", false, 11)]
        private static void CreateSnapPoseProvider(MenuCommand menuCommand)
        {
            var go = new GameObject(nameof(XRHandInteractableSnapPoseProvider));
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            go.AddComponent<XRHandInteractableSnapPoseProvider>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/QCHT/Snap Pose", false, 11)]
        private static void CreateSnapPose(MenuCommand menuCommand)
        {
            var go = new GameObject(nameof(XRHandInteractableSnapPose));
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            var snapPose = go.AddComponent<XRHandInteractableSnapPose>();
            PresetSelector.ShowSelector(snapPose, null, false, null);
            var presets = Preset.GetDefaultPresetsForObject(snapPose);
            if (presets.Length > 0) presets[0].ApplyTo(snapPose);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        private static bool TryInstantiateController(string controller)
        {
            if (!TryInstantiatePrefab(controller, out var controllerObj))
            {
                return false;
            }

            var origin = XROriginUtility.GetOriginTransform();
            if (!origin)
                Debug.LogWarning(
                    $"[MenuUtils:TryInstantiateController] No XR Origin or AR Session Origin found! {controller} may not work properly");

            Transform parent = null;

            if (origin)
            {
                var xrOrigin = origin.GetComponent<XROrigin>();
                parent = xrOrigin ? xrOrigin.CameraFloorOffsetObject.transform : origin.transform;
            }

            controllerObj.transform.parent = parent;
            return true;
        }

        private static bool TryInstantiatePrefab(string prefabName, out GameObject instance)
        {
            var results = AssetDatabase.FindAssets(prefabName);

            GameObject prefab = null;
            foreach (var result in results)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(result);
                var prefabResult = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabResult != null && string.Equals(prefabResult.name, prefabName))
                {
                    prefab = prefabResult;
                    break;
                }
            }

            if (!prefab)
            {
                Debug.LogWarning($"[MenuUtils:TryInstantiatePrefab] Can't find prefab named {prefabName} in assets");
                instance = null;
                return false;
            }

            instance = Object.Instantiate(prefab);
            instance.name = prefab.name;
            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            Selection.activeObject = instance;
            return true;
        }
    }
}