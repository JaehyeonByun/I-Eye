// /******************************************************************************
//  * File: HandTrackingDevice.cs
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
using UnityEngine.InputSystem.XR;

namespace QCHT.Interactions.Core
{
    public struct HandTrackingXRControllerInputState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('Q', 'C', 'H', 'T');

        [InputControl(name = "trackingState")]
        public int trackingState;

        [InputControl(name = "isTracked")]
        public bool isTracked;

        [InputControl(name = "devicePosition")]
        public Vector3 devicePosition;

        [InputControl(name = "deviceRotation")]
        public Quaternion deviceRotation;

        [InputControl(name = "pokePosition")]
        public Vector3 pokePosition;

        [InputControl(name = "pokeRotation")]
        public Quaternion pokeRotation;

        [InputControl(name = "pinchPosition")]
        public Vector3 pinchPosition;

        [InputControl(name = "pinchRotation")]
        public Quaternion pinchRotation;

        [InputControl(name = "graspPosition")]
        public Vector3 graspPosition;

        [InputControl(name = "graspRotation")]
        public Quaternion graspRotation;

        [InputControl(name = "pointerPosition")]
        public Vector3 pointerPosition;

        [InputControl(name = "pointerRotation")]
        public Quaternion pointerRotation;

        [InputControl(name = "trigger", usage = "Trigger", layout = "Axis", aliases = new[] {"select"})]
        public float pinch;

        [InputControl(name = "triggerPressed", layout = "Button", aliases = new[] {"selectPressed", "triggerButton"})]
        public bool pinchPressed;

        [InputControl(name = "grip", usage = "Trigger", layout = "Axis", aliases = new[] {"squeeze"})]
        public float grasp;

        [InputControl(name = "gripPressed", layout = "Button", aliases = new[] {"squeezePressed", "gripButton"})]
        public bool graspPressed;

        #region XrHandGestureQCOM

        [InputControl(name = "gesture", layout = "Integer")]
        public int gesture;

        [InputControl(name = "gestureRatio", layout = "Axis")]
        public float gestureRatio;

        [InputControl(name = "flipRatio", layout = "Axis")]
        public float flipRatio;

        #endregion
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(displayName = kDeviceName, commonUsages = new[] {"LeftHand", "RightHand"},
        stateType = typeof(HandTrackingXRControllerInputState))]
    public class HandTrackingDevice : XRController
    {
        public const string kDeviceName = "Qualcomm Hand";

        public AxisControl trigger { get; private set; }
        public ButtonControl triggerPressed { get; private set; }
        public AxisControl grip { get; private set; }
        public ButtonControl gripPressed { get; private set; }
        public Vector3Control pointerPosition { get; private set; }
        public QuaternionControl pointerRotation { get; private set; }
        public Vector3Control pokePosition { get; private set; }
        public QuaternionControl pokeRotation { get; private set; }
        public Vector3Control pinchPosition { get; private set; }
        public QuaternionControl pinchRotation { get; private set; }
        public Vector3Control graspPosition { get; private set; }
        public QuaternionControl graspRotation { get; private set; }
        public IntegerControl gesture { get; private set; }
        public AxisControl gestureRatio { get; private set; }
        public AxisControl flipRatio { get; private set; }

        static HandTrackingDevice()
        {
            InputSystem.RegisterLayout<HandTrackingDevice>(matches: new InputDeviceMatcher().WithProduct(kDeviceName));
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();
            trigger = GetChildControl<AxisControl>("trigger");
            triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
            grip = GetChildControl<AxisControl>("grip");
            gripPressed = GetChildControl<ButtonControl>("gripPressed");
            pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
            pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
            pokePosition = GetChildControl<Vector3Control>("pokePosition");
            pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
            pinchPosition = GetChildControl<Vector3Control>("pinchPosition");
            pinchRotation = GetChildControl<QuaternionControl>("pinchRotation");
            graspPosition = GetChildControl<Vector3Control>("graspPosition");
            graspRotation = GetChildControl<QuaternionControl>("graspRotation");
            gesture = GetChildControl<IntegerControl>("gesture");
            gestureRatio = GetChildControl<AxisControl>("gestureRatio");
            flipRatio = GetChildControl<AxisControl>("flipRatio");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer()
        {
        }
    }
}