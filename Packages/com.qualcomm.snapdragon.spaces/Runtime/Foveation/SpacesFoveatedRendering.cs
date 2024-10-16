/******************************************************************************
 * File: SpacesFoveatedRendering.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public static class SpacesFoveatedRendering
    {
        private static readonly FoveatedRenderingFeature _foveatedRenderingFeature = OpenXRSettings.Instance.GetFeature<FoveatedRenderingFeature>();

        /// <summary>
        ///     Set the foveation level to use for the application.
        ///     Higher levels of foveation reduce visual fidelity in peripheral vision, in exchange for increased performance.
        /// </summary>
        /// <param name="foveationLevel">The level to set.</param>
        public static void SetFoveationLevel(FoveationLevel foveationLevel)
        {
            if (!FeatureUseCheckUtility.IsFeatureUseable(_foveatedRenderingFeature))
            {
#if !UNITY_EDITOR
                Debug.LogWarning("Unable to set foveation level because the Foveated Rendering feature is not valid.");
#endif
                return;
            }

            _foveatedRenderingFeature.SetFoveationLevel(foveationLevel);
        }

        /// <summary>
        ///     Try and get the current foveation level.
        /// </summary>
        /// <param name="foveationLevel">The foveation level currently in use. None if this call fails.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool TryGetFoveationLevel(out FoveationLevel foveationLevel)
        {
            if (!FeatureUseCheckUtility.IsFeatureUseable(_foveatedRenderingFeature))
            {
#if !UNITY_EDITOR
                Debug.LogWarning("Unable to get foveation level because the Foveated Rendering feature is not valid.");
#endif
                foveationLevel = FoveationLevel.None;
                return false;
            }

            foveationLevel = _foveatedRenderingFeature.CurrentFoveationLevel;
            return true;
        }
    }
}
