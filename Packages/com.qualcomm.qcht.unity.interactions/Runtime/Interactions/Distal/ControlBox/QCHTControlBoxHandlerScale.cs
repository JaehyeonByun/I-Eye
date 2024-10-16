// /******************************************************************************
//  * File: QCHTControlBoxHandlerScale.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Distal.ControlBox
{
    public class QCHTControlBoxHandlerScale : QCHTControlBoxHandler
    {
        private Vector3 _initialScale = Vector3.zero;
        private Vector3 _opposite;
        private Vector3 _originalPos;
        private Vector3 _originalGizmoPos;
        private Vector3 _diagDir;
        private float _minScale, _maxScale;

        public float MinScale
        {
            get => _minScale;
            set => _minScale = value;
        }

        public float MaxScale
        {
            get => _maxScale;
            set => _maxScale = value;
        }

        private Vector3 _pointerPressedPos;
        private float _pointerPressedDistance;

        public override event HandleEventHandler onHandleSelectEntered;
        public override event HandleEventHandler onHandleSelectExited;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            _interactor = args.interactorObject as XRRayInteractor;

            if (_interactor == null) return;

            _interactor.TryGetCurrentRaycast(out var raycastHit, out _, out _, out _, out _);

            if (raycastHit.HasValue)
            {
                _pointerPressedDistance = raycastHit.Value.distance;
                _pointerPressedPos = raycastHit.Value.point;
            }

            var handedness = XRInteractionGrid.GetInteractorHandedness(args.interactorObject);
            ref var select = ref handedness == XrHandedness.XR_HAND_LEFT ? ref _leftSelected : ref _rightSelected;
            select = true;

            var t = transform;
            var localHandleBoxPosition = t.localPosition;
            _initialScale = _controlBoxTransform.localScale;
            _originalGizmoPos = _controlBoxTransform.position;
            _originalPos = _gridTransform.TransformPoint(localHandleBoxPosition);
            _opposite = _gridTransform.TransformPoint(-localHandleBoxPosition);
            _diagDir = (_opposite - _originalPos).normalized;

            onHandleSelectEntered?.Invoke(this);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            _interactor = null;
            var handedness = XRInteractionGrid.GetInteractorHandedness(args.interactorObject);
            ref var select = ref handedness == XrHandedness.XR_HAND_LEFT ? ref _leftSelected : ref _rightSelected;
            select = false;

            onHandleSelectExited?.Invoke(this);
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (!IsSelected || _interactionGrid.IsControlBoxSelected)
                return;

            var initPoint = _pointerPressedPos;
            var t = _interactor.transform;
            var endPoint = t.position + t.forward.normalized * _pointerPressedDistance;

            var initialDist = Vector3.Dot(initPoint - _opposite, _diagDir);
            var currentDist = Vector3.Dot(endPoint - _opposite, _diagDir);
            var scaleFactor = 1 + (currentDist - initialDist) / initialDist;
            var targetScale = Vector3.one * Mathf.Clamp(0f, scaleFactor, scaleFactor);
            var finalScale = _initialScale.Multiply(targetScale);
            var dir = _controlBoxTransform.InverseTransformDirection(_originalGizmoPos - _opposite);
            var newPosition = _opposite + _controlBoxTransform.TransformDirection(dir.Multiply(targetScale));
            _controlBoxTransform.localScale = finalScale;
            _controlBoxTransform.position = newPosition;

            // Constraint scale on max and min scaleSettings
            if (_controlBoxTransform.localScale.x > _maxScale)
                _controlBoxTransform.localScale = _maxScale * Vector3.one;

            if (_controlBoxTransform.localScale.x < _minScale)
                _controlBoxTransform.localScale = _minScale * Vector3.one;
        }
    }
}