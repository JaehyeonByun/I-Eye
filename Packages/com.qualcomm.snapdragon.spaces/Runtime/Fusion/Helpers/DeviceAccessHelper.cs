/******************************************************************************
 * File: DeviceAccessHelper.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    /// <summary>
    ///     The glass device types which can be detected.
    /// </summary>
    public enum DeviceTypes
    {
        /// <summary>
        ///     "Disconnected", there is no device connection.
        /// </summary>
        None,

        /// <summary>
        ///     All In One glass type.
        /// </summary>
        Aio,

        /// <summary>
        ///     Glasses connected wired to a host.
        /// </summary>
        Wired,

        /// <summary>
        ///     Glasses connected wirelessly to a host.
        /// </summary>
        Wireless
    }

    public static class DeviceAccessHelper
    {
        public static DeviceTypes CurrentDeviceType { get; private set; }
        private static AndroidJavaObject _deviceAccessObject;
        private static JavaDeviceAccessCallbacks _deviceAccessCallbacks;
        private static AndroidJavaObject _unityActivity;

        /// <summary>
        ///     Returns what device is currently being used.
        /// </summary>
        public static DeviceTypes GetDeviceType()
        {
            if (CurrentDeviceType == DeviceTypes.None)
            {
                var deviceTypeRet = GetDeviceAccessObject().Call<AndroidJavaObject>("getDeviceType");
                var tempType = deviceTypeRet.Call<int>("ordinal");
                switch (tempType)
                {
                    case 1:
                        CurrentDeviceType = DeviceTypes.Aio;
                        break;
                    case 2:
                        CurrentDeviceType = DeviceTypes.Wired;
                        break;
                    case 3:
                        CurrentDeviceType = DeviceTypes.Wireless;
                        break;
                    default:
                        CurrentDeviceType = DeviceTypes.None;
                        break;
                }

                Debug.Log("Device Type is: " + CurrentDeviceType);
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            return CurrentDeviceType;
#endif
            return DeviceTypes.None;
        }


        public class JavaDeviceAccessCallbacks : AndroidJavaProxy
        {
            public JavaDeviceAccessCallbacks() : base("com.qualcomm.qti.device.access.DeviceAccessCallbacks") { }

            public void OnServiceReady()
            {
                Debug.Log("DeviceAccess is Ready!");
                GetDeviceType();
            }

            public void OnServiceLost()
            {
                Debug.Log("DeviceAccess is no longer available.");

                _deviceAccessObject.Call("start", GetUnityActivity());
            }
        }

        public static AndroidJavaObject GetUnityActivity()
        {
            _unityActivity ??= new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            return _unityActivity;
        }

        public static AndroidJavaObject GetDeviceAccessObject()
        {
            if (_deviceAccessObject == null)
            {
                var deviceAccessClass = new AndroidJavaClass("com.qualcomm.qti.device.access.DeviceAccessManager");
                _deviceAccessObject = deviceAccessClass.CallStatic<AndroidJavaObject>("getInstance", GetDeviceAccessCallbacks());
            }

            return _deviceAccessObject;
        }

        public static JavaDeviceAccessCallbacks GetDeviceAccessCallbacks()
        {
            if (_deviceAccessCallbacks == null)
            {
                Debug.Log($"Creating DeviceAccess callbacks");
                _deviceAccessCallbacks = new JavaDeviceAccessCallbacks();
                Debug.Log($"Valid DeviceAccess callbacks: {_deviceAccessCallbacks != null}");
            }

            return _deviceAccessCallbacks;
        }
    }
}
