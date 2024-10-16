// /******************************************************************************
//  * File: QCHTControlBoxHandlerRotate.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Distal.ControlBox
{
    public class QCHTControlBoxHandlerRotate : QCHTControlBoxHandler
    {
        [SerializeField] private Axis axis;
        
        private Quaternion _originalGizmoRot;
        private Vector3 _pointerPressedPos;
        private float _pointerPressedDistance;
        private Vector3 _normalPlane;

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
                var t = _interactor.transform;
                _pointerPressedPos = t.position + t.forward.normalized * _pointerPressedDistance;   
                _normalPlane = GetPlaneAxis();

            }
            
            var handedness = XRInteractionGrid.GetInteractorHandedness(args.interactorObject);
            ref var select = ref handedness == XrHandedness.XR_HAND_LEFT ? ref _leftSelected : ref _rightSelected;
            select = true;

            _originalGizmoRot = _controlBoxTransform.rotation;

            onHandleSelectEntered?.Invoke(this);
        }

        private Vector3 GetPlaneAxis()
        {
            switch (axis)
            {
                case Axis.Right:
                    return _controlBoxTransform.transform.right;
                case Axis.Up:
                    return _normalPlane = _controlBoxTransform.transform.up;
                case Axis.Forward:
                    return _normalPlane = _controlBoxTransform.transform.forward;
            }

            return _controlBoxTransform.transform.forward;
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

            var t = _interactor.transform;
            var controlBoxPosition = _controlBoxTransform.position;

            var initPoint = _pointerPressedPos;
            var endPoint = t.position + t.forward.normalized * _pointerPressedDistance;
            var initDir = Vector3.ProjectOnPlane(initPoint - controlBoxPosition, _normalPlane).normalized;
            var currentDir = Vector3.ProjectOnPlane(endPoint - controlBoxPosition, _normalPlane).normalized;

            var goal = Quaternion.FromToRotation(initDir, currentDir) * _originalGizmoRot;
            _controlBoxTransform.rotation = Quaternion.Slerp(_controlBoxTransform.rotation, goal, Time.deltaTime * 10f);
            //_controlBoxTransform.rotation = goal;
        }
    }

    public enum Axis
    {
        Right = 0,
        Up = 1,
        Forward = 2
    }

}
