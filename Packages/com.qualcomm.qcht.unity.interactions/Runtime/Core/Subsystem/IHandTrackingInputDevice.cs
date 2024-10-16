// /******************************************************************************
//  * File: IHandTrackingInputDevice.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using Hand = QCHT.Interactions.Core.XRHandTrackingSubsystem.Hand;

namespace QCHT.Interactions.Core
{
    public interface IHandTrackingInputDevice
    {
        public void AddDevices();
        public void RemoveDevices();
        public void UpdateDevices(ref Hand leftHand, ref Hand rightHand);
    }
}