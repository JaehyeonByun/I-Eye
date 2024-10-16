// /******************************************************************************
//  * File: XRHandTrackingInputDeviceUpdater.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using Hand = QCHT.Interactions.Core.XRHandTrackingSubsystem.Hand;

namespace QCHT.Interactions.Core
{
    public class XRHandTrackingInputDeviceUpdater
    {
        private IHandTrackingInputDevice _deviceImpl;

        internal bool HasDevices => _deviceImpl != null;

        public void AddDevices()
        {
            if (_deviceImpl == null)
            {
                CreateHandDeviceImpl();
            }

            _deviceImpl?.AddDevices();
        }

        public void RemoveDevices()
        {
            _deviceImpl?.RemoveDevices();
        }

        public void Update(ref Hand leftHand, ref Hand rightHand)
        {
            _deviceImpl?.UpdateDevices(ref leftHand, ref rightHand);
        }

        private void CreateHandDeviceImpl()
        {
#if XR_OPENXR_1_8_0_OR_NEWER
            var handProfile = OpenXRSettings.Instance.GetFeature<HandInteractionProfile>();
            if (!Application.isEditor && handProfile != null && handProfile.enabled)
            {
                Debug.Log("[XRHandTrackingInputDeviceUpdater] Start with HandInteractionProfile...");
                // Don't need input devices as all controls are mapped
                _deviceImpl = new HandTrackingInteractionInputDeviceImpl();
                return;
            }
#endif
            // Microsoft Hand Interaction profile
            var msftHandProfile = OpenXRSettings.Instance.GetFeature<MicrosoftHandInteraction>();
            if (!Application.isEditor && msftHandProfile != null && msftHandProfile.enabled)
            {
                Debug.Log("[XRHandTrackingInputDeviceUpdater] Start with MicrosoftHandInteraction...");
                _deviceImpl = new HandTrackingInputDeviceImpl();
                return;
            }

            // No interaction profile selected 
            // All data has to be set from XR Hand Tracking Device struct
            _deviceImpl = new HandTrackingXRControllerDeviceImpl();
        }
    }
}