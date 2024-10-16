/******************************************************************************
 * File: SpacesARPlaneManagerConfig.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARPlaneManager))]
    public class SpacesARPlaneManagerConfig : MonoBehaviour
    {
        private PlaneDetectionFeature _planeDetection;
        [SerializeField]
        private bool _convexHullEnabled = true;

        public bool UseSceneUnderstandingPlaneDetection
        {
            get =>_planeDetection?.UseSceneUnderstandingPlaneDetection ?? false;
        }

        public bool ConvexHullEnabled
        {
            get => _planeDetection?.ConvexHullEnabled ?? false;
            set
            {
                
                if (_planeDetection != null && _planeDetection.ConvexHullEnabled != value)
                {
                    _planeDetection.ConvexHullEnabled = _convexHullEnabled = value && _planeDetection.UseSceneUnderstandingPlaneDetection;
                }
            }
        }

        public delegate void OnSpacesPlaneManagerConfigStartDelegate();
        public OnSpacesPlaneManagerConfigStartDelegate OnSpacesPlaneManagerConfigStart;
        private void Start()
        {
            _planeDetection = OpenXRSettings.Instance.GetFeature<PlaneDetectionFeature>();
            if (_planeDetection == null)
            {
                Debug.LogError("Could not get valid plane detection feature");
                return;
            }

            ConvexHullEnabled = _convexHullEnabled;
            OnSpacesPlaneManagerConfigStart.Invoke();
        }
    }
}
