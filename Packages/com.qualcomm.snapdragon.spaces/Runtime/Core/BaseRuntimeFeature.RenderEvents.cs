/******************************************************************************
 * File: BaseRuntimeFeature.RenderEvents.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    internal enum SpacesRenderEvent
    {
        SetThreadHint = 0,
        SetFoveationLevel,
        ConfigureSwapchain,
        DestroySwapchain,
        DestroySwapchainsForSession,
        Count // This must be the last event defined in the enum
    }

    public partial class BaseRuntimeFeature
    {
        // start index reserved for SpacesRenderEvents
        private static int _baseRenderEventIndex;

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "ReserveEventIndices")]
        private static extern int ReserveEventIndices(int countIdsToReserve);

        private void ConfigureSpacesRenderEvents()
        {
            _baseRenderEventIndex = ReserveEventIndices((int)SpacesRenderEvent.Count);
        }

        // convert from SpacesRenderEvent to an event id by offsetting by the baseRenderEventIndex
        internal static int SpacesRenderEventToEventId(SpacesRenderEvent renderEvent)
        {
            return _baseRenderEventIndex + (int)renderEvent;
        }

        // convert from an event id to a SpacesRenderEvent by offsetting by the baseRenderEventIndex
        internal static SpacesRenderEvent EventIdToSpacesRenderEvent(int eventId)
        {
            return (SpacesRenderEvent)(eventId - _baseRenderEventIndex);
        }

        /// <summary>
        ///     Executes an event on the render thread.
        /// </summary>
        /// <param name="eventID"> Id for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        internal static void RunOnRenderThread(int eventID)
        {
            /*
             * This method is intended to be executed on the render thread.
             * It must only be called using CommandBuffer.IssuePluginEvent(...)
             * see RenderEventDelegate
             * see SpacesRenderEventUtility.SubmitRenderEvent
             */
            switch (EventIdToSpacesRenderEvent(eventID))
            {
                case SpacesRenderEvent.SetThreadHint:
                    SpacesThreadUtility.SetThreadHint(SpacesThreadType.SPACES_THREAD_TYPE_RENDERER_WORKER);
                    break;

                case SpacesRenderEvent.SetFoveationLevel:
                    Debug.LogWarning("Tried to set foveation level but missing data containing the level to set. Did you mean to issue the plugin event RunOnRenderThreadWithData instead?");
                    break;

                case SpacesRenderEvent.ConfigureSwapchain:
                    Debug.LogWarning("Tried to configure swapchain but missing data about how to do that. Did you mean to issue the plugin event RunOnRenderThreadWithData instead?");
                    break;

                case SpacesRenderEvent.DestroySwapchain:
                    Debug.LogWarning("Tried to destroy swapchain but missing data about which one to destroy. Did you mean to issue the plugin event RunOnRenderThreadWithData instead?");
                    break;

                case SpacesRenderEvent.DestroySwapchainsForSession:
                    Debug.LogWarning("Tried to destroy swapchains belonging to a session but missing data about which ones to destroy. Did you mean to issue the plugin event RunOnRenderThreadWithData instead?");
                    break;

                // unhandled events
                case SpacesRenderEvent.Count:
                default:
                    Debug.LogWarning($"Unknown or unhandled render event: {eventID}");
                    break;
            }
        }

        /// <summary>
        ///     Executes an event on the render thread (but now with data).
        /// </summary>
        /// <param name="eventID">Id for the event.</param>
        /// <param name="data">Custom data supplied to the event</param>
        [MonoPInvokeCallback(typeof(RenderEventWithDataDelegate))]
        internal static void RunOnRenderThreadWithData(int eventID, IntPtr data)
        {
            /*
             * This method is intended to be executed on the render thread.
             * It must only be called using CommandBuffer.IssuePluginEventAndData(...)
             * see RenderEventWithDataDelegate
             * see SpacesRenderEventUtility.SubmitRenderEventWithData
             */
            switch (EventIdToSpacesRenderEvent(eventID))
            {
                case SpacesRenderEvent.SetThreadHint:
                    {
                        SpacesThreadUtility.SetThreadHint(SpacesThreadType.SPACES_THREAD_TYPE_RENDERER_WORKER);
                    }
                    break;
                case SpacesRenderEvent.SetFoveationLevel:
                    {
                        var foveatedRenderingFeature = OpenXRSettings.Instance.GetFeature<FoveatedRenderingFeature>();
                        if (FeatureUseCheckUtility.IsFeatureUseable(foveatedRenderingFeature))
                        {
                            foveatedRenderingFeature.SetFoveationLevel_GraphicsContext((FoveationLevel)Marshal.PtrToStructure<int>(data));
                        }
                    }
                    break;
                case SpacesRenderEvent.ConfigureSwapchain:
                    {
                        var compositionLayersFeature = OpenXRSettings.Instance.GetFeature<CompositionLayersFeature>();
                        if (FeatureUseCheckUtility.IsFeatureUseable(compositionLayersFeature))
                        {
                            compositionLayersFeature.ConfigureCompositionLayer(data);
                        }
                    }
                    break;
                case SpacesRenderEvent.DestroySwapchain:
                    {
                        var compositionLayersFeature = OpenXRSettings.Instance.GetFeature<CompositionLayersFeature>();
                        if (FeatureUseCheckUtility.IsFeatureUseable(compositionLayersFeature))
                        {
                            var layerId = Marshal.PtrToStructure<uint>(data);
                            compositionLayersFeature.DestroyCompositionLayer(layerId);
                        }
                    }
                    break;
                case SpacesRenderEvent.DestroySwapchainsForSession:
                    {
                        var compositionLayersFeature = OpenXRSettings.Instance.GetFeature<CompositionLayersFeature>();
                        if (FeatureUseCheckUtility.IsFeatureUseable(compositionLayersFeature))
                        {
                            var sessionId = Marshal.PtrToStructure<ulong>(data);
                            compositionLayersFeature.DestroyCompositionLayersInSession(sessionId);
                        }
                    }
                    break;
                // unhandled events
                case SpacesRenderEvent.Count:
                default:
                    {
                        Debug.LogWarning($"Unknown or unhandled render event: {eventID}");
                    }
                    break;
            }

            // this data was marshalled off of the render thread and will be free'd now.
            if (data != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(data);
            }
        }

        /// <summary>
        ///     c# implementation of the native plugin rendering callback from IUnityGraphics.h:
        ///     typedef void (UNITY_INTERFACE_API * UnityRenderingEvent)(int eventId);
        ///     A delegate of this type is to be used as the first parameter to CommandBuffer.IssuePluginEvent(...) calls.
        /// </summary>
        internal delegate void RenderEventDelegate(int eventID);

        /// <summary>
        ///     c# implementation of the native plugin rendering callback from IUnityGraphics.h:
        ///     typedef void (UNITY_INTERFACE_API * UnityRenderingEventAndData)(int eventId, void* data);
        ///     A delegate of this type is to be used as the first parameter to CommandBuffer.IssuePluginEventAndData(...) calls.
        /// </summary>
        internal delegate void RenderEventWithDataDelegate(int eventID, IntPtr data);
    }
}
