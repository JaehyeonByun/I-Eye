// /******************************************************************************
//  * File: XRRayInteractorLineVisual.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace QCHT.Interactions.Distal
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(XRRayInteractor))]
    public class XRRayInteractorLineVisual : MonoBehaviour
    {
        [SerializeField] private Gradient hoverColorGradient = new Gradient
        {
            colorKeys = new[] {new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)},
            alphaKeys = new[] {new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f)},
        };
        
        [SerializeField] private Gradient selectColorGradient = new Gradient
        {
            colorKeys = new[] {new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)},
            alphaKeys = new[] {new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f)},
        };

        public Gradient SelectColorGradient
        {
            get => selectColorGradient;
            set => selectColorGradient = value;
        }

        [SerializeField] private Gradient idleColorGradient = new Gradient
        {
            colorKeys = new[] {new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)},
            alphaKeys = new[] {new GradientAlphaKey(0f, 0f), new GradientAlphaKey(.2f, 1f)},
        };

        public Gradient IdleColorGradient
        {
            get => idleColorGradient;
            set => idleColorGradient = value;
        }

        [SerializeField, Range(2, 100)] private int lineQuality = 10;

        public int LineQuality
        {
            get => lineQuality;
            set => lineQuality = Math.Clamp(value, 2, 100);
        }

        [SerializeField, Range(0, .05f)] private float lineWidthMultiplier = .01f;

        public float LineWidthMultiplier
        {
            get => lineWidthMultiplier;
            set => lineWidthMultiplier = value;
        }

        [SerializeField] private bool hideWhenNoHit;

        public bool HideWhenNoHit
        {
            get => hideWhenNoHit;
            set => hideWhenNoHit = value;
        }

        [SerializeField] private bool enableOverUI;

        public bool EnableOverUI
        {
            get => enableOverUI;
            set => enableOverUI = value;
        }
        
        [SerializeField] private bool enableOver3D = true;

        public bool EnableOver3D
        {
            get => enableOver3D;
            set => enableOver3D = value;
        }
        
        [Header("Bending")]
        [SerializeField] private bool bendEnabled = true;

        public bool BendEnabled
        {
            get => bendEnabled;
            set => bendEnabled = value;
        }

        [SerializeField, Range(-.5f, .5f)] private float bendYRatio;

        public float BendYRatio
        {
            get => bendYRatio;
            set => bendYRatio = value;
        }

        [SerializeField] private float bendSpeed = 10f;

        public float BendSpeed
        {
            get => bendSpeed;
            set => bendSpeed = value;
        }

        [SerializeField] private float offSet;
        public float OffSet
        {
            get => offSet;
            set => offSet = value;
        }

        private LineRenderer _lineRenderer;
        private XRRayInteractor _rayInteractor;
        private Collider _selectedCollider;

        private Vector3 _previousControlPoint;
        private Vector3 _selectedOffset;

        private Vector3[] _linePoints = Array.Empty<Vector3>();
        private Vector3[] _renderPoints = Array.Empty<Vector3>();
        
        private readonly Vector3[] _emptyArray = Array.Empty<Vector3>();

        #region MonoBehaviour Functions

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _rayInteractor = GetComponent<XRRayInteractor>();
            _renderPoints = new Vector3[lineQuality];
            ClearLine();
        }

        private void OnEnable()
        {
            _lineRenderer.enabled = true;
            Application.onBeforeRender += OnBeforeRenderLineVisual;
            _rayInteractor.selectEntered.AddListener(OnSelectEntered);
            _rayInteractor.selectExited.AddListener(OnSelectExited);
        }

        private void OnDisable()
        {
            _lineRenderer.enabled = false;
            Application.onBeforeRender -= OnBeforeRenderLineVisual;
            _rayInteractor.selectEntered.RemoveListener(OnSelectEntered);
            _rayInteractor.selectExited.RemoveListener(OnSelectExited);
        }

        #endregion

        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderLineVisual)]
        private void OnBeforeRenderLineVisual() => UpdateLine();

        #region Select

        private void OnSelectEntered(SelectEnterEventArgs selectEnterEvent)
        {
            _rayInteractor.GetLinePoints(ref _linePoints, out _);
            if (_rayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit))
            {
                _selectedCollider = raycastHit.collider;
                var baseInteractor = _rayInteractor as XRBaseInteractor;
                if (baseInteractor.interactionManager.TryGetInteractableForCollider(raycastHit.collider,
                        out var interactable, out var snapVolume))
                {
                    if (snapVolume != null)
                        _selectedOffset =
                            _selectedCollider.transform.InverseTransformPoint(
                                snapVolume.GetClosestPoint(raycastHit.point));
                    else if (interactable is XRGrabInteractable grabbable && grabbable.useDynamicAttach)
                        _selectedOffset = _selectedCollider.transform.InverseTransformPoint(raycastHit.point);
                    else _selectedOffset = Vector3.zero;
                }
                else _selectedOffset = _selectedCollider.transform.InverseTransformPoint(raycastHit.point);
            }
        }

        private void OnSelectExited(SelectExitEventArgs selectExitEvent)
        {
            _selectedCollider = null;
            _selectedOffset = Vector3.zero;
        }

        #endregion

        private void UpdateLine()
        {
            _rayInteractor.GetLinePoints(ref _linePoints, out _);
            if (!_rayInteractor.enabled || _linePoints.Length < 2)
            {
                ClearLine();
                return;
            }

            var startPoint = _linePoints[0] + (transform.forward * offSet);
            var endPoint = _linePoints[_linePoints.Length - 1];
            var color = idleColorGradient;
            var bend = false;

            var is3DHitClosest = false;
            var isUIHitCLosest = false;
            
            // Selecting
            if (_rayInteractor.hasSelection && enableOver3D)
            {
                color = selectColorGradient;
                endPoint = _selectedCollider.transform.TransformPoint(_selectedOffset);
                bend = bendEnabled;
            }
            // Hovering
            else if (_rayInteractor.TryGetCurrentRaycast(out var raycastHit, out _, out var uiRaycastHit,
                         out _, out var isUIHit))
            {
                is3DHitClosest = !isUIHit;
                isUIHitCLosest = !is3DHitClosest;

                if (isUIHit && uiRaycastHit.HasValue)
                    endPoint = uiRaycastHit.Value.worldPosition;
                else if (raycastHit.HasValue)
                    endPoint = raycastHit.Value.point;

                color = hoverColorGradient;

                var baseInteractor = _rayInteractor as XRBaseInteractor;
                if (baseInteractor.interactionManager.TryGetInteractableForCollider(raycastHit.Value.collider, out _,
                        out var snapVolume))
                {
                    if (snapVolume != null)
                    {
                        endPoint = snapVolume.GetClosestPoint(endPoint);
                        bend = bendEnabled;
                    }
                }
            }
            // No hit
            else if (hideWhenNoHit)
            {
                ClearLine();
                return;
            }
            
            if (!enableOver3D && is3DHitClosest)
            {
                ClearLine();
                return;
            }
            
            if (!enableOverUI && isUIHitCLosest)
            {
                ClearLine();
                return;
            }

            // Quality has changed
            if (_renderPoints.Length != lineQuality)
            {
                _renderPoints = new Vector3[lineQuality];
            }
            
            if (_renderPoints.Length > 2)
            {
                var controlPoint = GetControlPoint(startPoint, endPoint, bend);
                
                for (var i = 0; i < _renderPoints.Length; i++)
                {
                    var t = i / ((float) _renderPoints.Length - 1);
                    _renderPoints[i] = CalculateQuadBezierPoint(t, startPoint, controlPoint, endPoint);
                }
            }
            else
            {
                _renderPoints[0] = startPoint;
                _renderPoints[1] = endPoint;
            }

            _lineRenderer.colorGradient = color;
            _lineRenderer.widthMultiplier = lineWidthMultiplier;
            _lineRenderer.positionCount = _renderPoints.Length;
            _lineRenderer.SetPositions(_renderPoints);
        }

        private void ClearLine()
        {
            _lineRenderer.positionCount = 0;
            _lineRenderer.SetPositions(_emptyArray);
        }

        private Vector3 GetControlPoint(Vector3 startPoint, Vector3 endPoint, bool bend)
        {
            var controlPoint = (startPoint + endPoint) * .5f;
            var finalControlPoint = controlPoint;

            if (bend)
            {
                controlPoint += Vector3.up * bendYRatio;
                finalControlPoint = _previousControlPoint != Vector3.zero
                    ? Vector3.Lerp(_previousControlPoint, controlPoint, Time.deltaTime * bendSpeed)
                    : controlPoint;
            }

            _previousControlPoint = finalControlPoint;

            return finalControlPoint;
        }

        private static Vector3 CalculateQuadBezierPoint(float t, Vector3 point0, Vector3 point1, Vector3 point2)
        {
            var u = 1 - t;
            var t2 = t * t;
            var u2 = u * u;
            var r = u2 * point0;
            r += 2 * u * t * point1;
            r += t2 * point2;
            return r;
        }
    }
}