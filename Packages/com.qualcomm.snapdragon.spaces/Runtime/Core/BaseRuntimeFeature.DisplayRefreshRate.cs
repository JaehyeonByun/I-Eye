/******************************************************************************
 * File: BaseRuntimeFeature.DisplayRefreshRate.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        public float[] SupportedDisplayRefreshRates { get; private set; }

        private void EnumerateDisplayRefreshRates()
        {
            if (_xrEnumerateDisplayRefreshRatesFB == null)
            {
                Debug.LogError("xrEnumerateDisplayRefreshRatesFB method not found!");
                return;
            }

            uint displayRefreshRateCapacityOutput = 0;
            XrResult result = _xrEnumerateDisplayRefreshRatesFB(SessionHandle, 0, ref displayRefreshRateCapacityOutput, IntPtr.Zero);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("xrEnumerateDisplayRefreshRatesFB(1) failed: " + result);
                return;
            }

            if (displayRefreshRateCapacityOutput == 0)
            {
                Debug.Log("No supported display refresh rates found.");
                return;
            }

            using ScopeArrayPtr<float> displayRefreshRatesPtr = new((int)displayRefreshRateCapacityOutput);

            result = _xrEnumerateDisplayRefreshRatesFB(SessionHandle, displayRefreshRateCapacityOutput, ref displayRefreshRateCapacityOutput, displayRefreshRatesPtr.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("xrEnumerateDisplayRefreshRatesFB(2) failed: " + result);
                return;
            }

            Debug.Log("Refresh Rate Count: " + displayRefreshRateCapacityOutput);

            SupportedDisplayRefreshRates = new float[displayRefreshRatesPtr.ElementCount];
            for (int i = 0; i < displayRefreshRatesPtr.ElementCount; ++i)
            {
                SupportedDisplayRefreshRates[i] = displayRefreshRatesPtr.AtIndex(i);
                Debug.Log("Refresh Rate: " + SupportedDisplayRefreshRates[i]);
            }
        }

        public bool TrySetDisplayRefreshRate(float displayRefreshRate)
        {
            // Note(TD): If this check does not exist the xr call would return
            // error code 'XR_ERROR_DISPLAY_REFRESH_RATE_UNSUPPORTED_FB' instead
            if (!SupportedDisplayRefreshRates.Contains(displayRefreshRate))
            {
                Debug.LogWarning($"Requested display refresh rate ({displayRefreshRate}) is not supported.");
            }

            XrResult result = _xrRequestDisplayRefreshRateFB(SessionHandle, displayRefreshRate);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("xrRequestDisplayRefreshRateFB failed: " + result);
                Debug.LogWarning($"Requested display refresh rate ({displayRefreshRate}) has not been set.");
                return false;
            }

            Debug.Log("Refresh Rate successfully set to: " + displayRefreshRate);

            return true;
        }

        public bool TryGetDisplayRefreshRate(out float displayRefreshRate)
        {
            displayRefreshRate = 0.0f;

            XrResult result = _xrGetDisplayRefreshRateFB(SessionHandle, ref displayRefreshRate);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("xrGetDisplayRefreshRateFB failed: " + result);
                return false;
            }

            return true;
        }

        #region XR_FB_display_refresh_rate bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrEnumerateDisplayRefreshRatesFBDelegate(ulong session, uint displayRefreshRateCapacityInput, ref uint displayRefreshRateCountOutput, IntPtr /* float* */ displayRefreshRates);
        private static xrEnumerateDisplayRefreshRatesFBDelegate _xrEnumerateDisplayRefreshRatesFB;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrGetDisplayRefreshRateFBDelegate(ulong session, ref float displayRefreshRate);
        private static xrGetDisplayRefreshRateFBDelegate _xrGetDisplayRefreshRateFB;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrRequestDisplayRefreshRateFBDelegate(ulong session, float displayRefreshRate);
        private static xrRequestDisplayRefreshRateFBDelegate _xrRequestDisplayRefreshRateFB;

        #endregion
    }
}
