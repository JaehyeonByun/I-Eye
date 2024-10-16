/******************************************************************************
 * File: CompositionLayersFeature.LayerLifecycle.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal partial class CompositionLayersFeature
    {
        /// <summary>
        ///     Keeps track of all active layers which have been created.
        /// </summary>
        private static readonly Dictionary<uint, SpacesCompositionLayer> _activeLayers = new Dictionary<uint, SpacesCompositionLayer>();

        /// <summary>
        ///     Keeps a list of all layers which belong to a given sessionHandle.
        /// </summary>
        private static readonly Dictionary<ulong, List<uint>> _layersBySession = new Dictionary<ulong, List<uint>>();

        protected override void OnSessionCreate(ulong sessionHandle)
        {
            base.OnSessionCreate(sessionHandle);

            _layersBySession.Add(sessionHandle, new List<uint>());
        }

        protected override void OnSubsystemDestroy()
        {
            base.OnSubsystemDestroy();

            // This should be done as a result of destroying a session.
            // but due to concurrency between main/render thread cannot be executed in-time OnSessionDestroy
            // cannot be done in OnSessionEnd because that can occur during e.g. pause/resume from power button.
            // and OnSessionEnd is not called in the event of a Fusion disconnect anyway.
            IntPtr sessionIdPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ulong>());
            Marshal.StructureToPtr(SessionHandle, sessionIdPtr, false);
            SpacesRenderEventUtility.SubmitRenderEventAndData(SpacesRenderEvent.DestroySwapchainsForSession, sessionIdPtr);
        }

        private void AddActiveLayer(SpacesCompositionLayer layer)
        {
            _activeLayers.Add(layer.LayerId, layer);
            if (_layersBySession.ContainsKey(SessionHandle))
            {
                _layersBySession[SessionHandle].Add(layer.LayerId);
            }
        }

        /// <summary>
        ///     Removes a layer from the list of active layers. This layer is considered to no longer exist.
        /// </summary>
        /// <param name="layerId">The id of the layer to remove.</param>
        private void RemoveActiveLayer(uint layerId)
        {
            if (_activeLayers.ContainsKey(layerId))
            {
                _activeLayers.Remove(layerId);
            }
        }

        /// <summary>
        ///     Forcibly reconfigures an active layer. This layer will be reconstructed with a new layer id in the currently active session.
        /// </summary>
        /// <param name="layerId">The id of the layer to be reconfigured.</param>
        private void ReconfigureActiveLayer(uint layerId)
        {
            if (!_activeLayers.ContainsKey(layerId))
            {
                return;
            }

            _activeLayers[layerId].ForceReconfigure();
            _activeLayers.Remove(layerId);
        }

        /// <summary>
        ///     Remove all layers from the current session. These layers are forcibly reconfigured.
        ///     The list of layers belonging to this session is then cleared.
        /// </summary>
        /// <param name="sessionId">The id of the session which owns the layers to be reconfigured.</param>
        private void DecoupleLayersFromSession(ulong sessionId)
        {
            if (_layersBySession.ContainsKey(sessionId))
            {
                foreach (var layerId in _layersBySession[sessionId])
                {
                    ReconfigureActiveLayer(layerId);
                }

                _layersBySession.Remove(sessionId);
            }
        }
    }
}
