/******************************************************************************
 * File: SpacesHandManager.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#if AR_FOUNDATION_5_0_OR_NEWER
using Unity.XR.CoreUtils;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html", false)]
    [DisallowMultipleComponent]
#if AR_FOUNDATION_5_0_OR_NEWER
    [RequireComponent(typeof(XROrigin))]
#else
    [RequireComponent(typeof(ARSessionOrigin))]
#endif
    public sealed class SpacesHandManager : ARTrackableManager<XRHandTrackingSubsystem, XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem.Provider, XRTrackedHand, SpacesHand>
    {
        [SerializeField]
        private GameObject _handPrefab;

        public static SpacesHandManager instance;
        public SpacesHand LeftHand => subsystem != null ? GetSpacesHandByTrackableID(subsystem.LeftHand.trackableId) : null;
        public SpacesHand RightHand => subsystem != null ? GetSpacesHandByTrackableID(subsystem.RightHand.trackableId) : null;

        protected override void OnEnable()
        {
            base.OnEnable();
            instance = this;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            instance = null;
        }

        public event Action<SpacesHandsChangedEventArgs> handsChanged;

        protected override GameObject GetPrefab()
        {
            return _handPrefab;
        }

        public GameObject HandPrefab
        {
            get => _handPrefab;
            set => _handPrefab = value;
        }

        protected override string gameObjectName => nameof(SpacesHand);

        protected override void OnAfterSetSessionRelativeData(SpacesHand spacesHand, XRTrackedHand xrHand)
        {
            spacesHand.UpdateHandData(subsystem);
        }

        protected override void OnTrackablesChanged(List<SpacesHand> added, List<SpacesHand> updated, List<SpacesHand> removed)
        {
            if (handsChanged != null)
            {
                handsChanged(new SpacesHandsChangedEventArgs(added, updated, removed));
            }
        }

        private SpacesHand GetSpacesHandByTrackableID(TrackableId id)
        {
            trackables.TryGetTrackable(id, out SpacesHand hand);
            return hand;
        }
    }
}
