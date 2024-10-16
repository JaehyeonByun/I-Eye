// /******************************************************************************
//  * File: XRReticleInteractor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Core;
using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace QCHT.Interactions.Distal
{
    [DisallowMultipleComponent]
    public class XRReticleInteractor : XRRayInteractorStateIndicator<XRReticleInteractor.ReticleState>
    {
        [Serializable]
        public struct ReticleState
        {
            public float Scale;
            public Color Color;

            public static bool operator <(ReticleState a, ReticleState b) => a.Scale < b.Scale;

            public static bool operator >(ReticleState a, ReticleState b) => a.Scale > b.Scale;

            public static ReticleState Lerp(ReticleState a, ReticleState b, float t) =>
                new()
                {
                    Scale = Mathf.Lerp(a.Scale, b.Scale, t),
                    Color = Color.Lerp(a.Color, b.Color, t)
                };
        }

        [Space]
        
        [SerializeField] private float smoothSpeed = 0.01f;

        public float SmoothSpeed
        {
            get => smoothSpeed;
            set => smoothSpeed = value;
        }
        
        [SerializeField] private bool scaleOverDistance;

        public bool ScaleOverDistance
        {
            get => scaleOverDistance;
            set => scaleOverDistance = value;
        }

        [SerializeField] private bool disableWhenSnapping = true;

        public bool DisableWHenSnapping
        {
            get => disableWhenSnapping;
            set => disableWhenSnapping = value;
        }

        private Camera _camera;
        
        private Vector3 _targetPosition = Vector3.zero;
        private Vector3 _targetNormal = Vector3.zero;
        
        protected override bool ShouldRenderIndicator()
        {
            if (!base.ShouldRenderIndicator())
                return false;

            var is3DClosestTarget = false;
            var isUIClosestTarget = false;
            
            if (!_rayInteractor.TryGetCurrentRaycast(
                    out var raycastHit,
                    out _,
                    out var uiRaycastHit,
                    out _,
                    out var uiHit))
            {
                return false;
            }

            is3DClosestTarget = !uiHit;
            isUIClosestTarget = !is3DClosestTarget;
            
            if (isUIClosestTarget && uiRaycastHit.HasValue)
            {
                _targetPosition = uiRaycastHit.Value.worldPosition;
                _targetNormal = uiRaycastHit.Value.worldNormal;
            }
            else if (is3DClosestTarget && raycastHit.HasValue)
            {
                _targetPosition = raycastHit.Value.point;
                _targetNormal = raycastHit.Value.normal;

                if (_rayInteractor.interactionManager.TryGetInteractableForCollider(raycastHit.Value.collider,
                        out _,
                        out var snapVolume))
                {
                    if (snapVolume != null)
                    {
                        if (disableWhenSnapping)
                            return false;
                        
                        _targetPosition = snapVolume.GetClosestPoint(raycastHit.Value.point);
                    }
                }
            }

            if (!enableOver3D && is3DClosestTarget)
                return false;
            if (!enableOverUI && isUIClosestTarget)
                return false;

            return true;
        }

        protected override void UpdateIndicator()
        {
            _indicator.transform.position = _targetPosition;
            _indicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, _targetNormal);
        }
        
        protected override void ApplyIndicatorState(ReticleState state)
        {
            var distFactor = 1f;

            if (scaleOverDistance)
            {
                if (_camera == null)
                {
                    _camera = XROriginUtility.GetOriginCamera();
                }

                if (_camera != null)
                {
                    distFactor = Vector3.Distance(_camera.transform.position, _indicator.transform.position);
                }
            }
            
            _indicator.transform.localScale = Vector3.one * state.Scale * distFactor;
            _indicator.GetComponentInChildren<Image>().color = state.Color;
        }
        
        protected override ReticleState GetHoverState()
        {
            var pinchStrength = 0f;
            
            if (_subsystem != null)
            {
                var hand = _handedness == XrHandedness.XR_HAND_LEFT ? _subsystem.LeftHand : _subsystem.RightHand;
                pinchStrength = hand.GetFingerPinching(XrFinger.XR_HAND_FINGER_INDEX);
            }
            
            return ReticleState.Lerp(MaxHoverState, MinHoverState, pinchStrength);
        }
    }
}