// /******************************************************************************
//  * File: XRDistalIndicator.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Core;
using QCHT.Interactions.Extensions;
using UnityEngine;

namespace QCHT.Interactions.Distal
{
    [DisallowMultipleComponent]
    public class XRDistalIndicator : XRRayInteractorStateIndicator<XRDistalIndicator.DistalIndicatorState>
    {
        [Serializable]
        public struct DistalIndicatorState
        {
            public float Scale;
            public Color Color;

            public static bool operator <(DistalIndicatorState a, DistalIndicatorState b) => a.Scale < b.Scale;

            public static bool operator >(DistalIndicatorState a, DistalIndicatorState b) => a.Scale > b.Scale;

            public static DistalIndicatorState Lerp(DistalIndicatorState a, DistalIndicatorState b, float t) =>
                new()
                {
                    Scale = Mathf.Lerp(a.Scale, b.Scale, t),
                    Color = Color.Lerp(a.Color, b.Color, t)
                };
        }

        [Space]
        
        [SerializeField] 
        private float offset = 0.08f;

        public float Offset
        {
            get => offset;
            set => offset = value;
        }
        
        private ParticleSystem _particles;

        protected new void Awake()
        {
            base.Awake();

            if (_indicator != null)
            {
                _particles = GetComponentInChildren<ParticleSystem>();
            }
        } 
        
        protected override void UpdateIndicator()
        {
            if (_indicator != null)
            {
                var thisTransform = transform;
                var t = _indicator.transform;
                t.rotation = thisTransform.rotation;
                t.position = thisTransform.position + thisTransform.rotation * new Vector3(0, 0, offset);
            }
        }
        
        protected override void ApplyIndicatorState(DistalIndicatorState state)
        {
            _indicator.transform.localScale = Vector3.one * state.Scale;
            _indicator.GetComponentInChildren<MeshRenderer>().material.color = state.Color;

            if (_particles != null)
            {
                var mainParticle = _particles.main;
                mainParticle.startColor = state.Color;
            }
        }

        protected override DistalIndicatorState GetHoverState()
        {
            var pinchStrength = 0f;
            if (_subsystem != null)
            {
                var hand = _handedness == XrHandedness.XR_HAND_LEFT ? _subsystem.LeftHand : _subsystem.RightHand;
                pinchStrength = hand.GetFingerPinching(XrFinger.XR_HAND_FINGER_INDEX);
            }
            
            // remap value from 0f - 1f to minHoverScale - maxHoverScale
            if (MinHoverState > MaxHoverState)
                (MinHoverState, MaxHoverState) = (MaxHoverState, MinHoverState);

            return DistalIndicatorState.Lerp(MaxHoverState, MinHoverState, pinchStrength);
        }
    }
}