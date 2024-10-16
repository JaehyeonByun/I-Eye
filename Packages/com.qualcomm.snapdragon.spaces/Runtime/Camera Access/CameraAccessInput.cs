/******************************************************************************
 * File: CameraAccessInput.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace Qualcomm.Snapdragon.Spaces
{
    public struct CameraAccessInputState : IInputStateTypeInfo
    {
        public FourCC format => new('S', 'C', 'F', 'A');

        [Preserve] [InputControl(name = "ColorCameraPosition")]
        public Vector3 ColorCameraPosition;

        [Preserve] [InputControl(name = "ColorCameraRotation")]
        public Quaternion ColorCameraRotation;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(displayName = DeviceName,
        stateType = typeof(CameraAccessInputState))]
    public class CameraAccessInputDevice : InputDevice
    {
        public const string DeviceName = "Snapdragon Spaces Camera Frame Access";

        static CameraAccessInputDevice()
        {
            InputSystem.RegisterLayout<CameraAccessInputDevice>(matches: new InputDeviceMatcher().WithProduct(DeviceName));
        }

        public Vector3Control ColorCameraPosition { get; private set; }
        public QuaternionControl ColorCameraRotation { get; private set; }

        protected override void FinishSetup()
        {
            base.FinishSetup();
            ColorCameraPosition = GetChildControl<Vector3Control>("ColorCameraPosition");
            ColorCameraRotation = GetChildControl<QuaternionControl>("ColorCameraRotation");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer()
        {
        }
    }
}
