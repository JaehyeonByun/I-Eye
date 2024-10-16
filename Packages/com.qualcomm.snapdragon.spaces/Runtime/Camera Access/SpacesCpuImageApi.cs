/******************************************************************************
 * File: SpacesCpuImageApi.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Qualcomm.Snapdragon.Spaces;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

public class SpacesCpuImageApi : XRCpuImage.Api
{
    private bool _deviceIsA3;
    private List<XRCpuImage.Format> _supportedInputFormats = new List<XRCpuImage.Format> { XRCpuImage.Format.AndroidYuv420_888 };

    private List<TextureFormat> _supportedOutputFormats = new List<TextureFormat> { TextureFormat.RGB24, TextureFormat.RGBA32, TextureFormat.BGRA32 };

    private CameraAccessFeature _underlyingFeature = OpenXRSettings.Instance.GetFeature<CameraAccessFeature>();
    public static SpacesCpuImageApi instance { get; private set; }

    // Cache byte buffers to avoid re-allocation each frame
    private byte[] _sourcePixels;
    private byte[] _outPixels;

    public static SpacesCpuImageApi CreateInstance()
    {
        instance ??= new SpacesCpuImageApi();
        return instance;
    }

    public override bool NativeHandleValid(int nativeHandle)
    {
        foreach (var frameData in _underlyingFeature.CachedYuvFrames)
        {
            if (nativeHandle == (int)frameData.Handle)
            {
                return true;
            }
        }
        return false;
    }

    public override bool FormatSupported(XRCpuImage image, TextureFormat format)
    {
        if (!_supportedInputFormats.Contains(image.format))
        {
            return false;
        }

        if (!_supportedOutputFormats.Contains(format))
        {
            return false;
        }

        return true;
    }

    public override bool TryGetPlane(int nativeHandle, int planeIndex, out XRCpuImage.Plane.Cinfo planeCinfo)
    {
        planeCinfo = new XRCpuImage.Plane.Cinfo();

        if (!NativeHandleValid(nativeHandle))
        {
            Debug.LogWarning("Native handle [" + nativeHandle + "] is not valid. The frame might have expired.");
            return false;
        }

        if (!GetFrameFromHandle(nativeHandle, out SpacesYUVFrame frame))
        {
            Debug.LogError($"Failed to retrieve cached frame for handle [{nativeHandle}]");
            return false;
        }

        IntPtr dataPtr;
        int dataLength;

        switch (frame.Format)
        {
            // YUV420 format -  2 Planes: Y, UV
            case XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_YUV420_NV12_QCOMX:
            case XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_YUV420_NV21_QCOMX:
                switch (planeIndex)
                {
                    // XRCpuImage.GetPlane(0) : Y plane
                    case 0:
                        dataPtr = frame.DataPtr + (int)frame.YPlane.Root;
                        dataLength = (int)frame.YPlane.Stride * frame.Dimensions.y;
                        planeCinfo = new XRCpuImage.Plane.Cinfo(dataPtr, dataLength, (int) frame.YPlane.Stride, 1);
                        break;
                    // XRCpuImage.GetPlane(1) : UV / VU plane
                    case 1:
                        // We use the same Root and Stride for NV12 and NV21 since they share the underlying plane
                        dataPtr = frame.DataPtr + (int)frame.UPlane.Root;
                        dataLength = (int)frame.UPlane.Stride * (frame.Dimensions.y / 2);
                        planeCinfo = new XRCpuImage.Plane.Cinfo(dataPtr, dataLength, (int) frame.UPlane.Stride, 2);
                        break;
                }
                break;
            // YUYV format -    1 Plane, : YUYV
            case XrCameraFrameFormatQCOM.XR_CAMERA_FRAME_FORMAT_YUYV_QCOMX:
                switch (planeIndex)
                {
                    // XRCpuImage.GetPlane(0) : YUYV plane
                    case 0:
                        dataPtr = frame.DataPtr + (int)frame.YPlane.Root;
                        dataLength = (int)frame.YPlane.Stride * frame.Dimensions.y;
                        planeCinfo = new XRCpuImage.Plane.Cinfo(dataPtr, dataLength, (int) frame.UPlane.Stride, 4);
                        break;
                }
                break;
        }

        return true;
    }

    public override bool TryGetConvertedDataSize(int nativeHandle, Vector2Int dimensions, TextureFormat format, out int size)
    {
        size = 0;

        if (!NativeHandleValid(nativeHandle) || dimensions.x < 0 || dimensions.y < 0)
        {
            return false;
        }

        if (!_supportedOutputFormats.Contains(format))
        {
            return false;
        }

        switch (format)
        {
            case TextureFormat.RGB24:
                size = dimensions.x * dimensions.y * 3;
                break;
            case TextureFormat.RGBA32:
            case TextureFormat.BGRA32:
                size = dimensions.x * dimensions.y * 4;
                break;
        }

        return true;
    }

    public override bool TryConvert(int nativeHandle, XRCpuImage.ConversionParams conversionParams, IntPtr destinationBuffer, int bufferLength)
    {
        if (_underlyingFeature.DirectMemoryAccessConversion)
        {
            return TryConvertUsingNativeArrays(nativeHandle, conversionParams, destinationBuffer, bufferLength);
        }
        return TryConvertUsingCachedBuffers(nativeHandle, conversionParams, destinationBuffer, bufferLength);
    }

    public bool TryConvertUsingNativeArrays(int nativeHandle, XRCpuImage.ConversionParams conversionParams, IntPtr destinationBuffer, int bufferLength)
    {
        if (!NativeHandleValid(nativeHandle) || !_supportedOutputFormats.Contains(conversionParams.outputFormat))
        {
            return false;
        }

        // Conversion parameters
        var inputRect = conversionParams.inputRect;
        var outputDimensions = conversionParams.outputDimensions;
        var mirrorX = (conversionParams.transformation & XRCpuImage.Transformation.MirrorX) != 0;
        var mirrorY = (conversionParams.transformation & XRCpuImage.Transformation.MirrorY) != 0;

        if (!GetFrameFromHandle(nativeHandle, out SpacesYUVFrame frame))
        {
            Debug.LogError($"Failed to retrieve cached frame for handle [{nativeHandle}]");
            return false;
        }

        unsafe
        {
            NativeArray<byte> nativeSourcePixels = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>((void*)frame.DataPtr, frame.DataLength, Allocator.Invalid);
            NativeArray<byte> nativeOutPixels = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>((void*)destinationBuffer, bufferLength, Allocator.Invalid);
            ImagePlane yPlane = frame.YPlane;
            ImagePlane uPlane = frame.UPlane;
            ImagePlane vPlane = frame.VPlane;

            for (int row = 0; row < outputDimensions.y; row++)
            {
                for (int col = 0; col < outputDimensions.x; col++)
                {
                    // Nearest neighbour mapping from the output rectangle (target buffer) to the input rectangle (source image)
                    int sourceRow = (int)((inputRect.yMin + inputRect.height * (row / (float)outputDimensions.y)));
                    int sourceCol = (int)((inputRect.xMin + inputRect.width * (col / (float)outputDimensions.x)));

                    var y = nativeSourcePixels[yPlane.GetOffset(sourceCol, sourceRow)];
                    sbyte u = (sbyte)(nativeSourcePixels[uPlane.GetOffset(sourceCol, sourceRow)] - 128);
                    sbyte v = (sbyte)(nativeSourcePixels[vPlane.GetOffset(sourceCol, sourceRow)] - 128);

                    // YUV NV21 to RGB conversion
                    // https://en.wikipedia.org/wiki/YUV#Y%E2%80%B2UV420sp_(NV21)_to_RGB_conversion_(Android)

                    var r = y + (1.370705f * v);
                    var g = y - (0.698001f * v) - (0.337633f * u);
                    var b = y + (1.732446f * u);

                    r = r > 255 ? 255 : r < 0 ? 0 : r;
                    g = g > 255 ? 255 : g < 0 ? 0 : g;
                    b = b > 255 ? 255 : b < 0 ? 0 : b;

                    // Mirror output pixel across X axis (mirror rows) and Y axis (mirror columns)
                    int outputRow = mirrorX ? row : outputDimensions.y - row - 1;
                    int outputCol = mirrorY ? outputDimensions.x - col - 1 : col;
                    int pixelIndex = (outputRow * outputDimensions.x) + outputCol;

                    switch (conversionParams.outputFormat)
                    {
                        case TextureFormat.RGB24:
                            nativeOutPixels[3 * pixelIndex] = (byte)r;
                            nativeOutPixels[(3 * pixelIndex) + 1] = (byte)g;
                            nativeOutPixels[(3 * pixelIndex) + 2] = (byte)b;
                            break;
                        case TextureFormat.RGBA32:
                            nativeOutPixels[4 * pixelIndex] = (byte)r;
                            nativeOutPixels[(4 * pixelIndex) + 1] = (byte)g;
                            nativeOutPixels[(4 * pixelIndex) + 2] = (byte)b;
                            nativeOutPixels[(4 * pixelIndex) + 3] = 255;
                            break;
                        case TextureFormat.BGRA32:
                            nativeOutPixels[4 * pixelIndex] = (byte)b;
                            nativeOutPixels[(4 * pixelIndex) + 1] = (byte)g;
                            nativeOutPixels[(4 * pixelIndex) + 2] = (byte)r;
                            nativeOutPixels[(4 * pixelIndex) + 3] = 255;
                            break;
                    }
                }
            }
        }
        return true;
    }

    public bool TryConvertUsingCachedBuffers(int nativeHandle, XRCpuImage.ConversionParams conversionParams, IntPtr destinationBuffer, int bufferLength)
    {
        if (!NativeHandleValid(nativeHandle) || !_supportedOutputFormats.Contains(conversionParams.outputFormat))
        {
            return false;
        }

        // Conversion parameters
        var inputRect = conversionParams.inputRect;
        var outputDimensions = conversionParams.outputDimensions;
        var mirrorX = (conversionParams.transformation & XRCpuImage.Transformation.MirrorX) != 0;
        var mirrorY = (conversionParams.transformation & XRCpuImage.Transformation.MirrorY) != 0;

        if (!GetFrameFromHandle(nativeHandle, out SpacesYUVFrame frame))
        {
            Debug.LogError($"Failed to retrieve cached frame for handle [{nativeHandle}]");
            return false;
        }

        // Initialise pixel cache
        if (_sourcePixels == null || _sourcePixels.Length != frame.DataLength)
        {
            _sourcePixels = new byte[frame.DataLength];
        }
        if (_outPixels == null || _outPixels.Length != bufferLength)
        {
            _outPixels = new byte[bufferLength];
        }

        Marshal.Copy(frame.DataPtr, _sourcePixels, 0, frame.DataLength);
        ImagePlane yPlane = frame.YPlane;
        ImagePlane uPlane = frame.UPlane;
        ImagePlane vPlane = frame.VPlane;

        for (int row = 0; row < outputDimensions.y; row++)
        {
            for (int col = 0; col < outputDimensions.x; col++)
            {
                // Nearest neighbour mapping from the output rectangle (target buffer) to the input rectangle (source image)
                int sourceRow = (int)((inputRect.yMin + inputRect.height * (row / (float)outputDimensions.y)));
                int sourceCol = (int)((inputRect.xMin + inputRect.width * (col / (float)outputDimensions.x)));

                var y = _sourcePixels[yPlane.GetOffset(sourceCol, sourceRow)];
                sbyte u = (sbyte)(_sourcePixels[uPlane.GetOffset(sourceCol, sourceRow)] - 128);
                sbyte v = (sbyte)(_sourcePixels[vPlane.GetOffset(sourceCol, sourceRow)] - 128);

                // YUV NV21 to RGB conversion
                // https://en.wikipedia.org/wiki/YUV#Y%E2%80%B2UV420sp_(NV21)_to_RGB_conversion_(Android)

                var r = y + (1.370705f * v);
                var g = y - (0.698001f * v) - (0.337633f * u);
                var b = y + (1.732446f * u);

                r = r > 255 ? 255 : r < 0 ? 0 : r;
                g = g > 255 ? 255 : g < 0 ? 0 : g;
                b = b > 255 ? 255 : b < 0 ? 0 : b;

                // Mirror output pixel across X axis (mirror rows) and Y axis (mirror columns)
                int outputRow = mirrorX ? row : outputDimensions.y - row - 1;
                int outputCol = mirrorY ? outputDimensions.x - col - 1 : col;
                int pixelIndex = (outputRow * outputDimensions.x) + outputCol;

                switch (conversionParams.outputFormat)
                {
                    case TextureFormat.RGB24:
                        _outPixels[3 * pixelIndex] = (byte)r;
                        _outPixels[(3 * pixelIndex) + 1] = (byte)g;
                        _outPixels[(3 * pixelIndex) + 2] = (byte)b;
                        break;
                    case TextureFormat.RGBA32:
                        _outPixels[4 * pixelIndex] = (byte)r;
                        _outPixels[(4 * pixelIndex) + 1] = (byte)g;
                        _outPixels[(4 * pixelIndex) + 2] = (byte)b;
                        _outPixels[(4 * pixelIndex) + 3] = 255;
                        break;
                    case TextureFormat.BGRA32:
                        _outPixels[4 * pixelIndex] = (byte)b;
                        _outPixels[(4 * pixelIndex) + 1] = (byte)g;
                        _outPixels[(4 * pixelIndex) + 2] = (byte)r;
                        _outPixels[(4 * pixelIndex) + 3] = 255;
                        break;
                }
            }
        }

        Marshal.Copy(_outPixels, 0, destinationBuffer, bufferLength);
        return true;
    }

    public override void DisposeImage(int nativeHandle)
    {
        // NOTE(CH): No need to dispose images. The underlying feature takes care of
        // releasing frames from the runtime after requesting and of managing the
        // frame cache's native memory. We override to avoid a NotImplementedException.
    }

    private bool GetFrameFromHandle(int nativeHandle, out SpacesYUVFrame frame)
    {
        frame = null;

        foreach (var frameData in _underlyingFeature.CachedYuvFrames)
        {
            if (nativeHandle == (int)frameData.Handle)
            {
                frame = frameData;
                return true;
            }
        }
        return false;
    }
}
