using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class CameraAccess : MonoBehaviour
{
    [Header("Camera View Texture")]
    public RawImage CameraRawImage;
    //Native Resolution W : 3264, H : 2448 (8:6)

    private NativeArray<XRCameraConfiguration> _cameraConfigs;
    private ARCameraManager _cameraManager;
    private Texture2D _cameraTexture;
    
    private bool _configurationFound;
    private XRCpuImage _lastCpuImage;
    


    public void Awake()
    {
        _cameraManager = FindObjectOfType<ARCameraManager>();
    }

    public void Start()
    {


        _configurationFound = FindSupportedConfiguration();
        if (!_configurationFound)
        {
            return;
        }

        _cameraManager.frameReceived += OnFrameReceived;

    }

    private void OnFrameReceived(ARCameraFrameEventArgs args)
    {


        if (!_cameraManager.TryAcquireLatestCpuImage(out _lastCpuImage))
        {
            Debug.Log("Failed to acquire latest cpu image.");
            return;
        }

        UpdateCameraTexture(_lastCpuImage);
      
    }

    private unsafe void UpdateCameraTexture(XRCpuImage image)
    {
        var format = TextureFormat.RGBA32;

       
        var downsamplingFactor = Mathf.CeilToInt(image.width / 1632);//프레임 개선을 위해 3264의 반인 1632로 나누어줍니다.
        var outputDimensions = image.dimensions / downsamplingFactor;

        if (_cameraTexture == null || _cameraTexture.width != outputDimensions.x || _cameraTexture.height != outputDimensions.y)
        {
            _cameraTexture = new Texture2D(outputDimensions.x, outputDimensions.y, format, false);
        }

        var rawTextureData = _cameraTexture.GetRawTextureData<byte>();
        var rawTexturePtr = new IntPtr(rawTextureData.GetUnsafePtr());

       
        var conversionParams = new XRCpuImage.ConversionParams(image, format);
        try
        {
            conversionParams.inputRect = new RectInt(0, 0, image.width, image.height);
            conversionParams.outputDimensions = outputDimensions;
            image.Convert(conversionParams, rawTexturePtr, rawTextureData.Length);
        }
        finally
        {
            image.Dispose();
        }
        

        _cameraTexture.Apply();
        CameraRawImage.texture = _cameraTexture;
    }


    private bool FindSupportedConfiguration()
    {
        _cameraConfigs = _cameraManager.GetConfigurations(Allocator.Persistent);
        return _cameraConfigs.Length > 0;
    }








}
