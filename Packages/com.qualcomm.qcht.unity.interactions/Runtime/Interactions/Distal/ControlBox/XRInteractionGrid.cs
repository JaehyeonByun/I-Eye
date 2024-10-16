// /******************************************************************************
//  * File: XRInteractionGrid.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Core;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Distal.ControlBox
{
    public class XRInteractionGrid : XRGrabInteractable
    {
        private const string LightLeftKeyword = "LIGHT_L";
        private const string LightRightKeyword = "LIGHT_R";
        private const string Selected = "SELECTED";

        private static readonly int s_lightPosLeft = Shader.PropertyToID("_LightPosL");
        private static readonly int s_lightPosRight = Shader.PropertyToID("_LightPosR");
        private static readonly int s_lightColor = Shader.PropertyToID("_LightColor");

        private Material _material;

        private QCHTControlBoxHandler[] _handlers;

        private Vector3 _localLightPosLeft;
        private Vector3 _localLightPosRight;

        private XRSimpleInteractable _currentHandle;
        private bool IsHandleHovered => _currentHandle != null;

        private Transform _controlBoxTransform;

        private bool _leftSelected;
        private bool _rightSelected;
        public bool IsControlBoxSelected => _leftSelected | _rightSelected;

        private bool _leftHover;
        private bool _rightHover;
        private bool IsHover => _leftHover || _rightHover;

        private Vector3 _initialControlBoxPosition;
        private Vector3 _initialLocalPositionDeltaLeft;
        private Vector3 _initialLocalPositionDeltaRight;
        private Vector3 _initialControlBoxScale;
        private Vector3 _offsetToControlBoxCenter;

        private XRRayInteractor _rayRight;
        private XRRayInteractor _rayLeft;

        public event Action onHandled;
        public event Action onReleased;

        private QCHTControlBoxSettings _settings;

        public QCHTControlBoxSettings Settings
        {
            private get => _settings;
            set
            {
                _settings = value;
                foreach (var handler in _handlers)
                {
                    handler.SetStates(Settings.NormalState, Settings.HoverState, Settings.SelectedState);

                    if (handler is QCHTControlBoxHandlerScale scaleHandler)
                    {
                        scaleHandler.MinScale = Settings.MinScale;
                        scaleHandler.MaxScale = Settings.MaxScale;
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _material = GetComponent<Renderer>().material;
            _handlers = GetComponentsInChildren<QCHTControlBoxHandler>();

            _controlBoxTransform = transform.parent;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (var handle in _handlers)
            {
                handle.onHandleSelectEntered += OnHandleSelectEntered;
                handle.onHandleSelectExited += OnHandleSelectedExited;
                handle.onHandleHoverEntered += OnHandleHoveredEntered;
                handle.onHandleHoverExited += OnHandleHoveredExited;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            foreach (var handle in _handlers)
            {
                handle.onHandleSelectEntered -= OnHandleSelectEntered;
                handle.onHandleSelectExited -= OnHandleSelectedExited;
                handle.onHandleHoverEntered -= OnHandleHoveredEntered;
                handle.onHandleHoverExited -= OnHandleHoveredExited;
            }
        }

        private void Update()
        {
            if (_settings == null)
                return;

            UpdateLayer();
            UpdateHandles();
            UpdateLightColor();
            UpdateLightPositionInMaterial();
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);

            var isLeft = GetInteractorHandedness(args.interactorObject) == XrHandedness.XR_HAND_LEFT;
            var ray = args.interactorObject as XRRayInteractor;

            if (isLeft)
                _rayLeft = ray;
            else
                _rayRight = ray;

            ref var hover = ref isLeft ? ref _leftHover : ref _rightHover;
            hover = true;

            AddLight(isLeft);
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);

            var isLeft = GetInteractorHandedness(args.interactorObject) == XrHandedness.XR_HAND_LEFT;

            if (isLeft)
                _rayLeft = null;
            else
                _rayRight = null;

            ref var hover = ref isLeft ? ref _leftHover : ref _rightHover;
            hover = false;

            RemoveLight(isLeft);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }

            _controlBoxTransform.parent = transform;

            var handedness = GetInteractorHandedness(args.interactorObject);
            ref var select = ref handedness == XrHandedness.XR_HAND_LEFT ? ref _leftSelected : ref _rightSelected;
            select = true;

            Select();
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            foreach (var col in colliders)
            {
                col.isTrigger = false;
            }

            base.OnSelectExited(args);
            _controlBoxTransform.parent = null;
            transform.parent = _controlBoxTransform;

            var handedness = GetInteractorHandedness(args.interactorObject);
            ref var select = ref handedness == XrHandedness.XR_HAND_LEFT ? ref _leftSelected : ref _rightSelected;
            select = false;

            Deselect();
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            SetLightPosition();
        }

        private void OnHandleSelectEntered(XRSimpleInteractable handle)
        {
            Select();
        }

        private void OnHandleSelectedExited(XRSimpleInteractable handle)
        {
            Deselect();
        }

        private void OnHandleHoveredEntered(XRSimpleInteractable handle)
        {
            _currentHandle = handle;
        }

        private void OnHandleHoveredExited(XRSimpleInteractable handle)
        {
            _currentHandle = null;
        }

        private void Select()
        {
            _material.EnableKeyword(Selected);
            onHandled?.Invoke();
        }

        private void Deselect()
        {
            _material.DisableKeyword(Selected);
            onReleased?.Invoke();
        }

        private void AddLight(bool isLeft)
        {
            _material.EnableKeyword(isLeft ? LightLeftKeyword : LightRightKeyword);
        }

        private void RemoveLight(bool isLeft)
        {
            _material.DisableKeyword(isLeft ? LightLeftKeyword : LightRightKeyword);
        }

        private void UpdateLightColor()
        {
            var color = IsHover ? Settings.HoverColor : Color.white;
            color = IsControlBoxSelected ? Settings.SelectedColor : color;
            _material.SetColor(s_lightColor, color);
        }

        private void SetLightPosition()
        {
            if (_rayLeft)
            {
                _rayLeft.TryGetCurrentRaycast(out var raycastHit, out var hitIndex, out var uiRaycastHit,
                    out var uiRaycastHitIndex,
                    out var isUIHitClosest);

                if (raycastHit.HasValue && IsHover)
                {
                    _localLightPosLeft = _controlBoxTransform.InverseTransformPoint(raycastHit.Value.point);
                }
            }

            if (_rayRight)
            {
                _rayRight.TryGetCurrentRaycast(out var raycastHit, out var hitIndex, out var uiRaycastHit,
                    out var uiRaycastHitIndex,
                    out var isUIHitClosest);

                if (raycastHit.HasValue && IsHover)
                {
                    _localLightPosRight = _controlBoxTransform.InverseTransformPoint(raycastHit.Value.point);
                }
            }
        }

        private void UpdateLayer()
        {
            if (Settings.Layer == gameObject.layer) return;
            gameObject.layer = Settings.Layer;
            foreach (var handler in _handlers)
                handler.gameObject.layer = Settings.Layer;
        }

        private void UpdateHandles()
        {
            var type = Settings.DisplayType;
            foreach (var handler in _handlers)
            {
                if (type == QCHTControlBoxSettings.ControlBoxDisplayType.Never)
                {
                    handler.ToggleActivation(false);
                    handler.ToggleVisibility(false);
                    continue;
                }

                if (IsHandleHovered)
                {
                    // Interacting show only selected
                    var eq = _currentHandle.Equals(handler);
                    handler.ToggleActivation(eq);
                    handler.ToggleVisibility(eq);
                }
                else if (IsControlBoxSelected)
                {
                    handler.ToggleActivation(false);
                    handler.ToggleVisibility(false);
                }
                else
                {
                    handler.ToggleActivation(type == QCHTControlBoxSettings.ControlBoxDisplayType.Always || IsHover);
                    handler.ToggleVisibility(handler.IsHandleFacingUser() &&
                                             (type == QCHTControlBoxSettings.ControlBoxDisplayType.Always || IsHover));
                }
            }
        }

        private void UpdateLightPositionInMaterial()
        {
            if (_material.IsKeywordEnabled(LightLeftKeyword))
            {
                var pos = _controlBoxTransform.TransformPoint(_localLightPosLeft);
                _material.SetVector(s_lightPosLeft, pos);
            }

            if (_material.IsKeywordEnabled(LightRightKeyword))
            {
                var pos = _controlBoxTransform.TransformPoint(_localLightPosRight);
                _material.SetVector(s_lightPosRight, pos);
            }
        }

        internal static XrHandedness GetInteractorHandedness(IXRInteractor xrInteractor)
        {
            if (xrInteractor is XRBaseControllerInteractor interactor)
            {
                if (interactor.xrController != null &&
                    interactor.xrController.TryGetComponent<IHandedness>(out var hand))
                {
                    return hand.Handedness;
                }
            }

            return XrHandedness.XR_HAND_LEFT;
        }
    }
}