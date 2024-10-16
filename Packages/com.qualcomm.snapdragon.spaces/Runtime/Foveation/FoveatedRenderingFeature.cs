/******************************************************************************
 * File: FoveatedRenderingFeature.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.XR.OpenXR;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    public enum FoveationLevel
    {
        [Tooltip("No foveation - no change to visual fidelity, no performance impact.")]
        None,
        [Tooltip("Less foveation - periphery visual fidelity is reduced slightly for a small performance increase.")]
        Low,
        [Tooltip("Medium foveation - periphery visual fidelity is reduced for a medium performance increase.")]
        Medium,
        [Tooltip("High foveation - periphery visual fidelity is reduced significantly for a higher performance increase.")]
        High
    }

    #if UNITY_EDITOR
    [OpenXRFeature(UiName = FeatureName,
            BuildTargetGroups = new[]
            {
                BuildTargetGroup.Android
            },
            Company = "Qualcomm",
            Desc = "Enables Foveated Rendering feature on Snapdragon Spaces enabled devices",
            DocumentationLink = "",
            OpenxrExtensionStrings = FeatureExtensions,
            Version = "0.23.0",
            Required = false,
            Category = FeatureCategory.Feature,
            FeatureId = FeatureID)]
    #endif
    internal sealed class FoveatedRenderingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Foveated Rendering";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.foveatedrendering";
        public const string FeatureExtensions = "XR_FB_swapchain_update_state XR_FB_foveation XR_FB_foveation_configuration XR_FB_foveation_vulkan";

        protected override bool IsRequiringBaseRuntimeFeature => true;
        private bool FoveationEnabled = false;

        [Tooltip("The foveation level to use at application startup.\nThis is applied immediately at application start and affects any splash logos visible in XR." +
            "\nHigher levels of foveation reduce visual fidelity in peripheral vision, in exchange for increased performance.")]
        public FoveationLevel DefaultFoveationLevel = FoveationLevel.None;

        public FoveationLevel CurrentFoveationLevel => _currentFoveationLevel;
        private FoveationLevel _currentFoveationLevel;
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            base.OnInstanceCreate(xrInstance);
            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any())
            {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }

            FoveationEnabled = true;
            Internal_SetEnableFoveation(true);

            SetCreateSwapchainCallback(SetDefaultFoveationLevel);

            return true;
        }

        /// <summary>
        ///     Set the foveation level to use for the application.
        ///     Higher levels of foveation reduce visual fidelity in peripheral vision, in exchange for increased performance.
        /// </summary>
        /// <param name="level">The level to set.</param>
        public void SetFoveationLevel(FoveationLevel level)
        {
            // using scopePtr doesnt survive until command buffer is executed on render thread.
            // need to deallocate this memory when it is processed
            // See BaseRuntimeFeature.RunOnRenderThreadWithData for call to Marshal.Free
            IntPtr foveationLevelPtr = Marshal.AllocHGlobal(Marshal.SizeOf<int>());
            Marshal.StructureToPtr((int) level, foveationLevelPtr, false);

            SpacesRenderEventUtility.SubmitRenderEventAndData(SpacesRenderEvent.SetFoveationLevel, foveationLevelPtr);
        }

        /// <summary>
        ///     Set the foveation level to use for the application.
        ///     This is only to be called from within a valid graphics context (e.g. the render thread).
        /// </summary>
        /// <param name="level">The level to set.</param>
        internal void SetFoveationLevel_GraphicsContext(FoveationLevel level)
        {
            if (FoveationEnabled)
            {
                Internal_SetFoveationLevel(InstanceHandle, SessionHandle, (int)level);
                _currentFoveationLevel = level;
            }
        }

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetEnableFoveation")]
        private static extern void Internal_SetEnableFoveation(bool enableFoveation);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetFoveationLevel")]
        private static extern void Internal_SetFoveationLevel(ulong instance, ulong session, int level);

        private delegate void CreateSwapchainCallback();

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetCreateSwapchainCallback")]
        private static extern void SetCreateSwapchainCallback(CreateSwapchainCallback callback);

        [MonoPInvokeCallback(typeof(CreateSwapchainCallback))]
        private static void SetDefaultFoveationLevel()
        {
            var foveatedRenderingFeature = OpenXRSettings.Instance.GetFeature<FoveatedRenderingFeature>();
            if (FeatureUseCheckUtility.IsFeatureUseable(foveatedRenderingFeature))
            {
                foveatedRenderingFeature.SetFoveationLevel_GraphicsContext(foveatedRenderingFeature.DefaultFoveationLevel);
            }
        }
    }
}
