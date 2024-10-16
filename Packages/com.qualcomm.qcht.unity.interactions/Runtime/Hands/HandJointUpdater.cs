// /******************************************************************************
//  * File: HandJointUpdater.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using QCHT.Interactions.Core;

namespace QCHT.Interactions.Hands
{
    public class HandJointUpdater : MonoBehaviour, IHandJointUpdater
    {
        public virtual void UpdateJoint(XrSpace space, BoneData data)
        {
            var t = transform;
            
            if (space == XrSpace.XR_HAND_WORLD)
            {
                if (data.UpdatePosition)
                    t.position = data.Position;

                if (data.UpdateRotation)
                    t.rotation = data.Rotation;
            }
            else
            {
                if (data.UpdatePosition)
                    t.localPosition = data.Position;

                if (data.UpdateRotation)
                    t.localRotation = data.Rotation;
            }
        }

        public override string ToString()
        {
            return $" world position = {transform.position.ToString("F4")}," +
                   $" world rotation = {transform.rotation.ToString("F4")} " +
                   $" local scale = {transform.localScale.ToString("F4")} ";
        }
    }
}