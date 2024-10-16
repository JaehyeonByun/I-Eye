// /******************************************************************************
//  * File: HandTrackingXRControllerDeviceImpl.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Extensions;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Hand = QCHT.Interactions.Core.XRHandTrackingSubsystem.Hand;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;
using InputDevice = UnityEngine.InputSystem.InputDevice;

namespace QCHT.Interactions.Core
{
    public class HandTrackingXRControllerDeviceImpl : IHandTrackingInputDevice
    {
        private HandTrackingDevice _leftHandDevice;
        private HandTrackingDevice _rightHandDevice;

        private HandTrackingXRControllerInputState _leftState;
        private HandTrackingXRControllerInputState _rightState;

        // For now settings are not exposed 
        // You can use C# reflection to tweak them
        
        /// <summary>
        /// Pinch activation sensitivity threshold.
        /// </summary>
        private float _triggerPinchThreshold = 0.2f;

        /// <summary>
        /// Pinch deactivation sensitivity threshold.
        /// </summary>
        private float _releasePinchThreshold = 0.2f;
        
        /// <summary>
        /// Grasp activation sensitivity threshold.
        /// </summary>
        private float _triggerGraspThreshold = 0.2f;

        /// <summary>
        /// Set rotation state of raycast.
        /// </summary>
        private bool _activateRaycastRotation = false;

        /// <summary>
        /// x shoulder offset.
        /// </summary>
        private float _xShoulderOffset = 0.05f;//  when rotation enabled 0.1f;

        /// <summary>
        /// Y shoulder offset.
        /// </summary>
        private float _yShoulderOffset = -0.08f;// when rotation enabled -0.42f;

        /// <summary>
        /// Angle factor.
        /// </summary>
        private float _angleFactor = 0.37f;

        /// <summary>
        /// Min frequency factor for 1 euro filter
        /// </summary>
        private float _fcMinFactor = 5f;
        
        /// <summary>
        /// Beta factor for 1 euro filter
        /// </summary>
        private float _betaFactor = 0.05f;
        
        /// <summary>
        /// Derivative cutoff factor for 1 euro filter
        /// </summary>
        private float _cutoffDerivativeFactor = 1f;

        /// <summary>
        /// Should try to get data from QcComV1?
        /// </summary>
        private bool useQCOMv1 = true;

        /// <summary>
        /// Should try to get data from QcComV2?
        /// </summary>
        private bool useQCOMv2 = true;
        
        private static readonly ProfilerMarker s_updateDeviceMarker = new ProfilerMarker("[QCHT] XRHandTrackingSubsystem.UpdateDevice");

        /// <summary>
        /// Updates Hand Tracking device in input system.
        /// </summary>
        private void UpdateDevice(Hand hand, ref HandTrackingXRControllerInputState state, InputDevice device)
        {
            if (device == null || !device.added) return;

            using (s_updateDeviceMarker.Auto())
            {
                state.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
                state.isTracked = hand.IsTracked;
                 
#if (UNITY_EDITOR || !UNITY_ANDROID)
                useQCOMv1 = false;
                useQCOMv2 = false;
#endif
                if (!useQCOMv1 || !UpdateGestureData(hand.Handedness, ref state, ref hand))
                {
                    UpdateGestureDataFromHand(ref hand, ref state);
                }

                if (!useQCOMv2 || !UpdateInteractionData(hand.Handedness, ref state))
                {
                    UpdateInteractionDataFromHand(ref hand, ref state);
                }

                InputSystem.QueueStateEvent(device, state);
            }
        }

        /// <summary>
        /// Try to get interaction data using XR_QCOM_hand_tracking_gesture
        /// If success interaction data are populated using XrHandGestureV2QCOM structure
        /// </summary>
        /// <param name="handedness"> Handedness to retrieve. </param>
        /// <param name="state"> Updated input state. </param>
        /// <returns></returns>
        private bool UpdateInteractionData(XrHandedness handedness, ref HandTrackingXRControllerInputState state)
        {
            var handExt = handedness == XrHandedness.XR_HAND_LEFT ? XrHandEXT.XR_HAND_LEFT : XrHandEXT.XR_HAND_RIGHT;

            var interactionData = new XrHandGestureV2QCOM();
            if (QCHTOpenXRPlugin.TryGetInteractionData(handExt, ref interactionData) != XrResult.XR_SUCCESS)
            {
                // XrHandGestureV2QCOM not supported :/
                return false;
            }

            // Device pose
            state.devicePosition = interactionData.GripPose.position;
            state.deviceRotation = interactionData.GripPose.rotation;

            // Pointer pose
            state.pointerPosition = interactionData.AimPose.position;
            state.pointerRotation = interactionData.AimPose.rotation;

            // Poke position
            state.pokePosition = interactionData.PokePose.position;
            state.pokeRotation = interactionData.PokePose.rotation;

            // Pinch position 
            state.pinchPosition = interactionData.PinchPose.position;
            state.pinchRotation = interactionData.PinchPose.rotation;

            // Grasp position
            state.graspPosition = interactionData.GripPose.position;
            state.graspRotation = interactionData.GripPose.rotation;

            // Pinch
            state.pinch = interactionData.PinchValue;
            state.pinchPressed = interactionData.PinchValue > _triggerPinchThreshold;

            // Grasp
            state.grasp = interactionData.GraspValue;
            state.graspPressed = interactionData.GraspValue > _triggerGraspThreshold;

            return true;
        }

        /// <summary>
        /// Try to get gesture data using XR_QCOM_hand_tracking_gesture
        /// If success interaction data are populated using XrHandGestureV2QCOM structure
        ///
        /// Hand reference is passed in arguments to handle deprecated fields in hand data struct
        /// It should be removed when definitely deleting those fields.
        /// </summary>
        /// <param name="handedness"> Handedness to retrieve. </param>
        /// <param name="state"> Updated input state. </param>
        /// <returns></returns>
        private bool UpdateGestureData(XrHandedness handedness, ref HandTrackingXRControllerInputState state, ref Hand hand)
        {
            var handExt = handedness == XrHandedness.XR_HAND_LEFT ? XrHandEXT.XR_HAND_LEFT : XrHandEXT.XR_HAND_RIGHT;

            var gestureData = new XrHandGestureQCOM();
            if (QCHTOpenXRPlugin.TryGetHandGestureData(handExt, ref gestureData) != XrResult.XR_SUCCESS)
            {
                // XrHandGestureQCOM not supported :/
                return false;
            }
            
            state.gesture = (int)gestureData.Gesture;
            state.gestureRatio = gestureData.GestureRatio;
            state.flipRatio = gestureData.FlipRatio;

#pragma warning disable CS0612 // Type or member is obsolete
            // Handle deprecated fields in Hand data struct
            // From
            // XrHandGestureTypeQCOM
            // XR_HAND_GESTURE_TYPE_UNKNOWN_QCOM = -1,
            // XR_HAND_GESTURE_TYPE_OPEN_HAND_QCOM = 0,
            // XR_HAND_GESTURE_TYPE_GRAB_QCOM = 2,
            // XR_HAND_GESTURE_TYPE_PINCH_QCOM = 7
            // To
            // XrHandGesture
            // XR_HAND_UNKNOWN = -1,
            // XR_HAND_OPEN_HAND = 0,
            // XR_HAND_PINCH = 1,
            // XR_HAND_GRAB = 2
            var temp = (int)gestureData.Gesture;
            if (temp == 7) temp = 1; // Pinch
            hand._gesture = (XrHandGesture)temp;
            hand._gestureRatio = gestureData.GestureRatio;
            hand._flipRatio = gestureData.FlipRatio;
#pragma warning restore CS0612 // Type or member is obsolete

            return true;
        }

        private void UpdateInteractionDataFromHand(ref Hand hand, ref HandTrackingXRControllerInputState state)
        {
            var eyePose = Pose.identity;

            if (InputSystem.GetDevice<XRHMD>() is var hmd && hmd != null)
            {
                eyePose.position = hmd.centerEyePosition.ReadValue();
                eyePose.rotation = hmd.centerEyeRotation.ReadValue();
            }

            var palm = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_PALM);
            var indexTip = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_INDEX_TIP);
            var thumpTip = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_THUMB_TIP);

            // Device pose
            state.devicePosition = palm.position;
            state.deviceRotation = hand.Root.rotation * (Quaternion.AngleAxis(-90f, Vector3.right) * Quaternion.AngleAxis(-90f, Vector3.up) * Quaternion.Inverse(hand.Root.rotation)) * palm.rotation;

            // Pointer pose
            var aimRay = hand.Handedness == XrHandedness.XR_HAND_LEFT
                ? AimRay.AimRays[(int)XrHandedness.XR_HAND_LEFT]
                : AimRay.AimRays[(int)XrHandedness.XR_HAND_RIGHT];

            var shoulderOffset = hand.Handedness == XrHandedness.XR_HAND_LEFT
                ? new Vector3(-_xShoulderOffset, _yShoulderOffset, 0f)
                : new Vector3(_xShoulderOffset, _yShoulderOffset, 0f);

            var pointerPose = aimRay.GetPose(ref hand, eyePose, shoulderOffset, _angleFactor, _fcMinFactor, _betaFactor, _cutoffDerivativeFactor, true, _activateRaycastRotation);
            
            state.pointerPosition = pointerPose.position;
            state.pointerRotation = pointerPose.rotation;

            // Poke position
            state.pokePosition = indexTip.position;
            state.pokeRotation = indexTip.rotation;

            // Pinch position 
            state.pinchPosition = (indexTip.position + thumpTip.position) * .5f;
            state.pinchRotation = hand.Root.rotation;

            // Grasp position
            state.graspPosition = state.devicePosition;
            state.graspRotation = state.deviceRotation;

            // Inputs
            var wasPinched = state.pinchPressed;
            var pinchStrength = hand.GetFingerPinching(XrFinger.XR_HAND_FINGER_INDEX);

            if (wasPinched)
                state.pinchPressed = pinchStrength > 1f - _releasePinchThreshold; // release threshold
            else
                state.pinchPressed = pinchStrength > 1f - _triggerPinchThreshold; // trigger threshold
            
            state.pinch = pinchStrength;
            
            var grabFilter = new GrabFilter
            {
                Index = hand.GetFingerCurling(XrFinger.XR_HAND_FINGER_INDEX),
                Middle = hand.GetFingerCurling(XrFinger.XR_HAND_FINGER_MIDDLE),
                Ring = hand.GetFingerCurling(XrFinger.XR_HAND_FINGER_RING),
                Pinky = hand.GetFingerCurling(XrFinger.XR_HAND_FINGER_PINKY)
            };

            state.graspPressed = grabFilter.IsGrabbing();
            state.grasp = state.graspPressed ? 1f : 0f;
        }

        private void UpdateGestureDataFromHand(ref Hand hand, ref HandTrackingXRControllerInputState state)
        {
            if (state.pinchPressed)
                state.gesture = (int)XrHandGesture.XR_HAND_PINCH;
            else if (state.graspPressed)
                state.gesture = (int)XrHandGesture.XR_HAND_GRAB;
            else
                state.gesture = (int)XrHandGesture.XR_HAND_OPEN_HAND;

            if (state.pinchPressed)
                state.gestureRatio = state.pinch;
            else if (state.graspPressed)
                state.gestureRatio = state.grasp;
            else
                state.gestureRatio = 1f;
            
            state.flipRatio = GetFlipRatio(ref hand);
        }

        #region IHandTrackingInputDeviceImpl

        public void AddDevices()
        {
            _leftHandDevice ??= AddDevice(true);
            _rightHandDevice ??= AddDevice(false);
        }

        public void RemoveDevices()
        {
            RemoveDevice(_leftHandDevice);
            _leftHandDevice = null;

            RemoveDevice(_rightHandDevice);
            _rightHandDevice = null;
        }

        public void UpdateDevices(ref Hand leftHand, ref Hand rightHand)
        {
            UpdateDevice(leftHand, ref _leftState, _leftHandDevice);
            UpdateDevice(rightHand, ref _rightState, _rightHandDevice);
        }

        private static HandTrackingDevice AddDevice(bool isLeft)
        {
            var usage = isLeft ? CommonUsages.LeftHand : CommonUsages.RightHand;
            var device = InputSystem.AddDevice<HandTrackingDevice>($"{nameof(HandTrackingDevice)} - {usage}");
            if (device != null)
            {
                InputSystem.SetDeviceUsage(device, usage);
            }

            return device;
        }

        private static void RemoveDevice(InputDevice device)
        {
            if (device == null)
            {
                return;
            }

            InputSystem.RemoveDevice(device);
        }

        #endregion

        private static float GetFlipRatio(ref Hand hand)
        {
            var palmCenter = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_PALM).position;
            var thumbMetacarp = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL).position;
            var littleMetacarp = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_LITTLE_METACARPAL).position;

            var eyePose = Pose.identity;

            if (InputSystem.GetDevice<XRHMD>() is var hmd && hmd != null)
            {
                eyePose.position = hmd.centerEyePosition.ReadValue();
                eyePose.rotation = hmd.centerEyeRotation.ReadValue();
            }
            
            var camForward = eyePose.forward;
            var pcToLm = littleMetacarp - palmCenter;
            var pcToTm = thumbMetacarp - palmCenter;
            var crossProduct = Vector3.Cross(pcToLm, pcToTm);
            var dot = Vector3.Dot(crossProduct, camForward);
            
            return hand.Handedness == XrHandedness.XR_HAND_LEFT ? -dot : dot;
        }

        private struct GrabFilter
        {
            public float Index;
            public float Middle;
            public float Ring;
            public float Pinky;

            public bool IsGrabbing()
            {
                var i = 0;
                if (Index > 0.75f) i++;
                if (Middle > 0.75f) i++;
                if (Ring > 0.75f) i++;
                if (Pinky > 0.75f) i++;
                return i > 3;
            }
        }

        internal class AimRay
        {
            public static readonly AimRay[] AimRays =
            {
                new AimRay(true),
                new AimRay(false)
            };

            private OneEuroFilter3 _smooth = new OneEuroFilter3(0.0001f, 0.75f, 0.0001f, 0.75f, 0.0001f, 0.75f);

            private float _previousPinchRatio;

            public AimRay(bool isLeft)
            {
            }

            public Pose GetPose(ref Hand hand, Pose eyePose, Vector3 shoulderOffset, float angleFactor, float fcMinFactor, float betaFactor, float cutoffDerivativeFactor, bool smooth = true, bool rotateRaycast = true)
            {
                var shoulder = eyePose.position + eyePose.rotation * shoulderOffset;
                var rayStart = (hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL).position +
                                hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL).position) / 2.0f;
                var palm = hand.GetHandJoint(XrHandJoint.XR_HAND_JOINT_PALM);
                var rayDir = (rayStart - shoulder).normalized;
                
                if (rotateRaycast)
                {
                    Quaternion.FromToRotation(rayDir, -palm.up).ToAngleAxis(out var rotAngle, out var rotAxis);
                    rayDir = Quaternion.AngleAxis(rotAngle * angleFactor, rotAxis) * rayDir;
                    rayDir.Normalize();
                }
                
                if (smooth)
                {
                    var smoothRayEnd = rayStart + 5.0f * rayDir;
                    var pinchStrength = hand.GetFingerPinching(XrFinger.XR_HAND_FINGER_INDEX);
                    if (pinchStrength <= 0.2f)
                    {
                        if (_previousPinchRatio < pinchStrength)
                        {
                            var smoothRatio = 0.2f * Mathf.Abs(_previousPinchRatio - pinchStrength);
                            var previousEnd = _smooth.Current;
                            smoothRayEnd = (1.0f - smoothRatio) * smoothRayEnd + smoothRatio * previousEnd;
                        }
                    }
                    else
                    {
                        _previousPinchRatio = 0f;
                    }

                    _smooth.MinFrequency = fcMinFactor;
                    _smooth.Beta = betaFactor;
                    _smooth.CutOffDerivative = cutoffDerivativeFactor;
                    _smooth.Filter(ref smoothRayEnd, Time.unscaledTime);
                    
                    rayDir = (smoothRayEnd - rayStart).normalized;
                }
                
                var rayX = Vector3.Cross(palm.forward, rayDir).normalized;
                var rayUp = Vector3.Cross(rayDir, rayX).normalized;
                return new Pose(rayStart, Quaternion.LookRotation(rayDir, rayUp));
            }

            internal struct OneEuroFilter3
            {
                private OneEuroFilter _xFilter, _yFilter, _zFilter;

                public float CutOffDerivative
                {
                    set
                    {
                        _xFilter.CutOffDerivative = value;
                        _yFilter.CutOffDerivative = value;
                        _zFilter.CutOffDerivative = value;
                    }
                }

                public float Beta
                {
                    set
                    {
                        _xFilter.Beta = value;
                        _yFilter.Beta = value;
                        _zFilter.Beta = value;
                    }
                }

                public float MinFrequency
                {
                    set
                    {
                        _xFilter.MinFrequency = value;
                        _yFilter.MinFrequency = value;
                        _zFilter.MinFrequency = value;
                    }
                }

                public Vector3 Current => new Vector3(_xFilter.Current, _yFilter.Current, _zFilter.Current);

                public OneEuroFilter3(in float xMinCutoff, in float xGain, in float yMinCutoff, in float yGain,
                    in float zMinCutoff, in float zGain)
                {
                    _xFilter = new OneEuroFilter(xMinCutoff, xMinCutoff, xGain, 1.0f);
                    _yFilter = new OneEuroFilter(yMinCutoff, yMinCutoff, yGain, 1.0f);
                    _zFilter = new OneEuroFilter(zMinCutoff, zMinCutoff, zGain, 1.0f);
                }

                public void Filter(ref Vector3 vector, float time)
                {
                    _xFilter.Filter(ref vector.x, time);
                    _yFilter.Filter(ref vector.y, time);
                    _zFilter.Filter(ref vector.z, time);
                }
            }

            internal struct OneEuroFilter
            {
                private float _freq, _minCutOff, _beta, _dCutoff;
                private float _lastTime;

                private LowPassFilter _xFilter;
                private LowPassFilter _dxFilter;

                public float Current => _xFilter.Current;

                public float CutOffDerivative
                {
                    set => _dCutoff = value;
                }

                public float Beta
                {
                    set => _beta = value;
                }

                public float MinFrequency
                {
                    set => _freq = Mathf.Max(value, 0.0001f);
                }
                
                public OneEuroFilter(float initFrequency = 30.0f, float minCutoffFrequency = 0.05f,
                    float gainCoeff = 5.0f, float derivativeCutoffFrequency = 5.0f)
                {
                    _freq = Mathf.Max(initFrequency, 0.0001f);
                    _minCutOff = minCutoffFrequency;
                    _beta = gainCoeff;
                    _dCutoff = derivativeCutoffFrequency;
                    _lastTime = -1;

                    _xFilter = new LowPassFilter();
                    _dxFilter = new LowPassFilter();
                }

                public float Filter(ref float x, in float time)
                {
                    if (Math.Abs(time - _lastTime) < Mathf.Epsilon)
                        return _xFilter.Current;

                    var dx = 0f;

                    if (_lastTime > 0 && time > 0)
                    {
                        _freq = 1f / (time - _lastTime);
                    }

                    _lastTime = time;

                    if (_xFilter.HadPrevious)
                    {
                        dx = (x - _xFilter.Current) * _freq;
                    }

                    var edx = _dxFilter.Filter(ref dx, Alpha(_dCutoff));
                    var cutoff = _minCutOff + _beta * Mathf.Abs(edx);
                    return _xFilter.Filter(ref x, Alpha(cutoff));
                }

                private float Alpha(float cutoff)
                {
                    var tau = 1.0f / (2.0f * Mathf.PI * cutoff);
                    var te = 1.0f / _freq;
                    return 1.0f / (1.0f + tau / te);
                }
            }

            internal struct LowPassFilter
            {
                private bool _hadPrevious;

                public bool HadPrevious => _hadPrevious;

                private float _xEstPrev;

                public float Current => _xEstPrev;

                public float Filter(ref float x, in float alpha)
                {
                    float xEst;

                    if (_hadPrevious)
                    {
                        xEst = alpha * x + (1.0f - alpha) * _xEstPrev;
                    }
                    else
                    {
                        xEst = x;
                        _hadPrevious = true;
                    }

                    _xEstPrev = xEst;
                    x = xEst;
                    return xEst;
                }
            }
        }
    }
}