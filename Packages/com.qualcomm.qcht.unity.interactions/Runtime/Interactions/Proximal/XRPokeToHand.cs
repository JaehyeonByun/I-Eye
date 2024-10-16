// /******************************************************************************
//  * File: XRPokeToHand.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Proximal
{
    [RequireComponent(typeof(XRPokeInteractor))]
    public class XRPokeToHand : MonoBehaviour
    {
        [Tooltip("Changes poke depth when UI is selected.")] 
        [SerializeField] private float depthSelected = 1f;

        private float _depthDefault;
        private XrHandedness _handedness;
        private XRPokeInteractor _pokeInteractor;
        private XRHandTrackingManager _handTrackingManager;

        private bool _wasPoking;
        
        protected void Awake()
        {
            _handedness = GetComponentInParent<IHandedness>()?.Handedness ?? XrHandedness.XR_HAND_LEFT;
            _pokeInteractor = GetComponent<XRPokeInteractor>();
            _depthDefault = _pokeInteractor.pokeDepth;
        }

        protected void OnEnable() => FindHandTrackingManager();

        protected void Update()
        {
            if (_handTrackingManager == null)
            {
                return;
            }

            bool isPoking = false;
            
            if (_pokeInteractor.enableUIInteraction)
            {
                if (_pokeInteractor.TryGetUIModel(out var model))
                {
                    if (model.select && model.currentRaycast.gameObject)
                    {
                        var fwd = model.currentRaycast.gameObject.transform.forward;
                        var pos = model.currentRaycast.worldPosition;
                        var dir = (transform.position - pos).normalized;
                        _pokeInteractor.pokeDepth = Vector3.Dot(dir, fwd) > 0f ? depthSelected : _depthDefault;
                        _handTrackingManager.TrySetPoking(_handedness, pos);
                        isPoking = true;
                    }
                }
            }

            if (!isPoking && _wasPoking)
            {
                _handTrackingManager.TrySetPoking(_handedness, null);
                _pokeInteractor.pokeDepth = _depthDefault;
            }

            _wasPoking = isPoking;
        }

        private void FindHandTrackingManager() => _handTrackingManager =
            _handTrackingManager ? _handTrackingManager : FindObjectOfType<XRHandTrackingManager>();
    }
}