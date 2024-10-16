// /******************************************************************************
//  * File: XRReticleInteractorSnapping.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Distal
{
    public class XRReticleInteractorSnapping : MonoBehaviour
    {
        [FormerlySerializedAs("_prefabReticle")] [SerializeField]
        private GameObject prefabReticle;
        [FormerlySerializedAs("_smoothSpeed")] [SerializeField]
        private float smoothSpeed = 0.01f;

        private GameObject _reticle = null;
        private bool _hasToSnap = false;
        private bool _hasToCollide = false;

        private XRBaseInteractor _baseInteractor;
        private Vector3 _targetReticlePosition, _targetReticleNormal;

        private Vector3 _raycastPos = Vector3.zero;
        private Vector3 _raycastNormal = Vector3.zero;
        private RaycastHit _raycastHit3D;

        private void Awake()
        {
            _baseInteractor = GetComponent<XRBaseInteractor>();
            InstantiateReticle();
        }

        private void SetReticleActive(bool activate)
        {
            if (_reticle != null)
                _reticle.SetActive(activate);
        }

        private void InstantiateReticle()
        {
            if (_reticle)
                Destroy(_reticle);
            _reticle = Instantiate(prefabReticle);
        }

        private void Update()
        {
            if (_reticle != null)
            {
                _hasToCollide = CheckColliding();
                _hasToSnap = CheckSnapping();
                if (!_hasToSnap && !_hasToCollide)
                {
                    SetReticleActive(false);
                }
                else
                {
                    SetReticleActive(true);
                    UpdateReticlePositionAndRotation(_targetReticlePosition, _targetReticleNormal);
                }
            }
        }

        private void UpdateReticlePositionAndRotation(Vector3 targetPosition, Vector3 targetNormal)
        {
            _reticle.transform.position = targetPosition;
            _reticle.transform.rotation = Quaternion.FromToRotation(Vector3.up, targetNormal);
        }

        private bool CheckColliding()
        {
            if (((XRRayInteractor) _baseInteractor).TryGetCurrentRaycast(
                    out var raycastHit,
                    out var raycastHitIndex,
                    out var uiRaycastHit,
                    out var uiRaycastHitIndex,
                    out var uiHit))
            {
                if (uiHit && uiRaycastHit.HasValue)
                {
                    var hit = uiRaycastHit.Value;
                    _raycastPos = hit.worldPosition;
                    _raycastNormal = hit.worldNormal;
                }
                else if (raycastHit.HasValue)
                {
                    var hit = raycastHit.Value;
                    _raycastPos = hit.point;
                    _raycastNormal = hit.normal;
                }

                if (!_hasToSnap)
                    SetTarget(_raycastPos, _raycastNormal);
                return true;
            }
            else
                return false;
        }

        private bool CheckSnapping()
        {
            if (_baseInteractor.hasSelection) return false;

            var raycastHit = new RaycastHit();
            Vector3[] linePoints = null;
            ((XRRayInteractor) _baseInteractor).GetLinePoints(ref linePoints, out _);
            ((XRRayInteractor) _baseInteractor).TryGetCurrent3DRaycastHit(out raycastHit);

            if (_baseInteractor.interactionManager.TryGetInteractableForCollider(raycastHit.collider,
                    out var interactable,
                    out var snapVolume))
            {
                if (snapVolume != null)
                {
                    _raycastPos =
                        snapVolume.GetClosestPoint(raycastHit.point);
                    _raycastNormal = raycastHit.normal;
                }

                else
                {
                    _raycastPos = raycastHit.point;
                    _raycastNormal = raycastHit.normal;
                }

                SetTarget(_raycastPos, _raycastNormal);
                return true;
            }
            else
                return false;
        }

        private void SetTarget(Vector3 targetPosition, Vector3 targetNormal)
        {
            var velocity = Vector3.zero;
            _targetReticlePosition =
                Vector3.SmoothDamp(_targetReticlePosition, targetPosition, ref velocity, smoothSpeed);
            _targetReticleNormal = Vector3.SmoothDamp(_targetReticleNormal, targetNormal, ref velocity, smoothSpeed);
        }

        private void OnEnable()
        {
            SetReticleActive(false);
        }

        private void OnDisable()
        {
            SetReticleActive(false);
        }
    }
}