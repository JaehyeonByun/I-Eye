/******************************************************************************
 * File: SpacesARCameraManagerConfig.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARCameraManager))]
    public class SpacesARCameraManagerConfig : MonoBehaviour
    {
        private CameraSubsystem _cameraSubsystem;
        private CameraAccessFeature _cameraAccess;
        private uint _cameraSensorsCount = 0;

        void Start()
        {
            _cameraAccess = OpenXRSettings.Instance.GetFeature<CameraAccessFeature>();
            if (_cameraAccess == null)
            {
                Debug.LogError("Could not get valid camera access feature");
                return;
            }

            var cameraSubsystems = new List<CameraSubsystem>();
            SubsystemManager.GetInstances(cameraSubsystems);
            if (cameraSubsystems.Count > 0)
            {
                _cameraSubsystem = cameraSubsystems[0];
            }
            else
            {
                Debug.LogError("Could not get valid camera access subsystem");
            }
        }

        public uint GetActiveCameraCount()
        {
            if (_cameraSubsystem != null && _cameraSensorsCount == 0)
            {
                CameraSubsystem.CameraProvider cameraProvider = (CameraSubsystem.CameraProvider)_cameraSubsystem.GetProvider();
                _cameraSensorsCount = cameraProvider.CameraInfo.SensorCount;
            }
            return _cameraSensorsCount;
        }

        public bool SetUseDirectMemoryAccess(bool useDirectMemoryAccess)
        {
            if (_cameraAccess == null)
            {
                Debug.LogError("Could not get valid camera access feature");
                return false;
            }

            _cameraAccess.DirectMemoryAccessConversion = useDirectMemoryAccess;
            return true;
        }
    }
}
