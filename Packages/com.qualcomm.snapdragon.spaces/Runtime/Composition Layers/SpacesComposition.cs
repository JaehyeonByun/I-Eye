using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public static class SpacesComposition
    {
        private static readonly CompositionLayersFeature _compositionLayersFeature = OpenXRSettings.Instance.GetFeature<CompositionLayersFeature>();

        public delegate void EventHandler(uint layerId);

        public static void ListenForLayerCreation(EventHandler callback)
        {
            if (FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature))
            {
                _compositionLayersFeature.OnCompositionLayerCreated += callback.Invoke;
            }
        }

        public static void StopListeningForLayerCreation(EventHandler callback)
        {
            if (FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature))
            {
                _compositionLayersFeature.OnCompositionLayerCreated -= callback.Invoke;
            }
        }

        public static void ListenForLayerDestruction(EventHandler callback)
        {
            if (FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature))
            {
                _compositionLayersFeature.OnCompositionLayerDestroyed += callback.Invoke;
            }
        }

        public static void StopListeningForLayerDestruction(EventHandler callback)
        {
            if (FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature))
            {
                _compositionLayersFeature.OnCompositionLayerDestroyed -= callback.Invoke;
            }
        }

        public static uint MaxSwapchainImageHeight => FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature) ? _compositionLayersFeature.SystemProperties.GetGraphicsProperties().MaxSwapchainImageHeight : 0;
        public static uint MaxSwapchainImageWidth => FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature) ? _compositionLayersFeature.SystemProperties.GetGraphicsProperties().MaxSwapchainImageWidth : 0;
        public static uint MaxLayerCount => FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature) ? _compositionLayersFeature.SystemProperties.GetGraphicsProperties().MaxLayerCount : 0;
    }
}
