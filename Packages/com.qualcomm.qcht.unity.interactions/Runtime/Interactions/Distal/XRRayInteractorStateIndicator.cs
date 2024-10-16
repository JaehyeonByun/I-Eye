// /******************************************************************************
//  * File: XRRayInteractorStateIndicator.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using QCHT.Interactions.Core;
using QCHT.Interactions.Hands;

namespace QCHT.Interactions.Distal
{
    [RequireComponent(typeof(XRRayInteractor))]
    public abstract class XRRayInteractorStateIndicator<T> : MonoBehaviour
    {
        protected XRHandTrackingSubsystem _subsystem;

        [SerializeField] protected T minHoverState;
        [SerializeField] protected T maxHoverState;
        [SerializeField] protected T selectState;

        protected XrHandedness _handedness;
        protected XRRayInteractor _rayInteractor;

        public T MinHoverState
        {
            get => minHoverState;
            set => minHoverState = value;
        }

        public T MaxHoverState
        {
            get => maxHoverState;
            set => maxHoverState = value;
        }

        public T SelectState
        {
            get => selectState;
            set => selectState = value;
        }

        [Tooltip("Indicator prefab")] [SerializeField]
        private GameObject prefab;

        public GameObject Prefab
        {
            get => prefab;
            set
            {
                if (ReferenceEquals(value, prefab)) 
                    return;

                prefab = value;
                
                DestroyIndicator();
                InstantiateIndicator();
            }
        }

        [SerializeField] protected bool enableOverUI = true;

        public bool EnableOverUI
        {
            get => enableOverUI;
            set => enableOverUI = value;
        }

        [SerializeField] protected bool enableOver3D = true;

        public bool EnableOver3D
        {
            get => enableOver3D;
            set => enableOver3D = value;
        }

        protected GameObject _indicator;

        private const string kTriggerName = "trigger";
        private static readonly int s_trigger = Animator.StringToHash(kTriggerName);
        private Animator _animator;

        protected void Awake()
        {
            _handedness = GetComponentInParent<IHandedness>()?.Handedness ?? XrHandedness.XR_HAND_LEFT;
            _rayInteractor = GetComponent<XRRayInteractor>();
        }

        protected virtual void OnEnable()
        {
            if (_indicator == null)
            {
                InstantiateIndicator();
            }
            
            if (_indicator != null)
            {
                _animator = _indicator.GetComponent<Animator>();
            }
            
            Application.onBeforeRender += OnBeforeRender;
        }

        protected virtual void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
            SetIndicatorActive(false);
        }

        protected void OnDestroy() => DestroyIndicator();

        private void InstantiateIndicator()
        {
            if (prefab != null)
            {
                _indicator = Instantiate(prefab);
            }
        }
        
        private void DestroyIndicator()
        {
            if (_indicator != null)
            {
                Destroy(_indicator);
                _indicator = null;
            }
        }
        
        protected void SetIndicatorActive(bool activate)
        {
            if (_indicator != null)
            {
                _indicator.SetActive(activate);
            }
        }
        
        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderLineVisual)]
        private void OnBeforeRender()
        {
            if (_indicator == null) 
                return;
            
            _subsystem ??= XRHandTrackingSubsystem.GetSubsystemInManager();
            
            if (!ShouldRenderIndicator())
            {
                SetIndicatorActive(false);
                return;
            }
            
            SetIndicatorActive(true);
            
            // Update indicator
            UpdateIndicator();
            
            // Apply indicator state
            if (HasSelection())
            {
                if (_animator != null)
                {
                    _animator.SetBool(s_trigger, true);
                }

                ApplyIndicatorState(GetSelectedState());
            }
            else
            {
                if (_animator != null)
                {
                    _animator.SetBool(s_trigger, false);
                }

                ApplyIndicatorState(GetHoverState());
            }
        }

        private bool HasSelection() => _rayInteractor.hasSelection || _rayInteractor.TryGetUIModel(out var model) && model.select;

        protected virtual bool ShouldRenderIndicator() => enableOver3D || (enableOverUI && _rayInteractor.IsOverUIGameObject());
        
        protected abstract void ApplyIndicatorState(T state);

        protected abstract void UpdateIndicator();
        
        protected virtual T GetSelectedState() => selectState;

        protected virtual T GetHoverState() => maxHoverState;
    }
}