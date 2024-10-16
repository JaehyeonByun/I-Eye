/******************************************************************************
 * File: SpacesOpenXRFeature.cs
 * Copyright (c) 2021-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    public abstract class SpacesOpenXRFeature : OpenXRFeature
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GetInstanceProcAddrDelegate(IntPtr xrInstance, [MarshalAs(UnmanagedType.LPStr)] string name, ref IntPtr functionPtr);

        public static GetInstanceProcAddrDelegate GetInstanceProcAddrPtr;
#if UNITY_ANDROID && !UNITY_EDITOR
        protected const string InterceptOpenXRLibrary = "libInterceptOpenXR";
#else
        protected const string InterceptOpenXRLibrary = "InterceptOpenXR";
#endif

        public ulong SessionHandle { get; private set; }
        public ulong SystemIDHandle { get; private set; }
        public ulong InstanceHandle { get; private set; }
        public ulong SpaceHandle { get; private set; }
        public bool IsSessionRunning { get; private set; }
        public int SessionState { get; private set; }
        protected virtual bool IsRequiringBaseRuntimeFeature => false;

        protected override bool OnInstanceCreate(ulong instanceHandle)
        {
            InstanceHandle = instanceHandle;
            GetInstanceProcAddrPtr = (GetInstanceProcAddrDelegate)Marshal.GetDelegateForFunctionPointer(xrGetInstanceProcAddr, typeof(GetInstanceProcAddrDelegate));
            OnHookMethods();
            return true;
        }

        protected virtual void OnHookMethods()
        {
            HookMethod("xrGetSystemProperties", out _xrGetSystemProperties);
        }

        protected void HookMethod<TDelegate>(string methodName, out TDelegate delegatePointer) where TDelegate : Delegate
        {
            IntPtr functionPtr = IntPtr.Zero;
            if ((XrResult)GetInstanceProcAddrPtr((IntPtr)InstanceHandle, methodName, ref functionPtr) ==
                XrResult.XR_SUCCESS)
            {
                delegatePointer =
                    (TDelegate)Marshal.GetDelegateForFunctionPointer(functionPtr,
                        typeof(TDelegate));
            }
            else
            {
                delegatePointer = null;
            }
        }

        protected override void OnInstanceDestroy(ulong instanceHandle)
        {
            SystemIDHandle = 0;
            InstanceHandle = 0;
        }

        protected override void OnSystemChange(ulong systemIDHandle)
        {
            SystemIDHandle = systemIDHandle;
        }

        protected override void OnSessionCreate(ulong sessionHandle)
        {
            SessionHandle = sessionHandle;
        }

        protected override void OnSessionBegin(ulong sessionHandle)
        {
            IsSessionRunning = true;
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            SessionState = newState;
        }

        protected override void OnSessionEnd(ulong sessionHandle)
        {
            IsSessionRunning = false;
        }

        protected override void OnSessionDestroy(ulong sessionHandle)
        {
            IsSessionRunning = false;
            SessionHandle = 0;
        }

        protected override void OnAppSpaceChange(ulong spaceHandle)
        {
            SpaceHandle = spaceHandle;
        }

        protected IEnumerable<string> GetMissingExtensions(string extensions)
        {
            return extensions.Split(null)
                .Where(extension => !OpenXRRuntime.IsExtensionEnabled(extension));
        }

        protected virtual string GetXrLayersToLoad()
        {
            return "";
        }

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "RequestLayers")]
        protected static extern uint RequestLayers([MarshalAs(UnmanagedType.LPStr)] string requestedLayers);

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            var result = base.HookGetInstanceProcAddr(func);
            RequestLayers(GetXrLayersToLoad());
            return result;
        }

#if UNITY_EDITOR
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            if (!IsRequiringBaseRuntimeFeature)
            {
                return;
            }

            rules.Add(new ValidationRule(this)
            {
                message = "The \"Base Runtime\" feature has to be enabled for this feature.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings)
                    {
                        return false;
                    }

                    var feature = settings.GetFeature<BaseRuntimeFeature>();
                    if (!feature)
                    {
                        return false;
                    }

                    return feature.enabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings)
                    {
                        return;
                    }

                    var feature = settings.GetFeature<BaseRuntimeFeature>();
                    if (!feature)
                    {
                        return;
                    }

                    feature.enabled = true;
                },
                error = true
            });
        }
#endif

        protected virtual void OnGetSystemProperties()
        {
        }

        internal XrResult Internal_GetSystemProperties()
        {
            using ScopePtr<XrSystemProperties> systemPropertiesPtr = new();
            _systemProperties = new XrSystemProperties(SystemIDHandle);
            systemPropertiesPtr.Copy(_systemProperties);

            XrResult result = _xrGetSystemProperties(InstanceHandle, SystemIDHandle, systemPropertiesPtr.Raw);
            if (result == XrResult.XR_SUCCESS)
            {
                _systemProperties = systemPropertiesPtr.AsStruct();
                OnGetSystemProperties();
            }

            return result;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrGetSystemPropertiesDelegate(ulong instance, ulong systemId, IntPtr /*XrSystemProperties*/ systemProperties);

        private static xrGetSystemPropertiesDelegate _xrGetSystemProperties;

        private XrSystemProperties _systemProperties;

        internal XrSystemProperties SystemProperties => _systemProperties;

        /// <summary>
        ///     Called when the feature should check if it is in a useable state. Will always be called as part of a call to FeatureUseCheckUtility.IsFeatureUseable.
        ///     This will only be called after the feature instance is confirmed to be non-null, and if the feature is enabled.
        ///     A feature can optionally check some other state to determine whether it can be safely used.
        ///     This only applies to checking THIS feature, and NOT imposing requirements on OTHER features.
        /// </summary>
        /// <returns>True if feature is useable. False otherwise.</returns>
        protected internal virtual bool OnCheckIsFeatureUseable()
        {
            return SessionHandle != 0 && SystemIDHandle != 0;
        }

        /// <summary>
        ///     Called when the feature should check if it is in a useable state as part of a call to FeatureUseCheckUtility.IsFeatureUseable.
        ///     The result of the call to this check will be cached and must be unchanged for the lifecycle of an openXr instance.
        ///     This will only be called after the feature instance is confirmed to be non-null, and if the feature is enabled.
        ///     A feature can optionally check some other state to determine whether it can be safely used.
        ///     This only applies to checking THIS feature, and NOT imposing requirements on OTHER features.
        /// </summary>
        /// <returns>True if feature is useable. False otherwise.</returns>
        protected internal virtual bool OnCheckIsFeatureUseable_Cached()
        {
            return true;
        }
    }
}
