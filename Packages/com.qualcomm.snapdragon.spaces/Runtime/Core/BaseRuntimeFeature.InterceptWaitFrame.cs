/******************************************************************************
 * File: BaseRuntimeFeature.InterceptWaitFrame.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;
using AOT;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        internal delegate void ReceiveFrameStateCallback(XrFrameState frameState);

        private static long _predictedDisplayTime;
        public long PredictedDisplayTime => _predictedDisplayTime;

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetFrameStateCallback")]
        private static extern void SetFrameStateCallback(ReceiveFrameStateCallback callback);

        [MonoPInvokeCallback(typeof(ReceiveFrameStateCallback))]
        private static void OnFrameStateUpdate(XrFrameState frameState)
        {
            _predictedDisplayTime = frameState.PredictedDisplayTime;
        }
    }
}
