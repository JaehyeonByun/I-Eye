/******************************************************************************
 * File: FusionFeature.FeatureValidators.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class FusionFeature
    {
        const string MainCameraTag = "MainCamera";
        const string UntaggedTag = "Untagged";

        Camera FindActiveHostCamera()
        {
            SpacesHostView hostView = FindObjectOfType<SpacesHostView>(true);
            if (!hostView)
                return null;

            return hostView.phoneCamera;
        }

        private ValidationRule Recommend_Scene_ARSessionObjectExists()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: Dual Render Fusion recommends an AR Session in the scene.",
                checkPredicate = () => FindObjectOfType<ARSession>(true),
                fixIt = () =>
                {
                    if (FindObjectOfType<ARSession>(true))
                    {
                        return;
                    }

                    GameObject arSessionGO= new GameObject("AR Session");
                    arSessionGO.AddComponent<ARSession>();
                    Undo.RegisterCreatedObjectUndo(arSessionGO, "Create AR Session");

                    if (!FindObjectOfType<ARInputManager>(true))
                    {
                        arSessionGO.AddComponent<ARInputManager>();
                    }
                    Debug.Log("Added AR Session Object to the Scene (" + arSessionGO.name + ")");
                },
                error = false,
                fixItMessage = "Adds a new GameObject \"AR Session\" to the scene. This object has the \"AR Session\" and \"AR Input Manager\" components."
            };
        }

        private ValidationRule Recommend_Scene_URP_MobileCameraTargetEyeNone()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: URP Projects need to manually check the Mobile Camera to set the Target Eye to None in the Inspector. 'Fix' will not handle this.",
                checkPredicate = () => !UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset,
                fixIt = () =>
                {
                    if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset)
                    {
                        return;
                    }

                    Debug.LogWarning("Camera Target Eye checks cannot be done programmatically for URP at this time. Manually check the Cameras for Target Eye and ensure all non-XR Cameras are set to 'None' instead of 'Both'.");
                },
                error = false,
                fixItMessage = "Cannot fix automatically."
            };
        }

        private ValidationRule Recommend_Scene_NonXRCameraTargetEyeNone()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: For Dual Render Fusion, each non-XR Camera needs to be set to Target Eye (none).",
                checkPredicate = () =>
                {
                    Camera xrCamera = OriginLocationUtility.GetOriginCamera();

                    Camera[] cameras = FindObjectsOfType<Camera>(true);
                    foreach (Camera camera in cameras)
                    {
                        if (xrCamera != camera && !camera.targetTexture)
                        {
                            if (camera.stereoTargetEye != StereoTargetEyeMask.None)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                },
                fixIt = () =>
                {
                    Camera xrCamera = OriginLocationUtility.GetOriginCamera();

                    int group = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("Set Target Eye for non-XR Cameras");
                    Camera[] cameras = FindObjectsOfType<Camera>(true);
                    foreach (Camera camera in cameras)
                    {
                        if (xrCamera != camera)
                        {
                            if (camera.stereoTargetEye != StereoTargetEyeMask.None && !camera.targetTexture )
                            {
                                Undo.RecordObject(camera, "Set Target Eye for " + camera.name);
                                camera.stereoTargetEye = StereoTargetEyeMask.None;
                                Debug.Log("Updated Camera Target Eye to None (" + camera.name + ")");
                            }
                        }
                    }
                    Undo.CollapseUndoOperations(group);
                },
                error = false,
                fixItMessage = "Sets the Target Eye for all non-XR cameras to None."
            };
        }

        private bool Check_MultipleCamerasTaggedMain()
        {
            bool foundMainTag = false;
            Camera[] cameras = FindObjectsOfType<Camera>(true);
            foreach (Camera camera in cameras)
            {
                if (Check_IsCameraTaggedMain(camera))
                {
                    if (!foundMainTag)
                    {
                        foundMainTag = true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Check_IsCameraTaggedMain(Camera camera)
        {
            return camera && !string.IsNullOrEmpty(camera.tag) && camera.tag.Equals(MainCameraTag);
        }

        private ValidationRule Recommend_Scene_XrCameraIsMain()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: Multiple cameras are tagged as MainCamera. Select 'Fix' to untag the XR Camera.",
                checkPredicate = () =>
                {
                    if (Check_MultipleCamerasTaggedMain())
                    {
                        return !Check_IsCameraTaggedMain(OriginLocationUtility.GetOriginCamera());
                    }

                    return true;
                },
                fixIt = () =>
                {
                    if (!Check_MultipleCamerasTaggedMain() || !Check_IsCameraTaggedMain(OriginLocationUtility.GetOriginCamera()))
                    {
                        return;
                    }

                    Camera xrCamera = OriginLocationUtility.GetOriginCamera();

                    Undo.RecordObject(xrCamera, "Untag XR Camera");
                    xrCamera.tag = UntaggedTag;
                    Debug.Log("Untagged XR Camera (" + xrCamera.name + ")");
                },
                error = false,
                fixItAutomatic = false,
                fixItMessage = "Removes the MainCamera tag from the XR Camera."
            };
        }

        private ValidationRule Recommend_Scene_HostViewCameraIsMain()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: Multiple cameras are tagged as MainCamera. Select 'Fix' to untag Host View Camera.",
                checkPredicate = () =>
                {
                    if (Check_MultipleCamerasTaggedMain())
                    {
                        return !Check_IsCameraTaggedMain(FindActiveHostCamera());
                    }

                    return true;
                },
                fixIt = () =>
                {
                    if (!Check_MultipleCamerasTaggedMain() || !Check_IsCameraTaggedMain(FindActiveHostCamera()))
                    {
                        return;
                    }

                    Camera hostCamera = FindActiveHostCamera();
                    Undo.RecordObject(hostCamera, "Untag camera " + hostCamera.name);
                    hostCamera.tag = UntaggedTag;
                    Debug.Log("Untagged Camera (" + hostCamera.name + ")");
                },
                error = false,
                fixItAutomatic = false,
                fixItMessage = "Removes the MainCamera tag from the Host View Camera."
            };
        }

        private ValidationRule Recommend_Scene_MultipleCamerasTaggedMain()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: Multiple cameras are tagged as MainCamera. 'Fix' will not handle this.",
                checkPredicate = () => !Check_MultipleCamerasTaggedMain(),
                fixIt = () =>
                {
                    if (!Check_MultipleCamerasTaggedMain())
                    {
                        return;
                    }

                    Debug.LogWarning("Multiple cameras are tagged as the Main camera. Cannot programmatically resolve the intent. Check each camera and ensure the Main tag is set on the correct camera.");
                },
                error = false,
                fixItAutomatic = false,
                fixItMessage = "Cannot fix automatically. Check each camera in the scene manually and ensure that only one camera has the MainCamera tag."
            };
        }

        private ValidationRule Recommend_Scene_XrCameraTargetDisplay1()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: XR Camera target display is not Display 1, recommend adding Fusion Simulator for adjusting the target display at Runtime.",
                checkPredicate = () =>
                {
                    Camera xrCamera = OriginLocationUtility.GetOriginCamera();
                    if (xrCamera)
                    {
                        if (xrCamera.targetDisplay != 0)
                        {
                            return FindObjectOfType<FusionSimulator>(true);
                        }
                    }
                    return true;
                },
                fixIt = () =>
                {
                    if (!FindObjectOfType<FusionSimulator>(true))
                    {
                        DualRenderFusionGameObjectHelper.AddFusionSimulator(new MenuCommand(null));
                    }
                },
                error = false,
                fixItMessage = "Set the \"Target Display\" for the XR Camera to Display 1."
            };
        }

        private static List<Type> _disabledByFusionLifecycleBlacklist = new List<Type>()
        {
            typeof(InputActionManager),
            typeof(EventSystem)
        };

        private struct LifecycleEventCallee
        {
            public GameObject calleeGO;
            public string eventName;
        }

        private struct ControlledByLifecycleEvent
        {
            public LifecycleEventCallee eventCallee;
            public Type componentType;
            public GameObject componentGO;
        }

        private void GetObjectsCallingMethodOnUnityEvent(UnityEvent ev, string loggableEventName, string methodName,  ref HashSet<LifecycleEventCallee> objectEventNamePair)
        {
            for (int ix = 0; ix < ev.GetPersistentEventCount(); ++ix)
            {
                if (ev.GetPersistentMethodName(ix) == methodName)
                {
                    var obj = ev.GetPersistentTarget(ix) as GameObject;
                    objectEventNamePair.Add(new LifecycleEventCallee() { calleeGO = obj, eventName = loggableEventName });
                }
            }
        }

        private void GetObjectsSetActiveByFusionLifecycleEvents(out HashSet<LifecycleEventCallee> objectsSetActive)
        {
            objectsSetActive = new HashSet<LifecycleEventCallee>();
            // Check all fusion lifecycle events for any calls to SetActive
            // Can't differentiate between calls to SetActive (true) and (false)
            var fusionLifecycleEvents = FindObjectOfType<FusionLifecycleEvents>(true);
            if (fusionLifecycleEvents)
            {
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnHostViewDisabled, nameof(fusionLifecycleEvents.OnHostViewDisabled), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnHostViewEnabled, nameof(fusionLifecycleEvents.OnHostViewEnabled), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnActive, nameof(fusionLifecycleEvents.OnActive), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnIdle, nameof(fusionLifecycleEvents.OnIdle), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnOpenXRAvailable, nameof(fusionLifecycleEvents.OnOpenXRAvailable), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnOpenXRUnavailable, nameof(fusionLifecycleEvents.OnOpenXRUnavailable), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnOpenXRStarted, nameof(fusionLifecycleEvents.OnOpenXRStarted), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnOpenXRStarting, nameof(fusionLifecycleEvents.OnOpenXRStarting), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnOpenXRStopped, nameof(fusionLifecycleEvents.OnOpenXRStopped), "SetActive", ref objectsSetActive);
                GetObjectsCallingMethodOnUnityEvent(fusionLifecycleEvents.OnOpenXRStopping, nameof(fusionLifecycleEvents.OnOpenXRStopping), "SetActive", ref objectsSetActive);
            }

            // additionally check AR Session, AR Session Origin
            // cant differentiate between calls to SetActive (true) and (false)
            var sessionOriginObject = OriginLocationUtility.GetOriginTransform(true).gameObject;
            var session = FindObjectOfType<ARSession>(true).gameObject;

            objectsSetActive.Add(new LifecycleEventCallee() { calleeGO = session, eventName = "OnGlassConnected"});
            objectsSetActive.Add(new LifecycleEventCallee() { calleeGO = session, eventName =  "OnGlassDisconnected"});
            objectsSetActive.Add(new LifecycleEventCallee() { calleeGO = sessionOriginObject, eventName = "OnGlassConnected"});
            objectsSetActive.Add(new LifecycleEventCallee() { calleeGO = sessionOriginObject, eventName = "OnGlassDisconnected"});
        }

        private ValidationRule Recommend_Scene_BlacklistedComponentsDisabledByFusionLifecycleEvents()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: Input Action Manager should not be a child of any GameObject which is disabled by fusion lifecycle events.",
                checkPredicate = () =>
                {
                    var inputActionManager = FindObjectOfType<InputActionManager>(true);
                    if (inputActionManager)
                    {
                        GetObjectsSetActiveByFusionLifecycleEvents(out var objectsSetActive);
                        foreach (var callee in objectsSetActive)
                        {
                            foreach (var type in _disabledByFusionLifecycleBlacklist)
                            {
                                if (callee.calleeGO.transform.GetComponentInChildren(type, true))
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    return true;
                },
                fixItAutomatic = false,
                fixIt = () =>
                {
                    List<ControlledByLifecycleEvent> data = new();
                    GetObjectsSetActiveByFusionLifecycleEvents(out var objectsSetActive);
                    foreach (var callee in objectsSetActive)
                    {
                        foreach (var type in _disabledByFusionLifecycleBlacklist)
                        {
                            var component = callee.calleeGO.transform.GetComponentInChildren(type, true);
                            if (component)
                            {
                                data.Add(new ControlledByLifecycleEvent()
                                {
                                    eventCallee = callee,
                                    componentType = type,
                                    componentGO = component.gameObject
                                });
                            }
                        }
                    }

                    string LogOutput(List<ControlledByLifecycleEvent> info)
                    {
                        string loggedOutput = "";
                        foreach (var entry in info)
                        {
                            loggedOutput += $"- Component of type [{entry.componentType}] found on object [{entry.componentGO}] is a child of [{entry.eventCallee.calleeGO}] which calls SetActive as a result of the fusion lifecycle event {entry.eventCallee.eventName}.\n";
                        }

                        return loggedOutput;
                    }

                    Debug.LogWarning("Certain components should not be attached to Game Objects which are children of any Game Object being disabled by Fusion Lifecycle Events." +
                        $"\nCannot programmatically tell which calls to SetActive on the following objects might disable these components:\n" + LogOutput(data) +
                        "\nThese objects should be checked manually, and the restricted components should be reparented if possible." +
                        "\nFailure to do this can result in side-effects." +
                        "\ne.g. In the case of InputActionManager / EventSystem types on disabled parent objects -> the Host View (mobile phone) display might not respond to touch inputs when the glasses are disconnected.");
                },
                error = false
            };
        }

        private ValidationRule Recommend_Scene_LifecycleEventsExists()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: There is no Fusion Lifecycle Events in the current scene.",
                checkPredicate = () => FindObjectOfType<FusionLifecycleEvents>(true),
                fixIt = () =>
                {
                    if (!FindObjectOfType<FusionLifecycleEvents>(true))
                    {
                        DualRenderFusionGameObjectHelper.AddLifecycleEvents(new MenuCommand(null));
                    }
                },
                fixItMessage = "Add a GameObject \"Fusion Lifecycle Events\" to the scene. This object has the \"Fusion Lifecycle Events\" component."
            };
        }

        private bool Check_MoreThanOneOrigin()
        {
#if AR_FOUNDATION_5_0_OR_NEWER
            return FindObjectsOfType<XROrigin>().Length > 1;
#else
            return FindObjectsOfType<XROrigin>().Length + FindObjectsOfType<ARSessionOrigin>().Length > 1;
#endif
        }

        private ValidationRule Required_Scene_OnlyOneXrOrigin()
        {
            return new ValidationRule(this)
            {
                message = "Scene Requirement: There should be only one active XR Origin in the scene.",
                checkPredicate = () => !Check_MoreThanOneOrigin(),
                fixIt = () =>
                {
                    XROrigin[] origins = FindObjectsOfType<XROrigin>();
                    if (origins != null && origins.Length >= 1)
                    {
                        string[] names = new string[origins.Length];
                        for (int i = 0; i < origins.Length; i++)
                        {
                            names[i] = origins[i].gameObject.name;
                        }
                        Debug.LogError("Please manually disable or remove unneeded XR Origin objects in the scene ([" + string.Join("],[", names) + "]).");
                    }

#if !AR_FOUNDATION_5_0_OR_NEWER
                    ARSessionOrigin[] arOrigins = FindObjectsOfType<ARSessionOrigin>();
                    if (arOrigins != null && arOrigins.Length >= 1)
                    {
                        string[] names = new string[arOrigins.Length];
                        for (int i = 0; i < arOrigins.Length; i++)
                        {
                            names[i] = arOrigins[i].gameObject.name;
                        }
                        Debug.LogError("Please manually disable or remove unneeded AR Session Origin objects in the scene ([" + string.Join("],[", names) + "]).");
                    }
#endif
                },
                error = true,
                fixItMessage = "Cannot fix automatically. Will log a list of all GameObjects with XR Origin or AR Session Origin components. Manually remove the extra components from the scene until only 1 remains."
            };
        }

        private ValidationRule Required_Scene_HostViewRendersAfterXr()
        {
            return new ValidationRule(this)
            {
                message = "Scene Requirement: Dual Render Fusion requires the mobile Camera to render after the XR Camera.",
                checkPredicate = () =>
                {
                    Camera xrCamera = OriginLocationUtility.GetOriginCamera();
                    Camera hostCamera = FindActiveHostCamera();

                    if (xrCamera && hostCamera)
                    {
                        if (hostCamera.depth <= xrCamera.depth)
                        {
                            return false;
                        }
                    }
                    return true;

                },
                fixIt = () =>
                {
                    Camera xrCamera = OriginLocationUtility.GetOriginCamera();
                    Camera hostCamera = FindActiveHostCamera();

                    Undo.RecordObject(hostCamera, "Modified Depth in " + hostCamera.name);

                    float oldDepth = hostCamera.depth;
                    hostCamera.depth = xrCamera.depth + 1;
                    Debug.Log("Fixed Camera (" + hostCamera.name + ") depth to " + hostCamera.depth + " from " + oldDepth);
                },
                error = true,
                fixItMessage = "Set the \"Depth\" of the Host View (mobile) Camera to render immediately after the XR Camera."
            };
        }

        private ValidationRule Required_Scene_XrCameraExists()
        {
            return new ValidationRule(this)
            {
                message = "Scene Requirement: Dual Render Fusion requires a camera attached to an AR Session Origin or XR Origin.",
                checkPredicate = () => OriginLocationUtility.GetOriginCamera(),
                fixIt = () =>
                {
                    if (OriginLocationUtility.GetOriginCamera())
                    {
                        return;
                    }

                    ARSessionOrigin aso = FindObjectOfType<ARSessionOrigin>(true);
                    XROrigin xro = FindObjectOfType<XROrigin>(true);

                    int group = Undo.GetCurrentGroup();

                    Transform cameraParentTransform;

                    // If no origins, add ARSessionOrigin
                    if (!aso && !xro)
                    {
                        GameObject originObject = new GameObject("AR Session Origin");
                        aso = originObject.AddComponent<ARSessionOrigin>();
                        Debug.Log("Added AR Session Origin to the Scene (" + originObject.name + ")");

                        Undo.RegisterCreatedObjectUndo(originObject, "Create AR Session Origin");

                        Undo.SetCurrentGroupName("Create AR Session Origin");
                    }
                    else
                    {
                        Undo.SetCurrentGroupName("Add XR Camera");
                    }

                    GameObject xrCameraObject = new GameObject("XR Camera");
                    var xrCamera = xrCameraObject.AddComponent<Camera>();
                    xrCamera.tag = UntaggedTag;

                    if (aso)
                    {
                        aso.camera = xrCamera;
                        cameraParentTransform = aso.transform;
                    }
                    else
                    {
                        if (!xro.CameraFloorOffsetObject)
                        {
                            xro.CameraFloorOffsetObject = new GameObject("Camera Offset");
                            xro.CameraFloorOffsetObject.transform.SetParent(xro.transform, false);
                            xro.CameraFloorOffsetObject.transform.position = new Vector3(0, xro.CameraYOffset, 0);
                            xro.CameraFloorOffsetObject.transform.rotation = Quaternion.identity;
                        }
                        xro.Camera = xrCamera;
                        cameraParentTransform = xro.CameraFloorOffsetObject.transform;
                    }

                    xrCameraObject.transform.SetParent(cameraParentTransform, false);
                    xrCamera.clearFlags = CameraClearFlags.SolidColor;
                    xrCamera.backgroundColor = Color.black;
                    xrCamera.farClipPlane = 1000;
                    xrCamera.stereoTargetEye = StereoTargetEyeMask.Both;
                    xrCamera.targetDisplay = 1;

                    xrCameraObject.AddComponent<ARCameraManager>();
                    xrCameraObject.AddComponent<ARCameraBackground>();
                    Debug.Log("Added XR Camera to the Scene (" + xrCamera.name + ")");

                    TrackedPoseDriver trackedPoseDriver = xrCameraObject.AddComponent<TrackedPoseDriver>();
                    var positionAction = new InputAction("Position", binding: "<XRHMD>/centerEyePosition", expectedControlType: "Vector3");
                    positionAction.AddBinding("<HandheldARInputDevice>/devicePosition");
                    var rotationAction = new InputAction("Rotation", binding: "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion");
                    rotationAction.AddBinding("<HandheldARInputDevice>/deviceRotation");
                    trackedPoseDriver.positionInput = new InputActionProperty(positionAction);
                    trackedPoseDriver.rotationInput = new InputActionProperty(rotationAction);

                    DualRenderFusionGameObjectHelper.AddFusionSimulator(new MenuCommand(null));

                    Undo.CollapseUndoOperations(group);
                },
                error = true,
                fixItMessage = "Adds an AR Session Origin if necessary. Add a new Game Object \"XR Camera\" as a child of the session origin. This object contains the \"Camera\", \"AR Camera Manager\", \"AR Camera Background\", and \"Tracked Posed Driver\" components." +
                    "\nAdds a new Game Object \"Fusion Simulator\". This object contains the \"Fusion Simulator\" component."
            };
        }

        private ValidationRule Required_Scene_SpacesHostViewExists()
        {
            return new ValidationRule(this)
            {
                message = "Scene Requirement: Dual Render Fusion requires a Spaces Host View component in order to receive events about the availability of the host viewer."
                    + "\nThis allows a single apk to run on both Dual Render Fusion compatible Host/Viewer device combinations, and MR/VR devices." +
                    "\nAdditionally the camera attached to this Game Object should be used to display the fusion host (mobile) display.",
                checkPredicate = () =>
                {
                    return FindObjectOfType<SpacesHostView>(true);
                },
                fixIt = () =>
                {
                    if (!FindObjectOfType<SpacesHostView>(true))
                    {
                        DualRenderFusionGameObjectHelper.AddSpacesHostViewGameObjectToScene(new MenuCommand(null));
                    }
                },
                error = true,
                fixItMessage = "Adds a new Game Object \"Spaces Host View\". This object has the \"Spaces Host View\" component."
            };
        }

        private ValidationRule Recommend_Scene_DynamicOpenXrLoaderExists(XRGeneralSettings generalSettings)
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: Use the Dynamic OpenXR Loader component to manage the OpenXR lifecycle of the application.",
                checkPredicate = () =>
                {
                    if (generalSettings.InitManagerOnStart)
                    {
                        return true;
                    }

                    return FindObjectOfType<DynamicOpenXRLoader>(true);
                },
                fixIt = () =>
                {
                    if (!FindObjectOfType<DynamicOpenXRLoader>(true))
                    {
                        DualRenderFusionGameObjectHelper.AddDynamicOpenXRLoaderGameObjectToScene(new MenuCommand(null));
                    }
                },
                error = false,
                fixItMessage = "Adds a new Game Object \"Dynamic OpenXR Loader\". This object has the \"Dynamic OpenXR Loader\" and the \"Spaces Glass Status\" components."
            };
        }

        private ValidationRule Required_Scene_SpacesGlassStatusExists(XRGeneralSettings generalSettings)
        {
            return new ValidationRule(this)
            {
                message = "Scene Requirement: Dual Render Fusion requires a Spaces Glass Status component in order to receive events about the connection of an XR viewer.",
                checkPredicate = () =>
                {
                    if (generalSettings.InitManagerOnStart)
                    {
                        return true;
                    }

                    return FindObjectOfType<SpacesGlassStatus>(true);
                },
                fixIt = () =>
                {
                    if (!FindObjectOfType<SpacesGlassStatus>(true))
                    {
                        DualRenderFusionGameObjectHelper.AddSpacesGlassStatusGameObjectToScene(new MenuCommand(null));
                    }
                },
                error = true,
                fixItMessage = "Adds a new Game Object \"Spaces Glass Status\". This object has the \"Spaces Glass Status\" component. This object does not have the \"Dynamic OpenXR Loader\" component, which is recommended to handle the connection events."
            };
        }

        private ValidationRule Recommended_Project_InitManagerOnStart(XRGeneralSettings generalSettings)
        {
            return new ValidationRule(this)
            {
                message = "Project Recommendation: Dual Render Fusion projects should not \"Initialize XR on Startup\". Instead, make use of the Dynamic OpenXR Loader component to manage the lifecycle of OpenXR in the application.",
                checkPredicate = () => !generalSettings.InitManagerOnStart,
                fixIt = () =>
                {
                    generalSettings.InitManagerOnStart = false;
                },
                fixItMessage = "Disables Project Settings > XR Plug-In Management > Initialize Xr on Startup"
            };
        }

        private ValidationRule Recommend_Scene_FusionSimulatorExists()
        {
            return new ValidationRule(this)
            {
                message = "Scene Recommendation: Use the Fusion Simulator component to preview the different displays of XR and Host View cameras.",
                checkPredicate = () =>
                {
                    if (!this.SimulateFusionDevice)
                    {
                        return true;
                    }

                    return FindObjectOfType<FusionSimulator>(true);
                },
                fixIt = () =>
                {
                    if (!this.SimulateFusionDevice || FindObjectOfType<FusionSimulator>(true))
                    {
                        return;
                    }

                    DualRenderFusionGameObjectHelper.AddFusionSimulator(new MenuCommand(null));
                },
                error = false,
                fixItMessage = "Adds a new Game Object \"Fusion Simulator\". This object has the \"Fusion Simulator\" component."
            };
        }

        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            if (!this.enabled)
            {
                return;
            }

            var openXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (!openXRSettings)
            {
                return;
            }

            var baseRuntimeFeature = openXRSettings.GetFeature<BaseRuntimeFeature>();
            if (!baseRuntimeFeature || !baseRuntimeFeature.enabled)
            {
                return;
            }

            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (!settings || !settings.Manager)
            {
                return;
            }

            var isOpenXRLoaderActive = settings.Manager.activeLoaders?.Any(loader => loader.GetType() == typeof(OpenXRLoader));

            if (ValidateOpenScene)
            {
                rules.Add(Recommend_Scene_ARSessionObjectExists());
                rules.Add(Recommend_Scene_URP_MobileCameraTargetEyeNone());
                rules.Add(Recommend_Scene_NonXRCameraTargetEyeNone());
                rules.Add(Recommend_Scene_XrCameraIsMain());
                rules.Add(Recommend_Scene_HostViewCameraIsMain());
                rules.Add(Recommend_Scene_XrCameraTargetDisplay1());
                rules.Add(Recommend_Scene_MultipleCamerasTaggedMain());
                rules.Add(Recommend_Scene_DynamicOpenXrLoaderExists(settings));
                rules.Add(Recommend_Scene_LifecycleEventsExists());
                rules.Add(Recommend_Scene_FusionSimulatorExists());
                rules.Add(Recommend_Scene_BlacklistedComponentsDisabledByFusionLifecycleEvents());
                rules.Add(Required_Scene_OnlyOneXrOrigin());
                rules.Add(Required_Scene_HostViewRendersAfterXr());
                rules.Add(Required_Scene_XrCameraExists());
                rules.Add(Required_Scene_SpacesHostViewExists());
                rules.Add(Required_Scene_SpacesGlassStatusExists(settings));
            }

            rules.Add(Recommended_Project_InitManagerOnStart(settings));
        }
    }
}
#endif
