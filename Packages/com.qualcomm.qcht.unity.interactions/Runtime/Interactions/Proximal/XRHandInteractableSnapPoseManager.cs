// /******************************************************************************
//  * File: XRHandInteractableSnapPoseManager.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using QCHT.Interactions.Core;
using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Proximal
{
    public class XRHandInteractableSnapPoseManager : MonoBehaviour
    {
        protected readonly List<XRHandInteractableSnapPoseReceiver> _snapPoseReceivers =
            new List<XRHandInteractableSnapPoseReceiver>();

        public void OnEnable()
        {
            FindReceivers();
        }
        
        public void RegisterHandPoseReceiver(XRHandInteractableSnapPoseReceiver receiver)
        {
            if (_snapPoseReceivers.Contains(receiver))
                return;
            
            _snapPoseReceivers.Add(receiver);
        }

        public void UnRegisterHandPoseReceiver(XRHandInteractableSnapPoseReceiver receiver)
        {
            _snapPoseReceivers.Remove(receiver);
        }
        
        public void SetHandPose(XrHandedness handedness, HandData? snapPose, HandMask? mask, Pose? rootPose)
        {
            foreach (var receiver in _snapPoseReceivers)
            {
                if (receiver == null)
                    continue;
                
                if (receiver.Handedness == handedness) 
                    receiver.SetPose(snapPose, mask, rootPose);
            }
        }

        private void Reset() => FindReceivers();

        public void FindReceivers()
        {
            var receivers = FindObjectsOfType<XRHandInteractableSnapPoseReceiver>();
            foreach (var receiver in receivers)
            {
                if (_snapPoseReceivers.Contains(receiver))
                    continue;

                _snapPoseReceivers.Add(receiver);
            }
        }
    }
}