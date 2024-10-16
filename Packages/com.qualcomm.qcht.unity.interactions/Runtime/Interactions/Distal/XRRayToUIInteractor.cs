// /******************************************************************************
//  * File: XRRayToUIInteractor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Distal
{
    [AddComponentMenu("XR/XR Ray Interactor To UI Interactor (QCHTPointerEventBased)", 11)]
    [RequireComponent(typeof(XRRayInteractor))]
    public sealed class XRRayToUIInteractor : MonoBehaviour
    {
        internal class FakeBaseRaycaster : BaseRaycaster
        {
            private Camera _camera;
            public override Camera eventCamera => _camera;
            public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) { }
            
            protected override void Awake() {
                base.Awake();
                _camera = Camera.main;
            }
        }
        
        [SerializeField] private bool isLeft = true;

        public bool IsLeft
        {
            get => isLeft;
            set => isLeft = value;
        }
        
        private FakeBaseRaycaster _fakeBaseRaycast;
        private XRPointerEventData _pointerEventData;
        private XRRayInteractor _rayInteractor;

        private void Awake() {
            _fakeBaseRaycast = gameObject.AddComponent<FakeBaseRaycaster>();
            _pointerEventData = new XRPointerEventData(null) {
                isLeftHand = isLeft,
            };
            _rayInteractor = GetComponent<XRRayInteractor>();
        }

        private void OnEnable() {
            _rayInteractor.hoverEntered.AddListener(OnHoverEntered);
            _rayInteractor.hoverExited.AddListener(OnHoverExited);
            _rayInteractor.selectEntered.AddListener(OnSelectEntered);
            _rayInteractor.selectExited.AddListener(OnSelectExited);
        }
        
        private void OnDisable() {
            _rayInteractor.hoverEntered.RemoveListener(OnHoverEntered);
            _rayInteractor.hoverExited.RemoveListener(OnHoverExited);
            _rayInteractor.selectEntered.RemoveListener(OnSelectEntered);
            _rayInteractor.selectExited.RemoveListener(OnSelectExited);
        }

        private void Update() {
            if (_rayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit)) {
                var origin = _rayInteractor.rayOriginTransform.position;
                var dir = Quaternion.Inverse(_rayInteractor.transform.rotation) * (raycastHit.point - origin).normalized;
                _pointerEventData.worldSpaceRay = new Ray(origin, dir);
                var pointerCurrentRaycast = _pointerEventData.pointerCurrentRaycast;
                pointerCurrentRaycast.distance = raycastHit.distance;
                pointerCurrentRaycast.worldPosition = raycastHit.point;
                _pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
            }
            
            if (_pointerEventData.pointerEnter) {
                var pointerMove = ExecuteEvents.GetEventHandler<IPointerMoveHandler>(_pointerEventData.pointerEnter);
                if (pointerMove)
                    ExecuteEvents.Execute(pointerMove, _pointerEventData, ExecuteEvents.pointerMoveHandler);
            }

            if (_pointerEventData.dragging) {
                _pointerEventData.dragPosition = _rayInteractor.rayOriginTransform.position + _rayInteractor.transform.rotation * _pointerEventData.worldSpaceRay.direction * _pointerEventData.pointerPressRaycast.distance;
                var screenPosition = _pointerEventData.enterEventCamera.WorldToScreenPoint(_pointerEventData.dragPosition, Camera.MonoOrStereoscopicEye.Mono);
                var pointerCurrentRaycast = _pointerEventData.pointerCurrentRaycast;
                pointerCurrentRaycast.screenPosition = screenPosition;
                _pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
                _pointerEventData.position = screenPosition;
                var pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(_pointerEventData.pointerDrag);
                if (pointerDrag)
                    ExecuteEvents.Execute(pointerDrag, _pointerEventData, ExecuteEvents.dragHandler);
            }
        }

        private void OnHoverEntered(HoverEnterEventArgs args) {
            var target = args.interactableObject as MonoBehaviour;
            if (target == null) return;
            var pointerEnter = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(target.gameObject);
            if (pointerEnter) {
                ExecuteEvents.Execute(pointerEnter, _pointerEventData, ExecuteEvents.pointerEnterHandler);
                _pointerEventData.pointerEnter = pointerEnter;
                var pointerCurrentRaycast = _pointerEventData.pointerCurrentRaycast;
                pointerCurrentRaycast.module = _fakeBaseRaycast;
                _pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
            }
        }

        private void OnHoverExited(HoverExitEventArgs args) {
            ExecuteEvents.Execute(_pointerEventData.pointerEnter, _pointerEventData, ExecuteEvents.pointerExitHandler);
            _pointerEventData.pointerEnter = null;
        }

        private void OnSelectEntered(SelectEnterEventArgs args) {
            var target = args.interactableObject as MonoBehaviour;
            if (target == null) return;
            
            if (_rayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit)) {
                var origin = _rayInteractor.rayOriginTransform.position;
                var dir = Quaternion.Inverse(_rayInteractor.transform.rotation) * (raycastHit.point - origin).normalized;
                _pointerEventData.worldSpaceRay = new Ray(origin, dir);
                var pointerPressRaycast = _pointerEventData.pointerPressRaycast;
                pointerPressRaycast.module = _fakeBaseRaycast;
                pointerPressRaycast.distance = raycastHit.distance;
                pointerPressRaycast.worldPosition = raycastHit.point;
                _pointerEventData.pointerPressRaycast = _pointerEventData.pointerCurrentRaycast = pointerPressRaycast;
                var screenPosition = _pointerEventData.pressEventCamera.WorldToScreenPoint(raycastHit.point, Camera.MonoOrStereoscopicEye.Mono);
                pointerPressRaycast.screenPosition = screenPosition;
                _pointerEventData.position = screenPosition;
            }
            
            var pointerPress = ExecuteEvents.GetEventHandler<IPointerDownHandler>(target.gameObject);
            if (pointerPress) {
                ExecuteEvents.Execute(pointerPress, _pointerEventData, ExecuteEvents.pointerDownHandler);
                _pointerEventData.pointerPress = pointerPress;
            }
            
            var pointerClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target.gameObject);
            if (pointerClick)
            {
                ExecuteEvents.Execute(pointerClick, _pointerEventData, ExecuteEvents.pointerClickHandler);
                _pointerEventData.pointerClick = pointerClick;
            }

            var pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(target.gameObject);
            if (pointerDrag) {
                _pointerEventData.pointerDrag = pointerDrag;
                _pointerEventData.dragging = true;
                ExecuteEvents.Execute(pointerDrag, _pointerEventData, ExecuteEvents.initializePotentialDrag);
                ExecuteEvents.Execute(pointerDrag, _pointerEventData, ExecuteEvents.beginDragHandler);
            }
        }

        private void OnSelectExited(SelectExitEventArgs args) {
            if (_pointerEventData.dragging) {
                ExecuteEvents.Execute(_pointerEventData.pointerDrag, _pointerEventData, ExecuteEvents.endDragHandler);
                _pointerEventData.dragging = false;
            }
            
            if (_pointerEventData.pointerPress) {
                ExecuteEvents.Execute(_pointerEventData.pointerPress, _pointerEventData, ExecuteEvents.pointerUpHandler);
                _pointerEventData.pointerPress = null;
            }

            _pointerEventData.pointerClick = null;
        }
    }
}