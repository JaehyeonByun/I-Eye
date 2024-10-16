// /******************************************************************************
//  * File: XRHandTrackingManager.Obsolete.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    public partial class XRHandTrackingManager
    {
        #region Subsystem

        [Obsolete(
            "Consider getting the subsystem with `XRHandTrackingSubsystem.GetSubsystemInManager()` in your script then call subsystem.Status instead.")]
        public HandTrackingStatus HandTrackingStatus => _subsystem?.Status ?? HandTrackingStatus.Idle;

        [Obsolete(
            "Consider getting the subsystem with `XRHandTrackingSubsystem.GetSubsystemInManager()` in your script then call subsystem.RightHand.Scale property instead.")]
        public float RightHandScale => _subsystem?.RightHand.Scale ?? 1f;

        [Obsolete(
            "Consider getting the subsystem with `XRHandTrackingSubsystem.GetSubsystemInManager()` in your script then call subsystem.LeftHand.Scale property instead.")]
        public float RightLeftScale => _subsystem?.LeftHand.Scale ?? 1f;

        [Obsolete(
            "Consider getting the subsystem with `XRHandTrackingSubsystem.GetSubsystemInManager()` in your script then call subsystem.Start() instead.")]
        public void StartHandTracking() => _subsystem?.Start();

        [Obsolete(
            "Consider getting the subsystem with `XRHandTrackingSubsystem.GetSubsystemInManager()` in your script then call subsystem.Stop() instead.")]
        public void StopHandTracking() => _subsystem?.Stop();

        #endregion

        #region HandSkin

        /// <summary>
        /// Try to set hand skin.
        /// Do nothing if the hand object is not ISkinnable.
        /// </summary>
        /// <param name="handedness"> Hand to skin </param>
        /// <param name="skin"> Skin to set. </param>
        [Obsolete(
            "Use the LeftHandSkin and RightHandSkin properties instead. This function will ever return true even if the skin has failed to be set.")]
        public bool SetHandSkin(XrHandedness handedness, HandSkin skin)
        {
            if (handedness == XrHandedness.XR_HAND_LEFT)
                LeftHandSkin = skin;
            else
                RightHandSkin = skin;

            return true;
        }

        /// <summary>
        /// Sets the left hand skin.
        /// Do nothing if the hand object is not ISkinnable.
        /// </summary>
        /// <param name="skin"> Skin to set. </param>
        [Obsolete("Use the LeftHandSkin property instead.")]
        public void SetLeftHandSkin(HandSkin skin) => LeftHandSkin = skin;

        /// <summary>
        /// Sets the right hand skin.
        /// Do nothing if the hand object is not ISkinnable.
        /// </summary>
        /// <param name="skin"> Skin to set. </param>
        [Obsolete("Use the RightHandSkin property instead.")]
        public void SetRightHandSkin(HandSkin skin) => RightHandSkin = skin;

        #endregion

        #region Hand Toggling

        /// <summary>
        /// Toggles the left hand visibility.
        /// Performs fading if the hand object is a IHandFadable. 
        /// </summary>
        /// <param name="visible"> Is the object visible? </param>
        [ContextMenu("ToggleLeftHand", true)]
        public void ToggleLeftHand(bool visible)
        {
            _forceLeftDisabled = !visible;
            
            var isTracked = false;
            if (_subsystem != null)
            {
                isTracked = _subsystem.LeftHand.IsTracked;
            }
            
            UpdateHandVisible(XrHandedness.XR_HAND_LEFT, isTracked);
        }

        /// <summary>
        /// Toggles the right hand visibility.
        /// </summary>
        /// <param name="visible"> Is the object visible? </param>
        [ContextMenu("ToggleRightHand", true)]
        public void ToggleRightHand(bool visible)
        {
            _forceRightDisabled = !visible;
            
            var isTracked = false;
            if (_subsystem != null)
            {
                isTracked = _subsystem.RightHand.IsTracked;
            }
            
            UpdateHandVisible(XrHandedness.XR_HAND_RIGHT, isTracked);
        }

        #endregion
        
        #region Hand pose
        
        [Obsolete("It has been deprecated. Call TrySetHandPose(XrHandedness.XR_HAND_LEFT, HandData, HandMask) instead.")]
        public void SetHandPoseLeft(HandData? data, HandMask? mask) => TrySetHandPose(_leftHand, data, mask);

        [Obsolete("It has been deprecated. Call TrySetHandPose(XrHandedness.XR_HAND_LEFT, HandData, HandMask) instead.")]
        public bool TrySetHandPoseLeft(HandData? data, HandMask? mask) => TrySetHandPose(_leftHand, data, mask);

        [Obsolete("It has been deprecated. Call TrySetHandPose(XrHandedness.XR_HAND_RIGHT, HandData, HandMask) instead.")]
        public void SetHandPoseRight(HandData? data, HandMask? mask) => TrySetHandPose(_rightHand, data, mask);

        [Obsolete("It has been deprecated. Call TrySetHandPose(XrHandedness.XR_HAND_RIGHT, HandData, HandMask) instead.")]
        public bool TrySetHandPoseRight(HandData? data, HandMask? mask) => TrySetHandPose(_rightHand, data, mask);
        
        [Obsolete("It has been deprecated. Call TrySetHandPose(XrHandedness, HandData, HandMask) instead.")]
        public void SetHandPose(XrHandedness handedness, HandData? data, HandMask? mask)
        {
            if (handedness == XrHandedness.XR_HAND_LEFT)
                SetHandPoseLeft(data, mask);
            else
                SetHandPoseRight(data, mask);
        }
        
        #endregion
        
        #region VFF
        
        /// <summary>
        /// Sets virtual force feed back system state if available on left hand object.
        /// </summary>
        /// <param name="active"> is vff active? </param>
        [Obsolete("It has been deprecated. Call TrySetVff(XrHandedness.XR_HAND_LEFT, bool) instead.")]
        public void SetVffLeft(bool active)
        {
            _shouldLeftBeVff = active;
            TrySetVff(_leftHand, active);
        }

        /// <summary>
        /// Sets virtual force feed back system state if available on right hand object.
        /// </summary>
        /// <param name="active"> is vff active? </param>
        [Obsolete("It has been deprecated. Call TrySetVff(XrHandedness.XR_HAND_RIGHT, bool) instead.")]
        public void SetVffRight(bool active)
        {
            _shouldRightBeVff = active;
            TrySetVff(_rightHand, active);
        }
        
        #endregion
    }
}