/******************************************************************************
 * File: CompositionLayersFeature.LayerConfiguration.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{

    internal partial class CompositionLayersFeature
    {
        /// <summary>
        ///     Keeps track of all configurations which have been created, and are pending Finalisation.
        /// </summary>
        private static readonly Dictionary<IntPtr, SpacesCompositionLayer> configurationLookup = new Dictionary<IntPtr, SpacesCompositionLayer>();

        internal void ConfigureCompositionLayer(IntPtr data)
        {
            if (data == IntPtr.Zero)
            {
                Debug.LogError("Attempting to Configure composition layer but invalid layer configuration data was provided.");
                return;
            }

            var layerConfigurationData = Marshal.PtrToStructure<CompositionLayerConfigurationData>(data);
            var layerId = CreateCompositionLayer(layerConfigurationData.LayerType, InstanceHandle, SessionHandle, layerConfigurationData.Width, layerConfigurationData.Height, layerConfigurationData.SortingOrder, layerConfigurationData.UseAndroidSurfaceSwapchain);

            if (layerId == 0)
            {
                Debug.LogError("CreateCompositionLayer returned a layerId of 0.");
                return;
            }

            // if not using android surface swapchain the layer should be set to not be visible until the image has been populated. This will occur on the first update for the composition layer.
            Internal_SetLayerVisible(layerId, layerConfigurationData.UseAndroidSurfaceSwapchain);

            FinaliseConfiguration(data, layerId);
        }

        internal IntPtr ConfigurationData(SpacesCompositionLayer CompositionLayer)
        {
            if (CompositionLayer == null)
            {
                Debug.LogError("Cannot create configuration data from invalid composition layer.");
                return IntPtr.Zero;
            }

            IntPtr configuration = Marshal.AllocHGlobal(Marshal.SizeOf<CompositionLayerConfigurationData>());
            CompositionLayerConfigurationData configurationData = new CompositionLayerConfigurationData(
                CompositionLayer.UseAndroidSurfaceSwapchain ? (uint)CompositionLayer.SurfaceTextureSize.x : (uint)CompositionLayer.LayerTexture.width,
                CompositionLayer.UseAndroidSurfaceSwapchain ? (uint)CompositionLayer.SurfaceTextureSize.y : (uint)CompositionLayer.LayerTexture.height,
                CompositionLayer.SortingOrder,
                CompositionLayer.LayerType,
                CompositionLayer.UseAndroidSurfaceSwapchain);

            Marshal.StructureToPtr(configurationData, configuration, false);
            configurationLookup.Add(configuration, CompositionLayer);

            return configuration;
        }

        private void FinaliseConfiguration(IntPtr Configuration, uint LayerId)
        {
            // Very limited in terms of available actions in this context. Can only change simple fields on the layer to be configured.
            // Assume that there is no access to GameObject internals.
            // Logging is risky - likely to crash.
            if (!configurationLookup.ContainsKey(Configuration))
            {
                return;
            }

            configurationLookup[Configuration].OnConfigured(LayerId);
            AddActiveLayer(configurationLookup[Configuration]);

            configurationLookup.Remove(Configuration);
        }

        /// <summary>
        ///     Allows SpacesCompositionLayer data to be passed via Marshalling to the render thread.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct CompositionLayerConfigurationData
        {
            public uint Width;
            public uint Height;
            public int SortingOrder;
            public SpacesCompositionLayerType LayerType;
            public bool UseAndroidSurfaceSwapchain;

            public CompositionLayerConfigurationData(uint Width, uint Height, int SortingOrder, SpacesCompositionLayerType LayerType, bool UseAndroidSurfaceSwapchain)
            {
                this.Width = Width;
                this.Height = Height;
                this.SortingOrder = SortingOrder;
                this.LayerType = LayerType;
                this.UseAndroidSurfaceSwapchain = UseAndroidSurfaceSwapchain;
            }
        }
    }
}
