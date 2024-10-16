/******************************************************************************
 * File: CameraSubsystem.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    public class CameraSubsystem : XRCameraSubsystem
    {
        public const string ID = "Spaces-CameraSubsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRCameraSubsystemCinfo _cinfo = new XRCameraSubsystemCinfo
            {
                id = ID,
                providerType = typeof(CameraProvider),
                subsystemTypeOverride = typeof(CameraSubsystem),
                supportsAverageBrightness = false,
                supportsAverageColorTemperature = false,
                supportsColorCorrection = false,
                supportsDisplayMatrix = false,
                supportsProjectionMatrix = false,
                supportsTimestamp = false,
                supportsCameraConfigurations = false,
                supportsCameraImage = true,
                supportsAverageIntensityInLumens = false,
                supportsFaceTrackingAmbientIntensityLightEstimation = false,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = false,
                supportsWorldTrackingHDRLightEstimation = false,
                supportsFocusModes = false,
                supportsCameraGrain = false
            };

            Register(_cinfo);
        }

        internal class CameraProvider : Provider
        {
            private CameraAccessInputUpdate _cameraInputUpdate { get; } = new CameraAccessInputUpdate();
            private bool _autoFocusEnabled = false;
            private bool _autoFocusRequested;
            private ulong _cameraHandle;
            private XrCameraInfoQCOM _cameraInfo;
            private Material _cameraMaterial;

            private XRCpuImage.Api _cpuImageApi;
            private Feature _currentCamera;
            private XRCameraConfiguration? _currentConfiguration;
            private Feature _currentLightEstimation;
            private List<XrCameraFrameConfigurationQCOM> _deviceFrameConfigurations;
            private readonly List<XrCameraFrameFormatQCOM> _supportedFrameFormats = new List<XrCameraFrameFormatQCOM>(){XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_YUV420_NV12_QCOMX,XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_YUV420_NV21_QCOMX,XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_YUYV_QCOMX};
            private XrCameraFrameConfigurationQCOM _frameConfiguration;
            private bool _invertCulling;

            private XRCameraFrame _lastFrame;
            private ulong _lastFrameHandle;
            private bool _permissionGranted;
            private Feature _requestedCamera;
            private Feature _requestedLightEstimation;
            private CameraAccessFeature _underlyingFeature;

            internal XrCameraInfoQCOM CameraInfo => _cameraInfo;

            public override XRCpuImage.Api cpuImageApi => _cpuImageApi;

            public override Material cameraMaterial => _cameraMaterial;

            public override bool permissionGranted => _permissionGranted;

            public override bool invertCulling => _invertCulling;

            public override Feature currentCamera => _currentCamera;

            public override Feature requestedCamera
            {
                get => _requestedCamera;
                set => _requestedCamera = value;
            }

            public override bool autoFocusEnabled => _autoFocusEnabled;

            public override bool autoFocusRequested
            {
                get => _autoFocusRequested;
                set => _autoFocusRequested = value;
            }

            public override Feature currentLightEstimation => _currentLightEstimation;

            public override Feature requestedLightEstimation
            {
                get => _requestedLightEstimation;
                set => _requestedLightEstimation = value;
            }

            public override XRCameraConfiguration? currentConfiguration
            {
                get => _currentConfiguration;
                set => _currentConfiguration = value;
            }

            public override void Start()
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    var nativeXRSupportChecker = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.serviceshelper.NativeXRSupportChecker");

                    if (nativeXRSupportChecker.CallStatic<bool>("CanShowPermissions"))
                    {
                        Permission.RequestUserPermission(Permission.Camera);
                    }
                }
#endif
                _underlyingFeature = OpenXRSettings.Instance.GetFeature<CameraAccessFeature>();

                if (!FeatureUseCheckUtility.IsFeatureUseable(_underlyingFeature))
                {
#if !UNITY_EDITOR
                    Debug.LogError("Spaces CameraAccessFeature is missing or not enabled.");
#endif
                    Destroy();
                    return;
                }

                var arCameraBackgrounds = GameObject.FindObjectsOfType<ARCameraBackground>().Where(a=>a.enabled);
                foreach (var arCameraBackground in arCameraBackgrounds )
                {
                    arCameraBackground.enabled = false;
                    Debug.LogWarning("Disabling ARCameraBackground component. Snapdragon Spaces Camera Frame Access does not support ARCameraBackground.");
                }

                _cpuImageApi = SpacesCpuImageApi.CreateInstance();
                _permissionGranted = true;
                _invertCulling = false;
                _currentCamera = Feature.WorldFacingCamera;
                _requestedCamera = Feature.WorldFacingCamera;
                _currentLightEstimation = Feature.None;
                _requestedLightEstimation = Feature.None;
                _currentConfiguration = null;
                _cameraInputUpdate.AddDevice();
            }

            public override void Stop()
            {
                if (_cameraHandle == 0)
                {
                    return;
                }

                if (!_underlyingFeature.TryReleaseFrame())
                {
                    Debug.LogError("Could not release frame with handle [" + _underlyingFeature.CachedFrameData.Handle + "].");
                }

                if (!_underlyingFeature.TryReleaseCameraHandle(_cameraHandle))
                {
                    Debug.LogError("Failed to release camera handle for camera [" + _cameraHandle + "].");
                }
                else
                {
                    _cameraHandle = 0;
                }

                _cameraInputUpdate.RemoveDevice();
            }

            public override void Destroy()
            {
                Stop();
            }

            public override NativeArray<XRCameraConfiguration> GetConfigurations(XRCameraConfiguration defaultCameraConfiguration, Allocator allocator)
            {
                // InitialiseRGBCamera() should be performed asynchronously on Start() in the future
                // In this case, a null configuration should still return a NativeArray<..> of length == 0
                if (_currentConfiguration == null && !InitialiseRGBCamera())
                {
                    Debug.LogError("Failed to initialise target camera.");
                    return new NativeArray<XRCameraConfiguration>(0, allocator);
                }

                NativeArray<XRCameraConfiguration> cameraConfigs = new NativeArray<XRCameraConfiguration>(1, allocator);
                cameraConfigs[0] = (XRCameraConfiguration)_currentConfiguration;
                return cameraConfigs;
            }

            public override bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics)
            {
                cameraIntrinsics = new XRCameraIntrinsics();

                // InitialiseRGBCamera() should be performed asynchronously on Start() in the future
                if (_cameraHandle == 0 && !InitialiseRGBCamera())
                {
                    Debug.LogError("Failed to initialise target camera.");
                    return false;
                }

                var shouldRequestSensorInfo = _underlyingFeature.SensorProperties?.Length == 0;
                if (shouldRequestSensorInfo && !_underlyingFeature.TryAccessFrame(_cameraHandle, _frameConfiguration, _cameraInfo.SensorCount))
                {
                    Debug.LogError("Failed to acquire intrinsics of current camera.");
                    return false;
                }

                // CH: In case of multiple sensors (i.e Left/Right eyes), only the first sensor's data is exposed
                XrCameraSensorPropertiesQCOM[] sensorProperties = _underlyingFeature.SensorProperties;
                cameraIntrinsics = new XRCameraIntrinsics(
                    sensorProperties[0].Intrinsics.FocalLength.ToVector2(),
                    sensorProperties[0].Intrinsics.PrincipalPoint.ToVector2(),
                    sensorProperties[0].ImageDimensions.ToVector2Int());
                return true;
            }

            public override bool TryAcquireLatestCpuImage(out XRCpuImage.Cinfo cameraImageCinfo)
            {
                // CH: In case of multiple sensors (i.e Left/Right eyes), only the first sensor's data is exposed
                cameraImageCinfo = new XRCpuImage.Cinfo();
                if (_underlyingFeature.CachedYuvFrames.Length == 0)
                {
                    Debug.LogError("Tried to acquire latest CPU image, but no CPU image is available yet.");
                    return false;
                }
                cameraImageCinfo = new XRCpuImage.Cinfo(
                    (int)_underlyingFeature.CachedYuvFrames[0].Handle,
                    _underlyingFeature.CachedYuvFrames[0].Dimensions,
                    _underlyingFeature.CachedYuvFrames[0].NativePlaneCount,
                    ConvertXrTimeToDouble(_underlyingFeature.CachedYuvFrames[0].Timestamp),
                    XRCpuImage.Format.AndroidYuv420_888);

                return true;
            }

            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                cameraFrame = default;

                // InitialiseRGBCamera() should be performed asynchronously on Start() in the future
                if (_cameraHandle == 0 && !InitialiseRGBCamera())
                {
                    Debug.LogError("Failed to initialise target camera.");
                    return false;
                }

                if (!_underlyingFeature.TryAccessFrame(_cameraHandle, _frameConfiguration, _cameraInfo.SensorCount))
                {
                    // Do not print anything, TryAccessFrame already prints the cause.
                    return false;
                }

                if (_lastFrameHandle == _underlyingFeature.CachedFrameData.Handle)
                {
                    // No new frame received, do not fire ARCameraManager.frameReceived event
                    return false;
                }

                _lastFrameHandle = _underlyingFeature.CachedFrameData.Handle;

                // Note(TD): This is the equivalent of filling the cameraFrame struct through native code.
                // We do this since the XRCameraFrame struct has no constructors or setters.
                CustomXRCameraFrame customCameraFrame = default;
                customCameraFrame.Properties = XRCameraFrameProperties.Timestamp;
                customCameraFrame.TimestampNs = _underlyingFeature.CachedFrameData.Timestamp;

                GCHandle handle = GCHandle.Alloc(customCameraFrame, GCHandleType.Pinned);
                cameraFrame = (XRCameraFrame)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(XRCameraFrame));

                _cameraInputUpdate.UpdateCameraDevice(_underlyingFeature.LastFramePose);

                return true;
            }

            // InitialiseRGBCamera() should be performed asynchronously on Start() in the future
            private bool InitialiseRGBCamera()
            {
                if (!_underlyingFeature.TryEnumerateCameras())
                {
                    Debug.LogError("Failed to enumerate cameras.");
                    Destroy();
                    return false;
                }

                // Find first RGB camera enumerated
                string cameraSet = null;
                foreach (var cameraInfo in _underlyingFeature.CameraInfos)
                {
                    if (cameraInfo.CameraType == XrCameraTypeQCOM.XR_CAMERA_TYPE_RGB_QCOMX)
                    {
                        cameraSet = cameraInfo.CameraSet;
                        _cameraInfo = cameraInfo;
                        break;
                    }
                }

                if (cameraSet == null)
                {
                    Debug.LogError("No RGB camera found.");
                    Destroy();
                    return false;
                }

                // Retrieve target frame configuration for RGB camera set
                _deviceFrameConfigurations = _underlyingFeature.TryGetSupportedFrameConfigurations(cameraSet);
                if (_deviceFrameConfigurations.Count == 0)
                {
                    Debug.LogError("Failed to find supported frame configurations for camera set [" + cameraSet + "].");
                    Destroy();
                    return false;
                }

                // Consider only YUV420_NV12, YUV420_NV21 and YUYV formats, if some extra format add it to supported Frame format list
                // Retrieve first "full" frame configuration. If none, select first non-"full" frame configuration.
                foreach (var frameConfig in _deviceFrameConfigurations)
                {
                    var formatIsSupported = _supportedFrameFormats.Contains(frameConfig.Format);
                    if (!formatIsSupported)
                    {
                        continue;
                    }

                    if (frameConfig.ResolutionName.Equals("full"))
                    {
                        _frameConfiguration = frameConfig;
                        break;
                    }

                    if (_frameConfiguration.Format == XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_UNKNOWN_QCOMX)
                    {
                        _frameConfiguration = frameConfig;
                    }
                }

                if (_frameConfiguration.Format == XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_UNKNOWN_QCOMX)
                {
                    Debug.LogError("No supported frame configurations found.");
                    Destroy();
                    return false;
                }

                // Create camera handle for access
                if (!_underlyingFeature.TryCreateCameraHandle(out _cameraHandle, cameraSet))
                {
                    Debug.LogError("Failed to create camera handle for camera set [" + cameraSet + "].");
                    Destroy();
                    return false;
                }

                _currentConfiguration = new XRCameraConfiguration(
                    IntPtr.Zero,
                    _frameConfiguration.Dimensions.ToVector2Int(),
                    (int)_frameConfiguration.MaxFPS
                );

                return true;
            }

            public override NativeArray<XRTextureDescriptor> GetTextureDescriptors(XRTextureDescriptor defaultDescriptor, Allocator allocator)
            {
                // Note(PA): This is the equivalent of filling the xrTextureDescriptor struct through native code.
                // We do this since the XRTextureDescriptor struct has no public setters.
                CustomXRTextureDescriptor customTextureDescriptor = default;
                customTextureDescriptor.nativeTexture = _underlyingFeature.FrameBuffers[0].Buffer;
                customTextureDescriptor.width = (int)_frameConfiguration.Dimensions.Width;
                customTextureDescriptor.height = (int)_frameConfiguration.Dimensions.Height;
                customTextureDescriptor.mipmapCount = 1;
                customTextureDescriptor.format = TextureFormat.RGBA32;
                customTextureDescriptor.propertyNameId = 0;
                customTextureDescriptor.depth = 0;
                customTextureDescriptor.dimension = TextureDimension.Tex2D;

                GCHandle handle = GCHandle.Alloc(customTextureDescriptor, GCHandleType.Pinned);
                var xrTextureDescriptor = (XRTextureDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(XRTextureDescriptor));

                var nativeArray = new NativeArray<XRTextureDescriptor>(1, allocator);

                nativeArray[0] = xrTextureDescriptor;

                handle.Free();

                return nativeArray;
            }

            private double ConvertXrTimeToDouble(long xrTime)
            {
                const long nSecInSec = 1000000000;

                Timespec timeInTimespec;

                bool result = _underlyingFeature.SpacesTimespecFromXRTime(xrTime, out timeInTimespec);

                if (!result)
                {
                    Debug.LogError("SpacesTimespecFromXRTime failed.");
                    return 0.0;
                }

                return ((double)timeInTimespec.Seconds + ((double)timeInTimespec.Nanoseconds/nSecInSec));
            }
        }
    }
}
