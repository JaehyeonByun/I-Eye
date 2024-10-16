/******************************************************************************
 * File: FusionLifecycleEvents.cs
 * Copyright (c)2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Qualcomm.Snapdragon.Spaces
{
    [AddComponentMenu("XR/Dual Render Fusion/Fusion Lifecycle Events")]
    public class FusionLifecycleEvents : MonoBehaviour
    {
        [Header("Settings")] [Tooltip("If true, will rebroadcast the events OnOpenXRAvailable, OnOpenXRUnavailable, OnOpenXRStarted and OnOpenXRStopped on scene load.")]
        public bool RebroadcastOnSceneLoad = true;

        [Header("Dynamic OpenXR Loader")]
        [Space]
        [Tooltip("Availability is broadcast when the DynamicOpenXrLoader is enabled, as well as before attempting to start OpenXR, and after stopping OpenXR." +
            "\nOnOpenXrAvailable will be broadcast at these times when the glasses are connected. " +
            "\nThis indicates that OpenXR _can_ be initialised, but it is not actively running. " +
            "\nThis can be used by a developer to signal to a user that the glasses are connected correctly, but this is not a good moment for the developer to start accessing OpenXR features." +
            "\n\nGlass connection status can be queried at any time through SpacesGlassStatus.Instance.GlassConnectionState")]
        public UnityEvent OnOpenXRAvailable;

        [Tooltip("Availability is broadcast when the DynamicOpenXrLoader is enabled, as well as before attempting to start OpenXR, and after stopping OpenXR." +
            "\nOnOpenXrUnavailable will be broadcast at these times when the glasses are disconnected. " +
            "\nThis indicates that OpenXR _cannot_ be initialised, and is not actively running. " +
            "\nThis can be used by a developer to signal to a user that the glasses are disconnected and should be reconnected to view openXr content." +
            "\n\\nGlass connection status can be queried at any time through SpacesGlassStatus.Instance.GlassConnectionState")]
        public UnityEvent OnOpenXRUnavailable;

        [Tooltip("OnOpenXRStarting is broadcast after the OpenXR loader has been initialised, just before OpenXR subsystems are started." +
            "\nAt this time it should be clear which OpenXR features are enabled, and have been initialised correctly, but subsystems will not be running.")]
        public UnityEvent OnOpenXRStarting;

        [Tooltip("OnOpenXRStarted is broadcast after OpenXR subsystems have started." +
            "\nAll XR content should now be usable. This is the event developers should subscribe to when wanting to start up optional XR content in the scene.")]
        public UnityEvent OnOpenXRStarted;

        [Tooltip("OnOpenXRStopping is broadcast before OpenXR subsystems are stopped, and the OpenXR loader is deinitialised." +
            "\nDevelopers should subscribe to this event in order to shutdown content which requires OpenXR while it is safe to do so.")]
        public UnityEvent OnOpenXRStopping;

        [Tooltip("OnOpenXRStopped is broadcast after OpenXR subsystems are stopped, and the OpenXR loader has been deinitialised." +
            "\nWhen this event is received, OpenXR is not loaded. Features can be queried to see if they are intended to be used, but are not actually usable. OpenXR subsystems are not running." +
            "\nImmediately after this event, Availability will be broadcast indicating whether OpenXR can be restarted (are glasses connected or not).")]
        public UnityEvent OnOpenXRStopped;

        [Header("Spaces Glass Status")]
        [Space]
        [Tooltip("When glasses are idle they are not being worn (e.g. this is triggered due to proximity sensor timeout)." +
            "\nThis can be used as a hint to save power by reducing rendering quality, or reducing priority of updates to xr only content." +
            "\n\nGlass active/idle status can be queried at any time through SpacesGlassStatus.Instance.GlassActiveState")]
        public UnityEvent OnIdle;

        [Tooltip("When glasses are active they are being worn and displaying content. Glasses cannot be active if they are disconnected." +
            "\n\nGlass active/idle status can be queried at any time through SpacesGlassStatus.Instance.GlassActiveState")]
        public UnityEvent OnActive;

        [Header("Spaces Host View")] [Space] [Tooltip("The On Host View Enabled event is fired when using a device which supports Dual Render Fusion, so that the application can choose to display or hide elements as appropriate." + "\nE.g. to enable the Dual Render Fusion host view on AR devices." + "\n\nThis is not an indication about whether a supported AR device is connected to the Host device.")]
        public UnityEvent OnHostViewEnabled;

        [Tooltip("The On Host View Disable event is fired when using a device which does not support Dual Render Fusion, so that the application can choose to display or hide elements as appropriate." + "\nE.g. to enable alternative user interface elements on MR/VR devices." + "\n\nThis is not an indication about whether a supported AR device is connected to the Host device.")]
        public UnityEvent OnHostViewDisabled;

        private void Start()
        {
            if (gameObject.scene.name == "DontDestroyOnLoad")
            {
                Debug.LogWarning("The Fusion Lifecycle Events are attached to a Game Object marked as Don't Destroy On Load" +
                    "\nThis may not be what you want." +
                    "\nFor each scene, you should populate a custom instance of this component with the correct way to respond to those events based on the current scene." +
                    "\ne.g. showing a notification informing the user to connect the glasses in one scene, exiting the current scene and returning to a menu for another.");
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DynamicOpenXRLoader.Instance.OnOpenXRAvailable = OnOpenXRAvailable;
            DynamicOpenXRLoader.Instance.OnOpenXRUnavailable = OnOpenXRUnavailable;
            DynamicOpenXRLoader.Instance.OnOpenXRStarting = OnOpenXRStarting;
            DynamicOpenXRLoader.Instance.OnOpenXRStarted = OnOpenXRStarted;
            DynamicOpenXRLoader.Instance.OnOpenXRStopping = OnOpenXRStopping;
            DynamicOpenXRLoader.Instance.OnOpenXRStopped = OnOpenXRStopped;
            SpacesGlassStatus.Instance.OnIdle = OnIdle;
            SpacesGlassStatus.Instance.OnActive = OnActive;
            SpacesHostView.Instance.OnHostViewEnabled = OnHostViewEnabled;
            SpacesHostView.Instance.OnHostViewDisabled = OnHostViewDisabled;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene _loadedScene, LoadSceneMode _mode)
        {
            if (!RebroadcastOnSceneLoad)
            {
                return;
            }

#if !UNITY_EDITOR
            if (DynamicOpenXRLoader.Instance.AreSubsystemsRunning)
            {
                DynamicOpenXRLoader.Instance.BroadcastXrAvailability();
                OnOpenXRStarted?.Invoke();
            }
            else
            {
                OnOpenXRStopped?.Invoke();
                DynamicOpenXRLoader.Instance.BroadcastXrAvailability();
            }
#endif
        }
    }
}
