// /******************************************************************************
//  * File: XRHandTrackingSubsystem.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Core
{
    /// <summary>
    /// Hand Tracking Status.
    /// </summary>
    public enum HandTrackingStatus
    {
        Idle,
        Running,
        Error
    }

    public partial class XRHandTrackingSubsystem : SubsystemWithProvider<XRHandTrackingSubsystem,
        XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem.Provider>
    {
        private Hand _leftHand = new Hand(XrHandedness.XR_HAND_LEFT);

        /// <summary>
        /// Gets left hand data.
        /// </summary>
        public Hand LeftHand => _leftHand;

        private Hand _rightHand = new Hand(XrHandedness.XR_HAND_RIGHT);

        /// <summary>
        /// Gets right hand data.
        /// </summary>
        public Hand RightHand => _rightHand;

        /// <summary>
        /// Event raised when hand has just been tracked.
        /// </summary>
        public event Action<Hand> OnHandTracked;

        /// <summary>
        /// Event raised when hand has just been untracked.
        /// </summary>
        public event Action<Hand> OnHandUntracked;

        /// <summary>
        /// Event raised when hands data have successfully been updated by the subsystem provider.
        /// </summary>
        public event Action<XRInteractionUpdateOrder.UpdatePhase> OnHandsUpdated;

        /// <summary>
        /// Gets the hand tracking subsystem status.
        /// </summary>
        public HandTrackingStatus Status { get; private set; }

        private readonly XRHandTrackingInputDeviceUpdater _inputDeviceUpdater = new XRHandTrackingInputDeviceUpdater();

        protected override void OnCreate()
        {
            base.OnCreate();

            if (provider.Space == XrSpace.XR_HAND_LOCAL)
            {
                _leftHand._space = XrSpace.XR_HAND_LOCAL;
                _rightHand._space = XrSpace.XR_HAND_LOCAL;
            }
            else // XrSpace.XR_HAND_WORLD || XrSpace.XR_HAND_XR_ORIGIN 
            {
                _leftHand._space = XrSpace.XR_HAND_WORLD;
                _rightHand._space = XrSpace.XR_HAND_WORLD;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            _inputDeviceUpdater.AddDevices();
            AddPlayerLoopSystem();
            Application.onBeforeRender += OnBeforeRender;

            Status = HandTrackingStatus.Idle; // Try get status next update
        }

        protected override void OnStop()
        {
            base.OnStop();

            _inputDeviceUpdater.RemoveDevices();
            RemovePlayerLoopSystem();
            Application.onBeforeRender -= OnBeforeRender;

            // Force hand untracked
            _leftHand._isTracked = false;
            OnHandUntracked?.Invoke(_leftHand);

            _rightHand._isTracked = false;
            OnHandUntracked?.Invoke(_rightHand);
        }

        /// <summary>
        /// On before render callback.
        /// </summary>
        [BeforeRenderOrder(XRInteractionUpdateOrder.k_Controllers)]
        private void OnBeforeRender() => UpdateHands(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);

        /// <summary>
        /// Retrieves Hand data for the given XrHandedness
        /// </summary>
        /// <param name="handedness"> Handedness required. </param>
        /// <returns> Hand data struct </returns>
        public Hand GetHand(XrHandedness handedness) => handedness == XrHandedness.XR_HAND_LEFT ? _leftHand : _rightHand;

        private static readonly ProfilerMarker s_updateHandsMarker = new ProfilerMarker("[QCHT] XRHandTrackingSubsystem.UpdateHands");

        /// <summary>
        /// Updates hand data and hand tracking devices in input system.
        /// </summary>
        private void UpdateHands(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            using (s_updateHandsMarker.Auto())
            {
                UpdateHand(ref _leftHand);
                UpdateHand(ref _rightHand);

                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                {
                    if (_inputDeviceUpdater.HasDevices)
                    {
                        _inputDeviceUpdater.Update(ref _leftHand, ref _rightHand);
                    }
                }
            }

            OnHandsUpdated?.Invoke(updatePhase);
        }

        private static readonly ProfilerMarker s_updateHandMarker = new ProfilerMarker("[QCHT] XRHandTrackingSubsystem.UpdateHand");

        /// <summary>
        /// Updates hand data from Hand tracking data provider.
        /// Checks and raises tracked and untracked events when hand is connected or disconnect at this frame.
        /// </summary>
        private void UpdateHand(ref Hand hand)
        {
            var wasTracked = hand.IsTracked;

            using (s_updateHandMarker.Auto())
            {
                Status = provider.TryUpdateHandData(hand._handedness, ref hand._isTracked, ref hand._root, ref hand._joints, ref hand._radiuses);
            }

            if (!wasTracked && hand.IsTracked)
            {
                OnHandTracked?.Invoke(hand);
            }
            else if (wasTracked && !hand.IsTracked)
            {
                OnHandUntracked?.Invoke(hand);
            }
        }

        public static XRHandTrackingSubsystem GetSubsystemInManager()
        {
            var subsystems = new List<XRHandTrackingSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            return subsystems.FirstOrDefault();
        }
    }
}