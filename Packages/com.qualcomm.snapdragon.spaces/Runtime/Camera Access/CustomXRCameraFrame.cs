/******************************************************************************
 * File: CustomXRCameraFrame.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    // NOTE(TD): This struct is a copy of XRCameraFrame.cs and exists only to copy data to it
    // via marshaling. This struct should NOT be used as a replacement for XRCameraFrame.
    [StructLayout(LayoutKind.Sequential)]
    internal struct CustomXRCameraFrame
    {
        public long TimestampNs;
        private float _averageBrightness;
        private float _averageColorTemperature;
        private Color _colorCorrection;
        private Matrix4x4 _projectionMatrix;
        private Matrix4x4 _displayMatrix;
        private TrackingState _trackingState;
        private IntPtr _nativePtr;
        public XRCameraFrameProperties Properties;
        private float _averageIntensityInLumens;
        private double _exposureDuration;
        private float _exposureOffset;
        private float _mainLightIntensityLumens;
        private Color _mainLightColor;
        private Vector3 _mainLightDirection;
        private SphericalHarmonicsL2 _ambientSphericalHarmonics;
        private XRTextureDescriptor _cameraGrain;
        private float _noiseIntensity;
    }
}
