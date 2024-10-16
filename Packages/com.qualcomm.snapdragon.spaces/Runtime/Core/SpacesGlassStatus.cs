/******************************************************************************
 * File: SpacesGlassStatus.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Concurrent;

namespace Qualcomm.Snapdragon.Spaces
{
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/Dual Render Fusion/Spaces Glass Status")]
    [DefaultExecutionOrder(int.MinValue)]
    public class SpacesGlassStatus : MonoBehaviour
    {
        /// <summary>
        ///     The glass active state informs whether the glasses are being actively worn.
        /// </summary>
        public enum ActiveState
        {
            /// <summary>
            ///     Glasses are idle. This indicates that they are not being worn (e.g. this is triggered due to proximity sensor timeout).
            ///     This could be used as a hint to save power by deprioritising rendering by reducing quality.
            /// </summary>
            Idle,

            /// <summary>
            ///     Glasses are active. They are being worn and displaying content. Glasses cannot be active if they are disconnected.
            /// </summary>
            Active
        }

        /// <summary>
        ///     The connection states for glasses informs whether glasses are connected or not.
        /// </summary>
        public enum ConnectionState
        {
            /// <summary>
            ///     Glasses are disconnected.
            ///     When glasses are disconnected, ActiveState will be set to idle.
            /// </summary>
            Disconnected,

            /// <summary>
            ///     Glasses are connected.
            ///     When glasses are connected, ActiveState can be either Idle (glasses not in use) or Active (glasses in use).
            /// </summary>
            Connected
        }

        private JavaGlassListener _glassListener;
        private readonly ConcurrentQueue<UnityEvent> _eventsList = new();
#pragma warning disable CS0414
        private static volatile bool _haveNewEvents;
#pragma warning restore CS0414
        internal UnityEvent OnConnected = new();
        internal UnityEvent OnDisconnected = new();
        internal UnityEvent OnIdle;
        internal UnityEvent OnActive;
        public static SpacesGlassStatus Instance { get; private set; }
        public static DeviceTypes DeviceType { get; private set; }
        public ActiveState GlassActiveState { get; private set; }
        public ConnectionState GlassConnectionState { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DeviceType = DeviceTypes.None;
            }
            else
            {
                Destroy(this);
            }

            DontDestroyOnLoad(this.gameObject);
        }

#if !UNITY_EDITOR
        private void Start()
        {

            if (_glassListener == null)
            {
                Debug.Log($"Creating glass listener");
                _glassListener = new JavaGlassListener();
                Debug.Log($"Valid glass listener: {_glassListener != null}");
            }

            DeviceAccessHelper.GetDeviceAccessObject().Call("addGlassListener", _glassListener);
            DeviceAccessHelper.GetDeviceAccessObject().Call("start", DeviceAccessHelper.GetUnityActivity());
        }

        private void LateUpdate()
        {
            if (_haveNewEvents)
            {
                while(_eventsList.TryDequeue(out UnityEvent action))
                {
                    action?.Invoke();
                }

                _haveNewEvents = false;
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            OnApplicationQuit();
        }

        private void OnApplicationQuit()
        {
            if (_glassListener != null)
            {
                DeviceAccessHelper.GetDeviceAccessObject().Call("removeGlassListener", _glassListener);
                _glassListener = null;
                _eventsList.Clear();
                _haveNewEvents = false;
            }

            DeviceAccessHelper.GetDeviceAccessObject().Call("stop");
        }
#endif

        private class JavaGlassListener : AndroidJavaProxy
        {
            public JavaGlassListener() : base("com.qualcomm.qti.device.access.GlassListener") { }

            public void GlassConnected()
            {
                Debug.Log("Callback invoked for GlassConnected! Pending connection...");
                DeviceAccessHelper.GetDeviceType();
                Thread.Sleep(100);
                Debug.Log("GlassConnected");
                Instance.GlassConnectionState = ConnectionState.Connected;
                Instance._eventsList.Enqueue(Instance.OnConnected);
                _haveNewEvents = true;
            }

            public void GlassDisconnected()
            {
                Debug.Log("Callback invoked for GlassDisconnected!");
                DeviceAccessHelper.GetDeviceType();
                Instance.GlassConnectionState = ConnectionState.Disconnected;
                Instance._eventsList.Enqueue(Instance.OnDisconnected);
                _haveNewEvents = true;
            }

            public void GlassIdle()
            {
                Debug.Log("Callback invoked for GlassIdle!");
                Instance.GlassActiveState = ActiveState.Idle;
                Instance._eventsList.Enqueue(Instance.OnIdle);
                _haveNewEvents = true;
            }

            public void GlassActive()
            {
                Debug.Log("Callback invoked for GlassActive!");
                Instance.GlassActiveState = ActiveState.Active;
                Instance._eventsList.Enqueue(Instance.OnActive);
                _haveNewEvents = true;
            }
        }
    }
}
