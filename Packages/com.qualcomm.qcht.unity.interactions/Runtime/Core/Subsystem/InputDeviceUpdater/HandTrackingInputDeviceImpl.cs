// /******************************************************************************
//  * File: HandTrackingInputDeviceImpl.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;
using InputDevice = UnityEngine.InputSystem.InputDevice;
using Hand = QCHT.Interactions.Core.XRHandTrackingSubsystem.Hand;

namespace QCHT.Interactions.Core
{
    public class HandTrackingInputDeviceImpl : IHandTrackingInputDevice
    {
        private HandTrackingInputDevice _leftHandDevice;
        private HandTrackingInputDevice _rightHandDevice;

        private HandTrackingInputDeviceState _leftState;
        private HandTrackingInputDeviceState _rightState;

        private static void UpdateDevice(Hand hand, ref HandTrackingInputDeviceState state, InputControl device)
        {
            if (device == null) return;

            // Poke position
            var indexPose = hand.Joints[(int) XrHandJoint.XR_HAND_JOINT_INDEX_TIP];
            state.pokePosition = indexPose.position;
            state.pokeRotation = indexPose.rotation;
            state.flipRatio = -1f; //TODO: find a computation to fill this

            InputState.Change(device, state);
        }

        #region IHandTrackingInputDeviceImpl

        public void AddDevices()
        {
            _leftHandDevice ??= AddDevice(true);
            _rightHandDevice ??= AddDevice(false);
        }

        public void RemoveDevices()
        {
            RemoveDevice(_leftHandDevice);
            _leftHandDevice = null;

            RemoveDevice(_rightHandDevice);
            _rightHandDevice = null;
        }

        public void UpdateDevices(ref Hand leftHand, ref Hand rightHand)
        {
            UpdateDevice(leftHand, ref _leftState, _leftHandDevice);
            UpdateDevice(rightHand, ref _rightState, _rightHandDevice);
        }

        #endregion

        private static HandTrackingInputDevice AddDevice(bool isLeft)
        {
            var usage = isLeft ? CommonUsages.LeftHand : CommonUsages.RightHand;
            var device =
                InputSystem.AddDevice<HandTrackingInputDevice>(
                    $"{nameof(HandTrackingInputDevice)} - {usage}");
            if (device != null) InputSystem.SetDeviceUsage(device, usage);
            return device;
        }

        private static void RemoveDevice(InputDevice device)
        {
            if (device == null) return;
            InputSystem.RemoveDevice(device);
        }
    }
}