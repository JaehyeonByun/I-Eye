// /******************************************************************************
//  * File: XRHandController.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using QCHT.Interactions.Core;
using UnityEngine.Serialization;

namespace QCHT.Interactions.Hands
{
    /// <summary>
    /// Custom XR Controller enabling two select actions grab and select.
    /// Optional select action ( grab action ) has priority on select action but they share the same selectInteractionState.
    /// XRHandController also exposes handedness and can be used to filter interactions. see XRFilter
    /// </summary>
    public class XRHandController : ActionBasedController, IHandedness   
    {
        protected XRHandState HandState;

        [FormerlySerializedAs("_handedness")] [SerializeField] private XrHandedness handedness;

        public XrHandedness Handedness => handedness;
        
        [SerializeField]
        protected InputActionProperty m_OptionalSelectAction;

        /// <summary>
        /// The Input System action to use for grabbing an Interactable.
        /// Must be an action with a button-like interaction where phase equals performed when pressed.
        /// Typically a <see cref="ButtonControl"/> Control or a Value type action with a Press or Sector interaction.
        /// </summary>
        public InputActionProperty grabAction
        {
            get => m_OptionalSelectAction; 
            set => SetInputActionProperty(ref m_OptionalSelectAction, value);
        }

        protected override void Awake()
        {
            base.Awake();
            HandState = new XRHandState();
            currentControllerState = HandState;
        }

        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);

            var handState = controllerState as XRHandState;

            if (handState != null)
            {
                var grabValueAction = m_OptionalSelectAction.action;
                var isGrabbed = IsPressed(m_OptionalSelectAction.action);

                if (isGrabbed || handState.isGrabbed)
                {
                    handState.selectInteractionState.value = ReadValue(grabValueAction);
                    handState.selectInteractionState.activatedThisFrame = isGrabbed && !handState.isGrabbed;
                    handState.selectInteractionState.deactivatedThisFrame = !isGrabbed && handState.isGrabbed;
                    handState.selectInteractionState.active = isGrabbed;
                    handState.isGrabbed = isGrabbed;
                }
            }
        }

        protected void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OptionalSelectAction.EnableDirectAction();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_OptionalSelectAction.DisableDirectAction();
        }
    }
}
