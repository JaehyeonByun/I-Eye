/******************************************************************************
 * File: SpacesCompositionLayer.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    [MovedFrom(false, null, null, "SpacesQuadCompositionLayer")]
    public class SpacesCompositionLayer : MonoBehaviour
    {
        [Tooltip("The type of composition layer to use:\n" +
            "Quad: A texture will be projected onto a single-sided quadrilateral.\n" +
            // "Cube: A cube-map will be projected onto the interior faces of a cube.\n"
            "Cylinder: A texture will be projected onto the interior surface of a cylindrical section. Like a curved TV.\n" +
            "Spherical (Equirect): An equirectangular texture will be projected onto the interior surface of a sphere."// +
            //"Passthrough: A texture will be used as a replacement for actual passthrough content."
            )]
        [SerializeField]
        private SpacesCompositionLayerType _layerType = SpacesCompositionLayerType.Quad;

        [Header("Layer Rendering")]
        [SpacesEditorConditional(nameof(_useAndroidSurfaceSwapchain))]
        public Texture LayerTexture;

        [SpacesEditorConditional(nameof(_useAndroidSurfaceSwapchain))]
        [Tooltip("If checked, the layer texture will be copied on update. Turning this on will affect performance.")]
        public bool IsTextureDynamic;

        //[SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.Cube, HideInInspector:true)]
        //[SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.Passthrough, HideInInspector:true)]
        [HideInInspector]
        [SerializeField]
        [Tooltip("Composition layers using an Android Surface Swapchain must supply the desired size of the surface explicitly at creation time in the `SurfaceTextureSize` field.\n" +
            "These layers do not use the 'LayerTexture' field - instead they must use Android Java code to write to the image.\n" +
            "Writing in this way is not bound to the normal Unity Update cycle (they can be written to by any thread, as frequently as desired) as long as the XrSession is still valid.\n\n" +
            "If this is enabled at runtime it will invalidate the existing overlay image - the java code responsible for writing to the external surface must be called, or nothing will be visible for this composition layer.\n" +
            "If this is disabled at runtime the layer will be immediately refreshed once.")]
        private bool _useAndroidSurfaceSwapchain;

        [SpacesEditorConditional(nameof(_useAndroidSurfaceSwapchain), Value:false, HideInInspector: true)]
        [SerializeField]
        [Tooltip("The size of the Android Surface at creation time for this composition layer.")]
        private Vector2Int _surfaceTextureSize;

        [Header("Layer Positioning")]
        [Tooltip("If checked, the layer will use this object's Transform component for it's position and orientation. A Quad layer will use the scale to determine Size.")]
        public bool UseTransform;

        [SerializeField]
        [SpacesEditorConditional(nameof(UseTransform))]
        private Quaternion _orientation = Quaternion.identity;

        [SerializeField]
        [SpacesEditorConditional(nameof(UseTransform))]
        private Vector3 _position = new Vector3(0.0f, 0.0f, -1.0f);

        [SerializeField]
        [Tooltip("Sorting order of the layer. Layers with higher numbers are rendered later.\n\n" +
            "The projection layer is rendered at sorting order 0. Layers with negative numbers will therefore be rendered 'underneath' the projection layer.\n\n" +
            "It is recommended to explicitly order layers correctly for best results. " +
            "Layers sharing a sorting order are not guaranteed to be rendered in any particular order, but it should be consistent during the lifecycle of the application, but not necessarily from one execution to another.")]
        private int _sortingOrder = 1;

        [Header("Layer Data")]
        [SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.Quad, HideInInspector:true, Inverse: true)]
        [SerializeField]
        [FormerlySerializedAs("_extents")]
        [Tooltip("The size of the quad layer in m.")]
        private Vector2 _size = new Vector2(0.1f, 0.1f);

        [SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.Cylinder, HideInInspector:true, Inverse: true)]
        [SerializeField]
        [Tooltip("Non-negative radius of the layer. Zero or float.PositiveInfinity is treated as a layer with an infinite radius.\n\n" +
            "A layer with an infinite radius should likely have a negative sorting order to be rendered before the projection layer.")]
        [Range(0, float.PositiveInfinity)]
        private float _cylinderRadius;

        [SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.SphericalEquirect, HideInInspector:true, Inverse: true)]
        [SerializeField]
        [Tooltip("Non-negative radius of the layer. Zero or float.PositiveInfinity is treated as a layer with an infinite radius.\n\n" +
            "A layer with an infinite radius should likely have a negative sorting order to be rendered before the projection layer.")]
        [Range(0, float.PositiveInfinity)]
        private float _sphereRadius;

        [SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.Cylinder, HideInInspector:true, Inverse: true)]
        [SerializeField]
        [Range(0, Mathf.PI * 2f)]
        [Tooltip("Visible horizontal angle of the cylinder in the range 0 -> 2pi. It grows symmetrically around the 0 radian angle.\n" +
            "E.g. A layer with a central angle of pi, will be projected onto the interior surface of a hemicylinder with the midpoint of the projection visible directly forward from the Position of the layer.")]
        private float _centralAngle = Mathf.PI;

        [SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.SphericalEquirect, HideInInspector:true, Inverse: true)]
        [SerializeField]
        [Range(0, Mathf.PI * 2f)]
        [Tooltip("Visible horizontal angle of the sphere in the range 0 -> 2pi. It grows symmetrically around the 0 radian angle.\n" +
            "E.g. A layer with a central horizontal angle of pi, will be projected onto the interior surface of a hemisphere with the midpoint of the projection visible directly forward from the Position of the layer.")]
        private float _centralHorizontalAngle = Mathf.PI;

        [SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.SphericalEquirect, HideInInspector:true, Inverse: true)]
        [SerializeField]
        [Range(-Mathf.PI / 2f, Mathf.PI / 2f)]
        [Tooltip("Defines the lower vertical angle of the visible portion of the sphere, in the range -pi/2 -> pi/2")]
        private float _lowerVerticalAngle = Mathf.PI / 2f;

        [SpacesEditorConditional(nameof(_layerType), SpacesCompositionLayerType.SphericalEquirect, HideInInspector:true, Inverse: true)]
        [SerializeField]
        [Range(-Mathf.PI / 2, Mathf.PI / 2)]
        [Tooltip("Defines the upper vertical angle of the visible portion of the sphere, in the range -pi/2 -> pi/2")]
        private float _upperVerticalAngle = Mathf.PI / 2f;

        internal CompositionLayersFeature _compositionLayersFeature;
        private bool IsCompositionLayersFeatureValid => FeatureUseCheckUtility.IsFeatureUseable(_compositionLayersFeature);
        private uint _layerId;
        private static readonly float _lessThan2PI = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(2f * Mathf.PI) - 1);

        /// <summary>
        ///     Has initiated configuration after ConfigureLayer has been called, either on Start, or on the next update after the composition layer feature is enabled following a ForceReconfigure
        /// </summary>
        private bool _initiatedConfiguration;

        /// <summary>
        ///     The overlay is considered to have been created when it has a valid id assigned to it.
        ///     Only after this, can it then be queried in the native library from the composition layer provider.
        /// </summary>
        private bool _overlayCreated;

        /// <summary>
        ///     The overlay is considered populated when it has had some valid data written to it in its lifetime.
        ///     When _useAndroidSurfaceSwapchain is true, the overlay is considered to be populated once the layer has initially
        ///     cleared its data: data may or may not have been written to the Surface externally, elsewhere, but this
        ///     implementation does not know about it.
        ///     The only way to populate the data is by calling UpdateSwapchainImage successfully (i.e. not while a frame is in
        ///     progress).
        ///     If the value of _useAndroidSurfaceSwapchain changes, or if the layer is reconfigured somehow, _overlayPopulated
        ///     should be reset to false.
        /// </summary>
        private bool _overlayPopulated;

        private Dictionary<IntPtr, Texture2D> _swapchainImages = new Dictionary<IntPtr, Texture2D>();

        internal uint LayerId => _layerId;

        public SpacesCompositionLayerType LayerType => _layerType;

        public Vector2Int SurfaceTextureSize
        {
            get => _surfaceTextureSize;
            private set => _surfaceTextureSize = value;
        }

        public Quaternion Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                if (_overlayCreated)
                {
                    _compositionLayersFeature.SetOrientationForLayer(_layerId, _orientation);
                }
            }
        }

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                if (_overlayCreated)
                {
                    _compositionLayersFeature.SetPositionForLayer(_layerId, _position);
                }
            }
        }

        public int SortingOrder
        {
            get => _sortingOrder;
            set
            {
                _sortingOrder = value;
                if (_overlayCreated)
                {
                    _compositionLayersFeature.SetSortingOrderForLayer(_layerId, _sortingOrder);
                }
            }
        }

        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                if (_overlayCreated)
                {
                    _compositionLayersFeature.SetSizeForQuadLayer(_layerId, _size);
                }
            }
        }

        public float Radius
        {
            get
            {
                if (LayerType == SpacesCompositionLayerType.Cylinder)
                {
                    return _cylinderRadius;
                }
                else if (LayerType == SpacesCompositionLayerType.SphericalEquirect)
                {
                    return _sphereRadius;
                }

                return 0;
            }
            set
            {
                if (LayerType == SpacesCompositionLayerType.Cylinder)
                {
                    _cylinderRadius = Mathf.Clamp(value, 0f, float.PositiveInfinity);;
                    if (_overlayCreated)
                    {
                        _compositionLayersFeature.SetRadiusForCylinderLayer(_layerId, _cylinderRadius);
                    }
                }
                else if (LayerType == SpacesCompositionLayerType.SphericalEquirect)
                {
                    _sphereRadius = Mathf.Clamp(value, 0f, float.PositiveInfinity);
                    if (_overlayCreated)
                    {
                        _compositionLayersFeature.SetRadiusForEquirectLayer(_layerId, _sphereRadius);
                    }
                }
            }
        }

        public float CentralAngle
        {
            get
            {
                if (LayerType == SpacesCompositionLayerType.Cylinder)
                {
                    return _centralAngle;
                }
                else if (LayerType == SpacesCompositionLayerType.SphericalEquirect)
                {
                    return _centralHorizontalAngle;
                }

                return 0;
            }
            set
            {
                if (LayerType == SpacesCompositionLayerType.Cylinder)
                {
                    // per the specification this must be strictly less than 2 PI
                    // https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrCompositionLayerCylinderKHR.html
                    _centralAngle = Mathf.Clamp(value, 0f, _lessThan2PI);
                    if (_overlayCreated)
                    {
                        _compositionLayersFeature.SetCentralAngleForCylinderLayer(_layerId, _centralAngle);
                    }

                }
                else if (LayerType == SpacesCompositionLayerType.SphericalEquirect)
                {
                    _centralHorizontalAngle = Mathf.Clamp(value, 0f, 2f*Mathf.PI);
                    if (_overlayCreated)
                    {
                        _compositionLayersFeature.SetCentralHorizontalAngleForEquirectLayer(_layerId, _centralHorizontalAngle);
                    }
                }
            }
        }

        public float LowerVerticalAngle
        {
            get => _lowerVerticalAngle;
            set
            {
                _lowerVerticalAngle = Mathf.Clamp(value, -Mathf.PI/2f, Mathf.PI/2f);
                if (_overlayCreated)
                {
                    _compositionLayersFeature.SetLowerVerticalAngleForEquirectLayer(_layerId, _lowerVerticalAngle);
                }
            }
        }

        public float UpperVerticalAngle
        {
            get => _upperVerticalAngle;
            set
            {
                _upperVerticalAngle = Mathf.Clamp(value, -Mathf.PI/2f, Mathf.PI/2f);;
                if (_overlayCreated)
                {
                    _compositionLayersFeature.SetUpperVerticalAngleForEquirectLayer(_layerId, _upperVerticalAngle);
                }
            }
        }

        public bool UseAndroidSurfaceSwapchain
        {
            get => _useAndroidSurfaceSwapchain;
            // private set
            // {
            //     if (value != _useAndroidSurfaceSwapchain)
            //     {
            //         _useAndroidSurfaceSwapchain = value;
            //         _overlayPopulated = false;
            //
            //         if (_overlayCreated)
            //         {
            //             _compositionLayersFeature.SetUseAndroidSurfaceSwapchain(_layerId, _useAndroidSurfaceSwapchain);
            //         }
            //     }
            // }
        }

        private void Start()
        {
            _compositionLayersFeature = OpenXRSettings.Instance.GetFeature<CompositionLayersFeature>();

            if (!IsCompositionLayersFeatureValid)
            {
#if !UNITY_EDITOR
                Debug.LogWarning("CompositionLayersFeature is unavailable!");
#endif
                return;
            }

            ConfigureLayer();
        }

        private void ConfigureLayer()
        {
            // get configuration data for this layer and then submit the layer for creation on the render thread.
            // this will prevent the call to create swapchain being made without a valid gl context.
            // compositionLayerConfig will be freed on the render thread after it has been processed.
            _initiatedConfiguration = true;
            IntPtr compositionLayerConfig = _compositionLayersFeature.ConfigurationData(this);
            SpacesRenderEventUtility.SubmitRenderEventAndData(SpacesRenderEvent.ConfigureSwapchain, compositionLayerConfig);
        }

        private void InitialiseLayerPhysicalAttributes()
        {
            if (!UseTransform)
            {
                _compositionLayersFeature.SetPositionForLayer(_layerId, Position);
                _compositionLayersFeature.SetOrientationForLayer(_layerId, Orientation);
            }

            switch (LayerType)
            {
                case SpacesCompositionLayerType.Quad:
                    if (!UseTransform)
                    {
                        _compositionLayersFeature.SetSizeForQuadLayer(_layerId, Size);
                    }
                    break;
                case SpacesCompositionLayerType.Cylinder:
                    _compositionLayersFeature.SetRadiusForCylinderLayer(_layerId, Radius);
                    _compositionLayersFeature.SetCentralAngleForCylinderLayer(_layerId, CentralAngle);
                    break;
                case SpacesCompositionLayerType.SphericalEquirect:
                    _compositionLayersFeature.SetRadiusForEquirectLayer(_layerId, Radius);
                    _compositionLayersFeature.SetCentralHorizontalAngleForEquirectLayer(_layerId, CentralAngle);
                    _compositionLayersFeature.SetLowerVerticalAngleForEquirectLayer(_layerId, LowerVerticalAngle);
                    _compositionLayersFeature.SetUpperVerticalAngleForEquirectLayer(_layerId, UpperVerticalAngle);
                    break;
                //case SpacesCompositionLayerType.Passthrough:
                default:
                    break;
            }
        }

        private void Update()
        {
            if (!_overlayCreated)
            {
                if (!_initiatedConfiguration && IsCompositionLayersFeatureValid)
                {
                    ConfigureLayer();
                }

                return;
            }

            if (IsTextureDynamic || !_overlayPopulated)
            {
                UpdateSwapchainImage();
            }

            if (UseTransform)
            {
                Vector3 position = transform.position;
                position.z = -position.z;
                Quaternion rotation = transform.rotation;
                rotation.z = -rotation.z;
                rotation.w = -rotation.w;
                _compositionLayersFeature.SetPositionForLayer(_layerId, position);
                _compositionLayersFeature.SetOrientationForLayer(_layerId, rotation);

                if (SpacesCompositionLayerType.Quad == LayerType)
                {
                    _compositionLayersFeature.SetSizeForQuadLayer(_layerId, transform.localScale);
                }
            }
        }

        private void OnEnable()
        {
            if (_overlayCreated)
            {
                _compositionLayersFeature.SetLayerVisible(_layerId, true);
            }
        }

        private void OnDisable()
        {
            if (_overlayCreated)
            {
                _compositionLayersFeature.SetLayerVisible(_layerId, false);
            }
        }

        private void OnDestroy()
        {
            if (_overlayCreated)
            {
                // the layerIdPtr will be freed on the render thread after it has been processed.
                IntPtr layerIdPtr = Marshal.AllocHGlobal(Marshal.SizeOf<uint>());
                Marshal.StructureToPtr(_layerId, layerIdPtr, false);
                SpacesRenderEventUtility.SubmitRenderEventAndData(SpacesRenderEvent.DestroySwapchain, layerIdPtr);
            }

            _swapchainImages.Clear();
        }

        /// <summary>
        ///     Fetch an Android Surface created by this layer.
        /// </summary>
        /// <returns>
        ///     A SpacesAndroidSurface object containing the java android.view.Surface pointer to be passed to Java code for
        ///     rendering, or null if this layer is not using an Android Surface Swapchain.
        /// </returns>
        public SpacesAndroidSurface GetAndroidSurface()
        {
            if (!_overlayCreated || !UseAndroidSurfaceSwapchain)
            {
                return null;
            }

#if UNITY_ANDROID
            return _compositionLayersFeature.GetAndroidSurfaceObject(_layerId);
#else
            Debug.LogError("Cannot access android surface object if not on android");
            return null;
#endif
        }

        private void UpdateSwapchainImage()
        {
            if (_useAndroidSurfaceSwapchain)
            {
                if (!_overlayPopulated)
                {
                    IntPtr surfaceRenderingClass = AndroidJNI.FindClass("com/qualcomm/snapdragon/spaces/serviceshelper/SurfaceRendering");
                    var clearSurface = AndroidJNI.GetStaticMethodID(surfaceRenderingClass, "clearSurface", "(Landroid/view/Surface;)V");
                    AndroidJNI.CallStaticVoidMethod(surfaceRenderingClass, clearSurface,
                        new jvalue[] { new() { l = GetAndroidSurface().ExternalSurface } });

                    _overlayPopulated = true;
                }

                return;
            }

            bool wasPopulated = false;
            // If we are on Windows, render natively
            if (_compositionLayersFeature.UseNativeTexture())
            {
                IntPtr natPtr = LayerTexture.GetNativeTexturePtr();
                if (natPtr != IntPtr.Zero)
                {
                    if (SystemInfo.graphicsUVStartsAtTop)
                    {
                        var tempRenderTex = RenderTexture.GetTemporary(LayerTexture.width, LayerTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                        Graphics.Blit(LayerTexture, tempRenderTex, new Vector2(1.0f, -1.0f), new Vector2(0.0f, 1.0f));
                        _compositionLayersFeature.SendToSwapchain(_layerId, tempRenderTex.GetNativeTexturePtr(), LayerTexture.width, LayerTexture.height);
                        RenderTexture.ReleaseTemporary(tempRenderTex);
                    }
                    else
                    {
                        _compositionLayersFeature.SendToSwapchain(_layerId, natPtr, LayerTexture.width, LayerTexture.height);
                    }
                }

                wasPopulated = true;
            }
            else
            {
                // graphicsUVStartsAtTop is false for gles3, true for vulkan
                if (!SystemInfo.graphicsUVStartsAtTop)
                {
                    IntPtr swapchainImagePtr = _compositionLayersFeature.AcquireSwapchainImageForLayer(_layerId);
                    if (swapchainImagePtr != IntPtr.Zero)
                    {
                        if (!_swapchainImages.ContainsKey(swapchainImagePtr))
                        {
                            _swapchainImages.Add(swapchainImagePtr, Texture2D.CreateExternalTexture(LayerTexture.width, LayerTexture.height, TextureFormat.ARGB32, false, true, swapchainImagePtr));
                        }

                        Graphics.CopyTexture(LayerTexture, _swapchainImages[swapchainImagePtr]);
                        _compositionLayersFeature.ReleaseSwapchainImageForLayer(_layerId);
                    }

                    wasPopulated = true;
                }
                // Dont update/acquire swapchain image if this call occurs between calls to XrBeginFrame and XrEndFrame because it causes crashes (esp. around scene transitions)
                else if (!_compositionLayersFeature.IsXrFrameInProgress())
                {
                    IntPtr swapchainImagePtr = _compositionLayersFeature.AcquireSwapchainImageForLayer(_layerId);
                    if (swapchainImagePtr != IntPtr.Zero)
                    {
                        if (!_swapchainImages.ContainsKey(swapchainImagePtr))
                        {
                            _swapchainImages.Add(swapchainImagePtr, Texture2D.CreateExternalTexture(LayerTexture.width, LayerTexture.height, TextureFormat.ARGB32, false, true, swapchainImagePtr));
                        }

                        var tempRenderTex = RenderTexture.GetTemporary(LayerTexture.width, LayerTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                        Graphics.Blit(LayerTexture, tempRenderTex, new Vector2(1.0f, -1.0f), new Vector2(0.0f, 1.0f));
                        Graphics.CopyTexture(tempRenderTex, _swapchainImages[swapchainImagePtr]);
                        RenderTexture.ReleaseTemporary(tempRenderTex);
                        _compositionLayersFeature.ReleaseSwapchainImageForLayer(_layerId);
                    }

                    wasPopulated = true;
                }
            }

            if (!_overlayPopulated && wasPopulated)
            {
                _overlayPopulated = true;
                // this layer has been populated with data so can now be made visible. Base this on the current `activeInHierarchy` value
                _compositionLayersFeature.SetLayerVisible(_layerId, gameObject.activeInHierarchy);
            }
        }

        /// <summary>
        ///     Called on the render thread when finished creating the swapchain and there is now a valid layer id for this object.
        ///     The layer is invisible at this point (if not using AndroidSurfaceSwapchain) and must be made visible after it is populated.
        /// </summary>
        /// <param name="layerId">The id to be assigned to this layer.</param>
        internal void OnConfigured(uint layerId)
        {
            // Very limited in terms of available actions in this context. Can only change simple fields on the layer to be configured.
            // Assume that there is no access to GameObject internals.
            // Logging is risky - likely to crash.
            _layerId = layerId;

            InitialiseLayerPhysicalAttributes();

            _overlayCreated = true;
            _overlayPopulated = false;
        }

        internal void ForceReconfigure()
        {
            _overlayCreated = false;
            _overlayPopulated = false;
            _layerId = 0;
            _initiatedConfiguration = false;
        }
    }
}
