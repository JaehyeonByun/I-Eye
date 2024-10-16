// /******************************************************************************
//  * File: XRHandInteractableSnapPoseReceiver.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Proximal
{
    public class XRHandInteractableSnapPoseReceiver : MonoBehaviour
    {
        private XRHandInteractableSnapPoseManager _snapPoseManager;

        [field:SerializeField] public HandDriver HandDriver { get; private set; }

        public XrHandedness Handedness => HandDriver.Handedness;

        private void Awake()
        {
            FindCreateSnapPoseManager();
            // A hand pose can be set or reset even if driver is disabled
            _snapPoseManager.RegisterHandPoseReceiver(this);
        }

        private void OnDestroy()
        {
            _snapPoseManager.UnRegisterHandPoseReceiver(this);
        }

        private void Reset()
        {
            if (HandDriver == null)
                HandDriver = GetComponentInParent<HandDriver>();
        }

        public void SetPose(HandData? pose, HandMask? mask, Pose? rootPose)
        {
            if (HandDriver == null) 
                return;
            
            HandDriver.HandPoseOverride = pose;
            HandDriver.HandPoseMask = mask;
            HandDriver.RootPoseOverride = rootPose;
        }
        
        private void FindCreateSnapPoseManager()
        {
            if (_snapPoseManager != null)
                return;

            _snapPoseManager = FindObjectOfType<XRHandInteractableSnapPoseManager>();

            if (_snapPoseManager == null)
            {
                _snapPoseManager = new GameObject(nameof(XRHandInteractableSnapPoseManager),
                    typeof(XRHandInteractableSnapPoseManager)).GetComponent<XRHandInteractableSnapPoseManager>();
            }
        }
    }
}