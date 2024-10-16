// /******************************************************************************
//  * File: XRHandTrackingSubsystem.Hand.Obsolete.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;

namespace QCHT.Interactions.Core
{
    public partial class XRHandTrackingSubsystem
    {
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
        public partial struct Hand
#pragma warning restore CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
        {
            /// <summary>
            /// Gesture as XrHandGesture.
            /// In QCHT Provider, this value is retrieved from QCHT OXR extension XR_QCOM_hand_tracking_gesture
            /// </summary>
            [Obsolete]
            internal XrHandGesture _gesture;
            [Obsolete] // TODO: Add deprecated message
            public XrHandGesture Gesture => _gesture; 

            /// <summary>
            /// Gesture ratio.
            /// In QCHT Provider, this value is retrieved from QCHT OXR extension XR_QCOM_hand_tracking_gesture
            /// </summary>
            [Obsolete]
            internal float _gestureRatio;
            [Obsolete] // TODO: Add deprecated message
            public float GestureRatio => _gestureRatio;
            
            /// <summary>
            /// Flip ratio
            /// Should be clamped between -1 and 1.
            /// 1 value corresponds to palm orientation turned toward from XMD eye position.
            /// -1 value corresponds to palm orientation turned backward from XMD eye position.
            /// In QCHT Provider, this value is retrieved from QCHT OXR extension XR_QCOM_hand_tracking_gesture
            /// </summary>
            [Obsolete]
            internal float _flipRatio;
            [Obsolete] // TODO: Add deprecated message
            public float FlipRatio => _flipRatio;
        }
    }
}