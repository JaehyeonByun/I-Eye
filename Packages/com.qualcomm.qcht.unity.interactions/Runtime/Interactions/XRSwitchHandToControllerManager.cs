// /******************************************************************************
//  * File: XRSwitchHandToControllerManager.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using QCHT.Interactions.Core;
using UnityEngine.Serialization;

namespace QCHT.Interactions
{
    public class XRSwitchHandToControllerManager : MonoBehaviour
    {
        /// <summary>
        /// Controller modes
        /// </summary>
        public enum ControllerMode
        {
            HandTracking,
            Controller
        }

        /// <summary>
        /// Defines when to switch to controller
        /// </summary>
        private enum SwitchRule
        {
            AtLeastOneConnected,
            BothConnected
        }

        [Space]
        [FormerlySerializedAs("_switchToControllerRule")]
        [SerializeField] private SwitchRule switchToControllerRule = SwitchRule.AtLeastOneConnected;

        [Space]
        [FormerlySerializedAs("_isLeftControllerTracked")]
        [SerializeField] private InputActionProperty isLeftControllerTracked;
        [FormerlySerializedAs("_isRightControllerTracked")] [SerializeField]
        private InputActionProperty isRightControllerTracked;

        [Space]
        [FormerlySerializedAs("_leftHandController")]
        [SerializeField] private GameObject leftHandController;
        [FormerlySerializedAs("_rightHandController")]
        [SerializeField] private GameObject rightHandController;

        [Space]
        [FormerlySerializedAs("_leftController")]
        [SerializeField] private GameObject leftController;
        [FormerlySerializedAs("_rightController")]
        [SerializeField] private GameObject rightController;

        [Space]
        public UnityEvent<ControllerMode> OnControllerSwitched = new UnityEvent<ControllerMode>();

        [Header("[Mock]")]
        [FormerlySerializedAs("_mockLeftTracked")]
        [SerializeField] private bool mockLeftTracked;
        [FormerlySerializedAs("_mockRightTracked")]
        [SerializeField] private bool mockRightTracked;

        private XRHandTrackingSubsystem _handTrackingSubsystem;

        private ControllerMode _mode;

        /// <summary>
        /// Defines which input action to test for left controller activation.
        /// Typically IsTracked input action on controller tracked devices.
        /// </summary>
        public InputActionProperty LeftControllerTracked
        {
            get => isLeftControllerTracked;
            set => isLeftControllerTracked = value;
        }

        /// <summary>
        /// Defines which input action to test for right controller activation.
        /// Typically IsTracked input action on controller tracked devices.
        /// </summary>
        public InputActionProperty RightControllerTracked
        {
            get => isRightControllerTracked;
            set => isRightControllerTracked = value;
        }

        /// <summary>
        /// References to a group of interactors attached for left Hand.  
        /// </summary>
        public GameObject LeftHandController
        {
            get => leftHandController;
            set => leftHandController = value;
        }

        /// <summary>
        /// References to a group of interactors attached for right Hand.  
        /// </summary>
        public GameObject RightHandController
        {
            get => rightHandController;
            set => rightHandController = value;
        }

        /// <summary>
        /// References to a group of interactors attached for left motion controller.  
        /// </summary>
        public GameObject LeftController
        {
            get => leftController;
            set => leftController = value;
        }

        /// <summary>
        /// References to a group of interactors attached for right motion controller.  
        /// </summary>
        public GameObject RightController
        {
            get => rightController;
            set => rightController = value;
        }

        /// <summary>
        /// Gets the current controller mode.
        /// It should be Hand Tracking or Motion Controller. 
        /// </summary>
        public ControllerMode CurrentMode => _mode;

        protected void OnEnable()
        {
            _handTrackingSubsystem ??= XRHandTrackingSubsystem.GetSubsystemInManager();

            isLeftControllerTracked.EnableDirectAction();
            isRightControllerTracked.EnableDirectAction();
        }

        protected void OnDisable()
        {
            isLeftControllerTracked.DisableDirectAction();
            isRightControllerTracked.DisableDirectAction();
        }

        protected void Update()
        {
#if UNITY_EDITOR && ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.B))
            {
                mockLeftTracked = !mockLeftTracked;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                mockRightTracked = !mockRightTracked;
            }
#endif
            var shouldSwitchToController = ShouldSwitchToController();
            SwitchHandTrackingController(shouldSwitchToController ? ControllerMode.Controller : ControllerMode.HandTracking);
            
            var isTrackingHands = _mode == ControllerMode.HandTracking;
            if (leftHandController) leftHandController.SetActive(isTrackingHands && _handTrackingSubsystem.LeftHand.IsTracked);
            if (rightHandController) rightHandController.SetActive(isTrackingHands && _handTrackingSubsystem.RightHand.IsTracked);
            if (leftController) leftController.SetActive(!isTrackingHands && IsLeftTrackerConnected());
            if (rightController) rightController.SetActive(!isTrackingHands && IsRightTrackerConnected());
        }

        protected bool ShouldSwitchToController()
        {
            var leftTracked = IsLeftTrackerConnected();
            var rightTracked = IsRightTrackerConnected();
            
            return switchToControllerRule switch
            {
                SwitchRule.AtLeastOneConnected => leftTracked || rightTracked,
                SwitchRule.BothConnected => leftTracked && rightTracked,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected bool IsLeftTrackerConnected()
        {
#if UNITY_EDITOR && ENABLE_LEGACY_INPUT_MANAGER
            return mockLeftTracked;
#else
            return isLeftControllerTracked.action.IsPressed();
#endif
        }

        protected bool IsRightTrackerConnected()
        {
#if UNITY_EDITOR && ENABLE_LEGACY_INPUT_MANAGER
            return mockRightTracked;
#else
            return isRightControllerTracked.action.IsPressed();
#endif
        }

        protected void SwitchHandTrackingController(ControllerMode mode)
        {
            if (_mode == mode)
            {
                if (_handTrackingSubsystem != null)
                {
                    // Fix: Ensures hat subsystem internal state
                    if (_mode == ControllerMode.HandTracking && !_handTrackingSubsystem.running)
                    {
                        _handTrackingSubsystem.Start();
                    }
                    else if (_mode == ControllerMode.Controller && _handTrackingSubsystem.running)
                    {
                        _handTrackingSubsystem.Stop();
                    }
                }
                
                return;
            }

            // Controller has switched
            _mode = mode;

            var isTrackingHands = mode == ControllerMode.HandTracking;

            if (isTrackingHands)
            {
                _handTrackingSubsystem?.Start();
            }
            else
            {
                _handTrackingSubsystem?.Stop();
            }

            OnControllerSwitched.Invoke(mode);
        }
    }
}