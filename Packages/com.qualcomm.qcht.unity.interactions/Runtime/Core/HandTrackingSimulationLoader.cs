// /******************************************************************************
//  * File: HandTrackingSimulationLoader.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    public class HandTrackingSimulationLoader : MonoBehaviour
    {
        private void OnEnable()
        {
            var list = new List<XRHandTrackingSubsystem>();
            SubsystemManager.GetSubsystems(list);
            foreach (var subsystem in list)
            {
                subsystem?.Start();
            }
        }

        private void OnDisable()
        {
            var list = new List<XRHandTrackingSubsystem>();
            SubsystemManager.GetSubsystems(list);
            foreach (var subsystem in list)
            {
                subsystem.Stop();
            }
        }

        private static readonly List<XRHandTrackingSubsystemDescriptor> s_handTrackingSubsystemDescriptors =
            new List<XRHandTrackingSubsystemDescriptor>();

#if UNITY_EDITOR || UNITY_STANDALONE
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void CreateHandTrackingSimulator()
        {
            if (!HandTrackingSimulationSettings.Instance.enabled)
                return;

            XRHandTrackingSubsystemDescriptor.Create(new XRHandTrackingSubsystemDescriptor.Cinfo
            {
                id = XRHandSimulationProvider.ID,
                providerType = typeof(XRHandSimulationProvider)
            });

            CreateSubsystem<XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem>(
                s_handTrackingSubsystemDescriptors, XRHandSimulationProvider.ID);

            var gameObject = new GameObject("Hand Tracking Simulator");
            gameObject.AddComponent<HandTrackingSimulationLoader>();
            DontDestroyOnLoad(gameObject);
        }

        private static ISubsystem CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id)
            where TDescriptor : ISubsystemDescriptor where TSubsystem : ISubsystem
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            SubsystemManager.GetSubsystemDescriptors<TDescriptor>(descriptors);

            if (descriptors.Count <= 0)
                return null;

            foreach (var descriptor in descriptors)
                if (string.Compare(descriptor.id, id, StringComparison.OrdinalIgnoreCase) == 0)
                    return descriptor.Create();

            return null;
        }
    }
}