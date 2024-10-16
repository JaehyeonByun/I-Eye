// /******************************************************************************
//  * File: QCHTControlBoxHandler.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Distal.ControlBox
{
    public class QCHTControlBoxHandler : XRSimpleInteractable
    {
        [SerializeField] protected bool isCorner;

        protected ParticleSystem _particleSystem;

        protected QCHTControlBoxSettings.HandleState _normalState;
        protected QCHTControlBoxSettings.HandleState _hoverState;
        protected QCHTControlBoxSettings.HandleState _selectedState;
        protected ParticleSystem.MainModule _psMainModule;

        protected Transform _controlBoxTransform;

        protected Transform _gridTransform;
        protected XRInteractionGrid _interactionGrid;

        protected bool _leftSelected;
        protected bool _rightSelected;
        protected new bool IsSelected => _leftSelected || _rightSelected;

        protected bool _leftHover;
        protected bool _rightHover;
        protected new bool IsHovered => _leftHover || _rightHover;

        public delegate void HandleEventHandler(QCHTControlBoxHandler handler);

        public virtual event HandleEventHandler onHandleHoverEntered;
        public virtual event HandleEventHandler onHandleHoverExited;
        public virtual event HandleEventHandler onHandleSelectEntered;
        public virtual event HandleEventHandler onHandleSelectExited;

        protected XRRayInteractor _interactor = null;
        protected Camera _camera;

        protected override void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _psMainModule = _particleSystem.main;
            var parent = transform.parent;
            _controlBoxTransform = parent.parent;
            _gridTransform = parent;
            _interactionGrid = _gridTransform.GetComponent<XRInteractionGrid>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _interactionGrid.selectEntered.AddListener(OnGridSelectEntered);
            _interactionGrid.selectExited.AddListener(OnGridSelectExited);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _interactionGrid.selectEntered.RemoveListener(OnGridSelectEntered);
            _interactionGrid.selectExited.RemoveListener(OnGridSelectExited);
        }

        private void Update()
        {
            if (IsSelected)
            {
                _psMainModule.startColor = _selectedState?.color ?? _psMainModule.startColor;
                _psMainModule.startSize = _selectedState?.size ?? _psMainModule.startSize;
                return;
            }

            if (IsHovered)
            {
                _psMainModule.startColor = _hoverState?.color ?? _psMainModule.startColor;
                _psMainModule.startSize = _hoverState?.size ?? _psMainModule.startSize;
                return;
            }

            _psMainModule.startColor = _normalState?.color ?? _psMainModule.startColor;
            _psMainModule.startSize = _normalState?.size ?? _psMainModule.startSize;
        }

        protected void OnGridSelectEntered(SelectEnterEventArgs args)
        {
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
        }

        protected void OnGridSelectExited(SelectExitEventArgs args)
        {
            foreach (var col in colliders)
            {
                col.isTrigger = false;
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            onHandleSelectEntered?.Invoke(this);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            onHandleSelectExited?.Invoke(this);
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);

            var handedness = XRInteractionGrid.GetInteractorHandedness(args.interactorObject);
            ref var hover = ref handedness == XrHandedness.XR_HAND_LEFT ? ref _leftHover : ref _rightHover;
            hover = true;

            onHandleHoverEntered?.Invoke(this);
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            
            var handedness = XRInteractionGrid.GetInteractorHandedness(args.interactorObject);
            ref var hover = ref handedness == XrHandedness.XR_HAND_LEFT ? ref _leftHover : ref _rightHover;
            hover = false;

            onHandleHoverExited?.Invoke(this);
        }

        public void ToggleVisibility(bool visible)
        {
            if (visible)
            {
                float particleSize;

                if (IsSelected)
                    particleSize = _selectedState.size;
                else if (IsHovered)
                    particleSize = _hoverState.size;
                else
                    particleSize = _normalState.size;

                _psMainModule.startSize = particleSize;
                _particleSystem.Play();
            }
            else
            {
                _particleSystem.Stop();
            }
        }

        public void SetStates(QCHTControlBoxSettings.HandleState normal, QCHTControlBoxSettings.HandleState hover,
            QCHTControlBoxSettings.HandleState selected)
        {
            _normalState = normal;
            _hoverState = hover;
            _selectedState = selected;
        }

        public void ToggleActivation(bool on)
        {
            foreach (var col in colliders)
                col.enabled = on;
        }

        public bool IsHandleFacingUser()
        {
            _camera = _camera ? _camera : Camera.main;
            if (_camera == null) return false;
            var t = transform.position;
            var h = _camera.transform.position;
            var ht = (t - h).normalized;
            if (Vector3.Dot(ht, transform.forward) < 0) return true;
            if (Vector3.Dot(ht, transform.right) < 0) return true;
            if (isCorner && Vector3.Dot(ht, transform.up) < 0) return true;
            return false;
        }
    }
}