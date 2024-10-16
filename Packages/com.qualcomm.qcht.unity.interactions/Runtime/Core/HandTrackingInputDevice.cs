// /******************************************************************************
//  * File: HandTrackingInputDevice.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace QCHT.Interactions.Core
{
    public struct HandTrackingInputDeviceState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('Q', 'C', 'H', 'Y');

        [Preserve, InputControl(name = "pokePosition")]
        public Vector3 pokePosition;

        [Preserve, InputControl(name = "pokeRotation")]
        public Quaternion pokeRotation;
        
        [InputControl(name = "flipRatio", layout = "Axis")]
        public float flipRatio;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(displayName = kDeviceName, commonUsages = new[] {"LeftHand", "RightHand"},
        stateType = typeof(HandTrackingInputDeviceState))]
    public class HandTrackingInputDevice : InputDevice
    {
        public const string kDeviceName = "Qualcomm Hand (Simple)";
        
        public Vector3Control pokePosition { get; private set; }
        public QuaternionControl pokeRotation { get; private set; }
        public AxisControl flipRatio { get; private set; }

        static HandTrackingInputDevice()
        {
            InputSystem.RegisterLayout<HandTrackingInputDevice>(matches: new InputDeviceMatcher().WithProduct(kDeviceName));
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();
            pokePosition = GetChildControl<Vector3Control>("pokePosition");
            pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
            flipRatio = GetChildControl<AxisControl>("flipRatio");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer()
        {
        }
    }
}