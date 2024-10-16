// /******************************************************************************
//  * File: HandPhysicsConfiguration.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.Hands.VFF
{
    [Serializable]
    public class HandPhysicsJointDrive
    {
        public float Spring = 1f;
        public float Damper = 0.01f;
        public float MaxForce = float.MaxValue;
        
        public JointDrive ToJointDrive => new JointDrive 
        {
                positionSpring = Spring,
                positionDamper = Damper,
                maximumForce = MaxForce
        };
    }
    
    [Serializable]
    public class HandsPhysicsBoneConfiguration
    {
        [Header("Rigidbody")]
        public bool UseGravity;
        public float RigidbodyMass = 0.1f;
        public float RigidbodyDrag;
        public float RigidbodyAngularDrag;

        [Header("Joint")]
        public float JointMassScale = 1.0f;
        public float JointConnectedMassScale = 1.0f;
        public ConfigurableJointMotion LinearMotion;
        public HandPhysicsJointDrive MotionDrive;
        public ConfigurableJointMotion AngularMotion;
        public HandPhysicsJointDrive AngularDrive;
    }

    [CreateAssetMenu(menuName = "QCHT/Interactions/VFF/HandsPhysicsBoneConfiguration")]
    public sealed class HandPhysicsConfiguration : ScriptableObject
    {
        public HandsPhysicsBoneConfiguration Root;
        public HandsPhysicsBoneConfiguration Thumb;
        public HandsPhysicsBoneConfiguration Standard;
    }
}