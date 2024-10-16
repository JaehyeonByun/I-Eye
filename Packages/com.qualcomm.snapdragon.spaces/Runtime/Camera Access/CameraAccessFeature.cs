/******************************************************************************
 * File: CameraAccessFeature.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = FeatureName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Qualcomm",
        Desc = "Enables Camera Access feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.23.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class CameraAccessFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Camera Access (Experimental)";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.cameraaccess";
        public const string FeatureExtensions = "XR_QCOMX_camera_frame_access XR_KHR_convert_timespec_time";

        [Tooltip("XRCpuImage.Convert read/write can be optimized on certain devices through direct memory access. By default, Spaces moves frame data using Marshal.Copy. Enabling this setting allows Spaces to use NativeArray<byte> direct representations of the source and target buffers. This setting may heavily impact performance on some architectures, use at your own risk.")]
        public bool DirectMemoryAccessConversion = false;

        private static List<XRCameraSubsystemDescriptor> _cameraSubsystemDescriptors = new List<XRCameraSubsystemDescriptor>();
        private BaseRuntimeFeature _baseRuntimeFeature;

        private List<XrCameraInfoQCOM> _cameraInfos = new List<XrCameraInfoQCOM>();
        private XrCameraFrameBufferQCOM _defaultFrameBuffer;
        private XrCameraFrameHardwareBufferQCOM _defaultFrameHardwareBuffer;
        private XrCameraSensorPropertiesQCOM _defaultSensorProperties;

        private XrCameraFrameBufferQCOM[] _frameBuffers;
        private XrCameraFrameHardwareBufferQCOM[] _frameHardwareBuffers;

        private bool _frameReleased = true;

        private XrCameraFrameConfigurationQCOM _lastFrameConfig;
        private XrCameraFrameDataQCOM _cachedFrameData;
        private SpacesYUVFrame[] _cachedYuvFrames = Array.Empty<SpacesYUVFrame>();
        private bool _deviceIsA3;

        private XrCameraSensorPropertiesQCOM[] _sensorProperties;
        private XrPosef _lastFramePose = new XrPosef(XrQuaternionf.identity, XrVector3f.zero);

        internal List<XrCameraInfoQCOM> CameraInfos => _cameraInfos;

        internal XrCameraSensorPropertiesQCOM[] SensorProperties => _sensorProperties;
        internal XrCameraFrameDataQCOM CachedFrameData => _cachedFrameData;
        internal SpacesYUVFrame[] CachedYuvFrames => _cachedYuvFrames;
        internal XrCameraFrameBufferQCOM[] FrameBuffers => _frameBuffers;
        internal Pose LastFramePose => _lastFramePose.ToPose();

        protected override bool IsRequiringBaseRuntimeFeature => true;

        protected override string GetXrLayersToLoad()
        {
            return "XR_APILAYER_QCOM_retina_tracking";
        }

        protected override bool OnInstanceCreate(ulong instanceHandle)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            base.OnInstanceCreate(instanceHandle);

            _baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();

            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any())
            {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                var nativeXRSupportChecker = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.serviceshelper.NativeXRSupportChecker");

                if (nativeXRSupportChecker.CallStatic<bool>("CanShowPermissions"))
                {
                    Debug.LogWarning(FeatureName + " Feature is missing the camera permissions but it will be started regardless as the device supports runtime permission query!");
                    return true;
                }
                Debug.LogError(FeatureName + " Feature is missing the camera permissions and can't be created therefore!");
                return false;
            }
#endif
            _deviceIsA3 = SystemInfo.deviceModel.ToLower().Contains("motorola edge");

            return true;
        }

        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(_cameraSubsystemDescriptors, CameraSubsystem.ID);
        }

        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRCameraSubsystem>();
        }

        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRCameraSubsystem>();
        }

        protected override void OnHookMethods()
        {
            HookMethod("xrEnumerateCamerasQCOMX", out _xrEnumerateCamerasQCOM);
            HookMethod("xrGetSupportedFrameConfigurationsQCOMX", out _xrGetSupportedFrameConfigurationsQCOM);
            HookMethod("xrCreateCameraHandleQCOMX", out _xrCreateCameraHandleQCOM);
            HookMethod("xrReleaseCameraHandleQCOMX", out _xrReleaseCameraHandleQCOM);
            HookMethod("xrAccessFrameQCOMX", out _xrAccessFrameQCOM);
            HookMethod("xrReleaseFrameQCOMX", out _xrReleaseFrameQCOM);
            HookMethod("xrConvertTimespecTimeToTimeKHR", out _xrConvertTimespecTimeToTimeKHR);
            HookMethod("xrConvertTimeToTimespecTimeKHR", out _xrConvertTimeToTimespecTimeKHR);
        }

        public bool TryEnumerateCameras()
        {
            _cameraInfos = new List<XrCameraInfoQCOM>();

            if (_xrEnumerateCamerasQCOM == null)
            {
                Debug.LogError("xrEnumerateCamerasQCOM method not found!");
                return false;
            }

            uint cameraInfoCountOutput = 0;

            var result = _xrEnumerateCamerasQCOM(SessionHandle, 0, ref cameraInfoCountOutput, IntPtr.Zero);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Enumerate device cameras (1) failed: " + Enum.GetName(typeof(XrResult), result));
                return false;
            }

            using ScopeArrayPtr<XrCameraInfoQCOM> cameraInfosPtr = new((int)cameraInfoCountOutput);
            var defaultCameraInfo = new XrCameraInfoQCOM(String.Empty, 0, 0);
            for (int i = 0; i < cameraInfoCountOutput; i++)
            {
                cameraInfosPtr.Copy(defaultCameraInfo, i);
            }

            result = _xrEnumerateCamerasQCOM(SessionHandle, cameraInfoCountOutput, ref cameraInfoCountOutput, cameraInfosPtr.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Enumerate device cameras (2) failed: " + Enum.GetName(typeof(XrResult), result));
                return false;
            }

            for (int i = 0; i < cameraInfoCountOutput; i++)
            {
                _cameraInfos.Add(cameraInfosPtr.AtIndex(i));
            }

            // Initialise default frame access structures for convenience
            //
            // XR_MAX_CAMERA_RADIAL_DISTORSION_PARAMS_LENGTH_QCOMX == 6
            // XR_MAX_CAMERA_TANGENTIAL_DISTORSION_PARAMS_LENGTH_QCOMX == 2
            //
            // Marshal.SizeOf(XrCameraFramePlaneQCOMX) == 32
            // XR_CAMERA_FRAME_PLANES_SIZE_QCOMX == 4

            var defaultSensorIntrinsics = new XrCameraSensorIntrinsicsQCOM(
                new XrVector2f(Vector2.zero),
                new XrVector2f(Vector2.zero),
                new float[6],
                new float[2],
                0);
            _defaultSensorProperties = new XrCameraSensorPropertiesQCOM(
                defaultSensorIntrinsics,
                new XrPosef(new XrQuaternionf(Quaternion.identity), new XrVector3f(Vector3.zero)),
                new XrOffset2Di(Vector2Int.zero),
                new XrExtent2Di(Vector2Int.zero),
                0,
                0);
            _defaultFrameBuffer = new XrCameraFrameBufferQCOM(
                0,
                IntPtr.Zero,
                new XrOffset2Di(Vector2Int.zero),
                0,
                new byte[32 * 4]);
            _defaultFrameHardwareBuffer = new XrCameraFrameHardwareBufferQCOM(IntPtr.Zero);

            return true;
        }

        public List<XrCameraFrameConfigurationQCOM> TryGetSupportedFrameConfigurations(string cameraSet)
        {
            var defaultReturn = new List<XrCameraFrameConfigurationQCOM>();

            if (_xrGetSupportedFrameConfigurationsQCOM == null)
            {
                Debug.LogError("xrGetSupportedFrameConfigurationsQCOM method not found!");
                return defaultReturn;
            }

            uint frameConfigurationCountOutput = 0;

            var result = _xrGetSupportedFrameConfigurationsQCOM(SessionHandle, cameraSet, 0, ref frameConfigurationCountOutput, IntPtr.Zero);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get Supported Frame Configurations (1): " + Enum.GetName(typeof(XrResult), result));
                return defaultReturn;
            }

            using ScopeArrayPtr<XrCameraFrameConfigurationQCOM> frameConfigurationsPtr = new((int)frameConfigurationCountOutput);
            var defaultFrameConfig = new XrCameraFrameConfigurationQCOM(0, String.Empty, new XrExtent2Di(0, 0), 0, 0, 0, 0);
            for (int i = 0; i < frameConfigurationCountOutput; i++)
            {
                frameConfigurationsPtr.Copy(defaultFrameConfig, i);
            }

            result = _xrGetSupportedFrameConfigurationsQCOM(SessionHandle, cameraSet, frameConfigurationCountOutput, ref frameConfigurationCountOutput, frameConfigurationsPtr.Raw);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to get Supported Frame Configurations (2): " + Enum.GetName(typeof(XrResult), result));
                return defaultReturn;
            }

            var frameConfigurations = new List<XrCameraFrameConfigurationQCOM>();
            for (int i = 0; i < frameConfigurationCountOutput; i++)
            {
                frameConfigurations.Add(frameConfigurationsPtr.AtIndex(i));
            }

            return frameConfigurations;
        }

        public bool TryCreateCameraHandle(out ulong cameraHandle, string cameraSet)
        {
            cameraHandle = 0;

            if (_xrCreateCameraHandleQCOM == null)
            {
                Debug.LogError("xrCreateCameraHandleQCOM method not found!");
                return false;
            }

            XrCameraActivationInfoQCOM activationInfo = new XrCameraActivationInfoQCOM(cameraSet);

            var result = _xrCreateCameraHandleQCOM(SessionHandle, ref activationInfo, ref cameraHandle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to create camera handle: " + Enum.GetName(typeof(XrResult), result));
                return false;
            }

            return true;
        }

        public bool TryReleaseCameraHandle(ulong cameraHandle)
        {
            if (_xrReleaseCameraHandleQCOM == null)
            {
                Debug.LogError("xrReleaseCameraHandleQCOM method not found!");
                return false;
            }

            var result = _xrReleaseCameraHandleQCOM(cameraHandle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to release camera handle: " + Enum.GetName(typeof(XrResult), result));
                return false;
            }

            return true;
        }

        public bool TryAccessFrame(ulong cameraHandle, XrCameraFrameConfigurationQCOM frameConfig, uint sensorCount)
        {
            if (_xrAccessFrameQCOM == null)
            {
                Debug.LogError("xrAccessFrameQCOM method not found!");
                return false;
            }

            // Release any locked frames from the runtime
            if (!_frameReleased && !TryReleaseFrame())
            {
                Debug.LogError("Failed to clear frame buffer before requesting frame.");
                return false;
            }

            // Create XrCameraSensorInfosQCOM structure
            //IntPtr sensorPropertiesPtr = IntPtr.Zero;
            IntPtr sensorInfosPtr = IntPtr.Zero;
            GCHandle pinnedSensorInfos = new GCHandle();

            using ScopeArrayPtr<XrCameraSensorPropertiesQCOM> sensorPropertiesPtr = new((int)sensorCount);
            for (int i = 0; i < sensorCount; i++)
            {
                sensorPropertiesPtr.Copy(_defaultSensorProperties, i);
            }

            XrCameraSensorInfosQCOM sensorInfos = new XrCameraSensorInfosQCOM(_baseRuntimeFeature.SpaceHandle, sensorCount, sensorPropertiesPtr.Raw);
            pinnedSensorInfos = GCHandle.Alloc(sensorInfos, GCHandleType.Pinned);
            sensorInfosPtr = pinnedSensorInfos.AddrOfPinnedObject();

            // Create XrCameraFrameBuffersQCOM structure
            using ScopeArrayPtr<XrCameraFrameBufferQCOM> fbArrayPtr = new((int)frameConfig.FrameBufferCount);
            using ScopeArrayPtr<XrCameraFrameHardwareBufferQCOM> fhwbArrayPtr = new((int)frameConfig.FrameBufferCount);
            bool hardwareBuffersAvailable = frameConfig.FrameHardwareBufferCount == frameConfig.FrameBufferCount;

            // If HardwareBuffers match FrameBuffers, assign a HardwareBuffer to every FrameBuffer based on the default FrameBuffer
            if (hardwareBuffersAvailable)
            {
                // Create HardwareBuffer array
                for (int i = 0; i < (int)frameConfig.FrameHardwareBufferCount; i++)
                {
                    fhwbArrayPtr.Copy(_defaultFrameHardwareBuffer, i);
                }
                // Create FrameBuffer array
                for (int i = 0; i < (int)frameConfig.FrameBufferCount; i++)
                {
                    XrCameraFrameBufferQCOM frameBuffer = new XrCameraFrameBufferQCOM(_defaultFrameBuffer, fhwbArrayPtr.AtIndexRaw(i));
                    fbArrayPtr.Copy(frameBuffer, i);
                }
            }
            // Otherwise, reuse the default FrameBuffer for all FrameBuffers
            else
            {
                for (int i = 0; i < (int)frameConfig.FrameBufferCount; i++)
                {
                    fbArrayPtr.Copy(_defaultFrameBuffer, i);
                }
            }

            XrCameraFrameBuffersQCOM frameBuffers = new XrCameraFrameBuffersQCOM(
                IntPtr.Zero,
                frameConfig.FrameBufferCount,
                fbArrayPtr.Raw
            );

            // Request data from runtime
            XrCameraFrameDataQCOM frameData = new XrCameraFrameDataQCOM(sensorInfosPtr);
            var result = _xrAccessFrameQCOM(cameraHandle, ref frameData, ref frameBuffers);
            if (result != XrResult.XR_SUCCESS)
            {
                if (result != XrResult.XR_ERROR_CAMERA_FRAME_NOT_READY_QCOMX)
                {
                    Debug.LogError("Failed to access frame: " + Enum.GetName(typeof(XrResult), result));
                }
                pinnedSensorInfos.Free();
                return false;
            }

            _frameReleased = false;

            // Skip received frame if it is the same as the last one
            if (_cachedFrameData.Handle == frameData.Handle)
            {
                pinnedSensorInfos.Free();

                if (!TryReleaseFrame())
                {
                    Debug.LogWarning("Could not release frame after requesting it.");
                }

                return true;
            }

            _cachedFrameData = frameData;

            // Extract sensor data
            _sensorProperties = new XrCameraSensorPropertiesQCOM[sensorCount];
            for (int i = 0; i < sensorCount; i++)
            {
                _sensorProperties[i] = sensorPropertiesPtr.AtIndex(i);
            }
            _lastFramePose = _sensorProperties[0].Extrinsic;

            pinnedSensorInfos.Free();

            // Extract CPU frame buffers
            _frameBuffers = new XrCameraFrameBufferQCOM[frameConfig.FrameBufferCount];
            for (int i = 0; i < frameConfig.FrameBufferCount; i++)
            {
                _frameBuffers[i] = fbArrayPtr.AtIndex(i);
            }

            // Extract GPU frame buffers
            if (hardwareBuffersAvailable)
            {
                _frameHardwareBuffers = new XrCameraFrameHardwareBufferQCOM[frameConfig.FrameBufferCount];
                for (int i = 0; i < frameConfig.FrameBufferCount; i++)
                {
                    _frameHardwareBuffers[i] = Marshal.PtrToStructure<XrCameraFrameHardwareBufferQCOM>(_frameBuffers[i].HardwareBuffer);
                }
            }

            // Cache YUV frame, 1 per frameBuffer/sensor

            if (_cachedYuvFrames == null || _cachedYuvFrames.Length != frameBuffers.FrameBufferCount)
            {
                _cachedYuvFrames = new SpacesYUVFrame[frameBuffers.FrameBufferCount];
            }

            for (int i = 0; i < frameConfig.FrameBufferCount; i++)
            {
                // Abstract planes for later access

                // YCbCr format layout for a 4x4 image:
                //
                // Y-UV variant - Y at 1:1 resolution, UV at 1:2 resolution
                //
                // YYYY    UVUV
                // YYYY    UVUV
                // YYYY
                // YYYY
                //
                // YUYV variant - Y at 1:1 resolution, UV at 1:1 vertically, 1:2 horizontally
                //
                // YUYVYUYV
                // YUYVYUYV
                // YUYVYUYV
                // YUYVYUYV
                //
                // ImagePlane is an abstraction of Y, U or V plane to sample the frameBuffer correctly, given Row and Column values.
                // ImagePlane is defined by: Root, Stride, Offset, Step, ColumnRate and RowRate
                // Root:    Index where plane data begins
                // Stride:  Row size in bytes
                // Offset:  Position of the correct byte inside a Y, UV -- YU, YV or YUYV byte group
                // Step:    Length of the byte group, Y(1), UV(2) -- YU(2), YV(2), YUYV(4)
                // ColumnRate:  Pixels represented by each byte group, horizontally.
                // RowRate:     Pixels represented by each byte group, vertically.
                //
                // For more information: https://en.wikipedia.org/wiki/YCbCr#Packed_pixel_formats_and_conversion

                ImagePlane yPlane = default;
                ImagePlane uPlane = default;
                ImagePlane vPlane = default;

                // NV12 has UV, NV21 has VU byte order
                bool swapuv = frameData.Format == XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_YUV420_NV21_QCOMX ^ _deviceIsA3;

                foreach (var plane in _frameBuffers[i].PlanesArray)
                {
                    switch (plane.PlaneType)
                    {
                        case XrCameraFramePlaneTypeQCOM.XR_CAMERA_FRAME_PLANE_TYPE_Y_QCOMX:
                            yPlane = new ImagePlane(plane.Offset, plane.Stride, 0, 1, 1, 1);
                            break;
                        case XrCameraFramePlaneTypeQCOM.XR_CAMERA_FRAME_PLANE_TYPE_UV_QCOMX:
                            uPlane = new ImagePlane(plane.Offset, plane.Stride, (uint)(swapuv ? 1 : 0), 2, 2, 2);
                            vPlane = new ImagePlane(plane.Offset, plane.Stride, (uint)(swapuv ? 0 : 1), 2, 2, 2);
                            break;
                        case XrCameraFramePlaneTypeQCOM.XR_CAMERA_FRAME_PLANE_TYPE_YUV_QCOMX:
                            yPlane = new ImagePlane(plane.Offset, plane.Stride, 0, 2, 1, 1);
                            uPlane = new ImagePlane(plane.Offset, plane.Stride, 1, 4, 2, 1);
                            vPlane = new ImagePlane(plane.Offset, plane.Stride, 3, 4, 2, 1);
                            break;
                    }
                }

                // Build and store cached frame
                _cachedYuvFrames[i] = new SpacesYUVFrame(
                    frameData.Handle,
                    frameData.Timestamp,
                    _sensorProperties[i].ImageDimensions.ToVector2Int(),
                    frameData.Format,
                    _frameBuffers[i].Buffer,
                    (int)_frameBuffers[i].BufferSize,
                    (int)_frameBuffers[i].PlaneCount,
                    yPlane,
                    uPlane,
                    vPlane);
            }

            if (!TryReleaseFrame())
            {
                Debug.LogWarning("Could not release frame after requesting it.");
            }

            return true;
        }

        public bool TryReleaseFrame()
        {
            if (_frameReleased)
            {
                Debug.LogWarning("Skipped releasing last frame: already released.");
                return true;
            }

            if (_xrReleaseFrameQCOM == null)
            {
                Debug.LogError("xrReleaseFrameQCOM method not found!");
                return false;
            }

            var result = _xrReleaseFrameQCOM(_cachedFrameData.Handle);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Failed to release frame: " + Enum.GetName(typeof(XrResult), result));
                return false;
            }

            _frameReleased = true;
            return true;
        }

        public bool XrTimeFromSpacesTimespec(Timespec time, out long convertedTime)
        {
            convertedTime = 0;
            if (_xrConvertTimespecTimeToTimeKHR == null)
            {
                Debug.LogError("xrConvertTimespecTimeToTimeKHR method not found!");
                return false;
            }

            var result = _xrConvertTimespecTimeToTimeKHR(_baseRuntimeFeature.InstanceHandle, ref time, ref convertedTime);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Fail in xrConvertTimespecTimeToTimeKHR: " + Enum.GetName(typeof(XrResult), result));
                return false;
            }

            return true;
        }
        public bool SpacesTimespecFromXRTime(long time, out Timespec convertedTime)
        {
            convertedTime = new Timespec();
            if (_xrConvertTimeToTimespecTimeKHR == null)
            {
                Debug.LogError("xrConvertTimeToTimespecTimeKHR method not found!");
                return false;
            }

            var result = _xrConvertTimeToTimespecTimeKHR(_baseRuntimeFeature.InstanceHandle, time, ref convertedTime);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError("Fail in xrConvertTimeToTimespecTimeKHR: " + Enum.GetName(typeof(XrResult), result));
                return false;
            }

            return true;
        }
    }
}
