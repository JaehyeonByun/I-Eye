/******************************************************************************
 * File: BaseRuntimeFeature.AndroidThreadSettings.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        private static xrSetAndroidApplicationThreadKHRDelegate _xrSetAndroidApplicationThreadKHR;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _threadUtility = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.serviceshelper.ThreadUtility");
#endif

        /// <summary>
        ///     Set a thread type for the calling thread.
        ///     This makes use of an openXr mechanism to register the threads with the runtime.
        /// </summary>
        /// <param name="threadType">The thread type to assign</param>
        /// <param name="detachJNI">Set this to true if not being called on the UnityMain thread (almost always). Set to false only if this is being called on the UnityMain thread.</param>
        internal void SetThreadHint(SpacesThreadType threadType, bool detachJNI = true)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (AndroidJNI.AttachCurrentThread() < 0)
            {
                Debug.LogError("Failed to attach current thread to AndroidJNI cannot get thread id");
                return;
            }

            int threadId = _threadUtility.CallStatic<int>("GetTid");
            Debug.Log($"Configuring thread (tid: {threadId}) as {threadType}.");

            // detach from JNI after the call to set android application thread khr - unless this is the UnityMain thread!
            if (detachJNI && AndroidJNI.DetachCurrentThread() < 0)
            {
                Debug.LogError($"Failed to detach current thread from AndroidJNI. This thread {threadId} is likely to leak memory, or cause a crash on shutdown.");
                return;
            }

            XrResult result = _xrSetAndroidApplicationThreadKHR(SessionHandle, (XrAndroidThreadTypeKHR)threadType, (uint)threadId);
            if (result != XrResult.XR_SUCCESS)
            {
                // If the call to set thread affinity fails, performance will be degraded!
                Debug.LogError($"On trying to set thread tid: {threadId} as {threadType}, xr call returned {result}");
            }
#endif
        }

        private void ConfigureXRAndroidApplicationThreads()
        {
            // main thread hint
            SetThreadHint(SpacesThreadType.SPACES_THREAD_TYPE_APPLICATION_MAIN, false);

            // issue command buffer render event to execute on render thread
            // if not using some sort of multi-threaded rendering, this would execute on the main thread which may not be desirable.
            if (SystemInfo.renderingThreadingMode == RenderingThreadingMode.MultiThreaded ||
                SystemInfo.renderingThreadingMode == RenderingThreadingMode.LegacyJobified ||
                SystemInfo.renderingThreadingMode == RenderingThreadingMode.NativeGraphicsJobs)
            {
                SpacesRenderEventUtility.SubmitRenderEvent(SpacesRenderEvent.SetThreadHint);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrSetAndroidApplicationThreadKHRDelegate(ulong session, XrAndroidThreadTypeKHR threadType, uint threadId);
    }
}
