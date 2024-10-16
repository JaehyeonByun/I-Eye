/******************************************************************************
 * File: CameraAccessInputUpdate.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Qualcomm.Snapdragon.Spaces
{
    public class CameraAccessInputUpdate
    {
        private CameraAccessInputDevice _rgbInputDevice;

        private CameraAccessInputState _rgbInputState;

        public void UpdateCameraDevice(Pose extrinsics)
        {
            if (_rgbInputDevice == null) { return; }

            _rgbInputState.ColorCameraPosition = extrinsics.position;
            _rgbInputState.ColorCameraRotation = extrinsics.rotation;

            InputState.Change(_rgbInputDevice, _rgbInputState);
        }

        public void AddDevice()
        {
            var usage = "RGBCamera";
            _rgbInputDevice = InputSystem.AddDevice<CameraAccessInputDevice>(usage);
            if (_rgbInputDevice != null)
            {
                InputSystem.SetDeviceUsage(_rgbInputDevice, usage);
            }
        }

        public void RemoveDevice()
        {
            if (_rgbInputDevice == null)
            {
                return;
            }

            InputSystem.RemoveDevice(_rgbInputDevice);
        }
    }
}
