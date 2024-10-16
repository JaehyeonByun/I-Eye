/******************************************************************************
 * File: SpacesRenderEventUtility.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Qualcomm.Snapdragon.Spaces
{
    internal static class SpacesRenderEventUtility
    {
        internal static void SubmitRenderEvent(SpacesRenderEvent renderEvent)
        {
            BaseRuntimeFeature.RenderEventDelegate renderThreadRunner = BaseRuntimeFeature.RunOnRenderThread;
            IntPtr renderThreadRunnerPtr = Marshal.GetFunctionPointerForDelegate(renderThreadRunner);

            CommandBuffer cb = new CommandBuffer();
            cb.IssuePluginEvent(renderThreadRunnerPtr, BaseRuntimeFeature.SpacesRenderEventToEventId(renderEvent));
            Graphics.ExecuteCommandBuffer(cb);
        }

        internal static void SubmitRenderEventAndData(SpacesRenderEvent renderEvent, IntPtr data)
        {
            BaseRuntimeFeature.RenderEventWithDataDelegate renderThreadRunner = BaseRuntimeFeature.RunOnRenderThreadWithData;
            IntPtr renderThreadRunnerPtr = Marshal.GetFunctionPointerForDelegate(renderThreadRunner);

            CommandBuffer cb = new CommandBuffer();
            cb.IssuePluginEventAndData(renderThreadRunnerPtr, BaseRuntimeFeature.SpacesRenderEventToEventId(renderEvent), data);
            Graphics.ExecuteCommandBuffer(cb);
        }
    }
}
