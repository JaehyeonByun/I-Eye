// /******************************************************************************
//  * File: HandTrackingInteractionInputDeviceImpl.cs
//  * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  *
//  ******************************************************************************/

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR.OpenXR.Features.Interactions;
using Hand = QCHT.Interactions.Core.XRHandTrackingSubsystem.Hand;

namespace QCHT.Interactions.Core
{
    public class HandTrackingInteractionInputDeviceImpl : IHandTrackingInputDevice
    {
        private XRHandTrackingSubsystem _subsystem;
        
        public void AddDevices()
        {
            // Ignored
            // Devices already added by OXR input plugin
        }

        public void RemoveDevices()
        {
            // Ignored
        }

        public void UpdateDevices(ref Hand leftHand, ref Hand rightHand)
        {
            // FIX: This implementation is a fix for hand interaction profile where the isTracked value is never updated  
            _subsystem ??= XRHandTrackingSubsystem.GetSubsystemInManager();
            
            if (_subsystem == null)
            {
                return;
            }
            
            var leftDevice = InputSystem.GetDevice<HandInteractionProfile.HandInteraction>(CommonUsages.LeftHand);
            
            if (leftDevice != null)
            {
                using (StateEvent.From(leftDevice, out var eventPtr))
                {
                    var value = _subsystem.LeftHand.IsTracked ? 1f : 0f;
                    ((ButtonControl) leftDevice["isTracked"]).WriteValueIntoEvent(value, eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                    //InputState.Change(leftDevice, eventPtr);
                }
            }

            var rightDevice = InputSystem.GetDevice<HandInteractionProfile.HandInteraction>(CommonUsages.RightHand);
            
            if (rightDevice != null)
            {
                using (StateEvent.From(rightDevice, out var eventPtr))
                {
                    var value = _subsystem.RightHand.IsTracked ? 1f : 0f;
                    ((ButtonControl) rightDevice["isTracked"]).WriteValueIntoEvent(value, eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                    //InputState.Change(rightDevice, eventPtr);
                }
            }
            // End FIX
        }
    }
}