// /******************************************************************************
//  * File: HandModel.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using QCHT.Interactions.Hands.VFF;
using UnityEngine.Serialization;

namespace QCHT.Interactions.Hands
{
    public class HandModel : MonoBehaviour, IHandSkinnable, IHideable, IHandPhysible,
        IHandPoseReceiver, IHandPokeReceiver
    { 
        [SerializeField] private HandPhysible handPhysible;
        [SerializeField] private HandVisualizer mainHandVisualizer;
        [SerializeField] private HandVisualizerGhost ghostHandVisualizer;
        [SerializeField] private HandDriver mainHandDriver;
        
        #region IHandSkinnable

        public HandSkin HandSkin
        {
            get => mainHandVisualizer.HandSkin;
            set
            {
                mainHandVisualizer.HandSkin = value;
                ghostHandVisualizer.HandSkin = value;
            }
        }

        #endregion

        #region IHideable

        public void Show()
        {
            mainHandVisualizer.Show();
            ghostHandVisualizer.Show();
        }

        public void Hide()
        {
            mainHandVisualizer.Hide();
            ghostHandVisualizer.Hide();
        }

        #endregion

        #region IPhysible

        public bool IsPhysible
        {
            get => handPhysible != null && handPhysible.IsPhysible;
            set
            {
                if (handPhysible != null)
                {
                    handPhysible.IsPhysible = value;
                }
            }
        }

        #endregion

        #region IHandPokeReceiver
        
        public Vector3? PokePoint
        {
            get => mainHandDriver.PokePoint;
            set => mainHandDriver.PokePoint = value;
        }
        
        #endregion

        #region IHandPoseReceiver
        
        public Pose? RootPoseOverride => mainHandDriver.RootPoseOverride;
        
        public HandData? HandPoseOverride
        {
            get => mainHandDriver.HandPoseOverride;
            set => mainHandDriver.HandPoseOverride = value;
        }

        public HandMask? HandPoseMask
        {
            get => mainHandDriver.HandPoseMask;
            set => mainHandDriver.HandPoseMask = value;
        }
        
        #endregion
    }
}