// /******************************************************************************
//  * File: XROriginUtility.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

#if UNITY_AR_FOUNDATION_LEGACY
using UnityEngine.XR.ARFoundation;
#endif

#if UNITY_AR_FOUNDATION_LEGACY
using TransformExtensions = UnityEngine.XR.ARFoundation.TransformExtensions;
#else
using TransformExtensions = Unity.XR.CoreUtils.TransformExtensions;
#endif

using Unity.XR.CoreUtils;

namespace QCHT.Interactions.Core
{
    public static class XROriginUtility
    {
        private static XROrigin s_xrOrigin;

        public static XROrigin FindXROrigin()
        {
            return s_xrOrigin = s_xrOrigin != null ? s_xrOrigin : Object.FindObjectOfType<XROrigin>();
        }

#if UNITY_AR_FOUNDATION_LEGACY
        private static ARSessionOrigin s_arSessionOrigin;

        public static ARSessionOrigin FindARSessionOrigin()
        {
            return s_arSessionOrigin = s_arSessionOrigin != null ? s_arSessionOrigin : Object.FindObjectOfType<ARSessionOrigin>();
        }
#endif

        public static Camera GetOriginCamera()
        {
            if (s_xrOrigin != null)
            {
                return s_xrOrigin.Camera;
            }
            
#if UNITY_AR_FOUNDATION_LEGACY
            if (s_arSessionOrigin != null)
            {
                return s_arSessionOrigin.camera;
            }
#endif
            
            var camera = FindXROrigin()?.Camera;

#if UNITY_AR_FOUNDATION_LEGACY
            if (camera == null) // prefers XR Origin camera if not null
                camera = FindARSessionOrigin()?.camera;
#endif
            if (camera == null)
                camera = Camera.main;

            return camera;
        }

        public static Transform GetOriginTransform()
        {
            if (s_xrOrigin != null)
            {
                return s_xrOrigin.transform;
            }
            
#if UNITY_AR_FOUNDATION_LEGACY
            if (s_arSessionOrigin != null)
            {
                return s_arSessionOrigin.transform;
            }
#endif
            var transform = FindXROrigin()?.transform;

#if UNITY_AR_FOUNDATION_LEGACY
            if (transform == null) // prefers XR Origin transform if not null
                transform = FindARSessionOrigin()?.transform;
#endif

            return transform;
        }

        public static Transform GetTrackablesParent()
        {
            if (s_xrOrigin != null)
            {
                return s_xrOrigin.TrackablesParent;
            }
            
#if UNITY_AR_FOUNDATION_LEGACY
            if (s_arSessionOrigin != null)
            {
                return s_arSessionOrigin.trackablesParent;
            }
#endif
            
            var parent = FindXROrigin()?.TrackablesParent;

#if UNITY_AR_FOUNDATION_LEGACY
            if (parent == null) // prefers XR Origin trackables parent if not null
                parent = FindARSessionOrigin()?.trackablesParent;
#endif
            return parent;
        }

        public static GameObject GetCameraFloorOffsetObject()
        {
            if (s_xrOrigin != null)
            {
                return s_xrOrigin.CameraFloorOffsetObject;
            }
            
#if UNITY_AR_FOUNDATION_LEGACY
            if (s_arSessionOrigin != null)
            {
                return null;
            }
#endif
            
            var cameraFloorObj = FindXROrigin()?.CameraFloorOffsetObject;

#if UNITY_AR_FOUNDATION_LEGACY
            if (cameraFloorObj == null)
                cameraFloorObj = null;
#endif

            return cameraFloorObj;
        }


        /// <summary>
        /// Converts XR Origin based position to World scene position
        /// </summary>
        /// <param name="point"> Position to transform </param>
        /// <param name="applyCameraOffset"> Should apply camera Y-offset? </param>
        public static void TransformPoint(ref Vector3 point, bool applyCameraOffset = false)
        {
            var origin = GetOriginTransform();
            if (origin == null)
            {
                return;
            }
            
            point = origin.TransformPoint(point);
                
            if (applyCameraOffset)
            {
                var cameraOffset = GetCameraFloorOffsetObject();
                if (cameraOffset != null)
                {
                    point.y += cameraOffset.transform.position.y - origin.position.y;
                }
            }
        }

        /// <summary>
        /// Converts XR Origin based pose to World scene pose
        /// </summary>
        /// <param name="pose"> Pose to transform </param>
        /// <param name="applyCameraOffset"> Should apply camera Y-offset? </param>
        public static void TransformPose(ref Pose pose, bool applyCameraOffset = false)
        {
            var origin = GetOriginTransform();
            if (origin == null)
            {
                return;
            }
            
            pose = TransformExtensions.TransformPose(origin, pose);
            
            if (applyCameraOffset)
            {
                var cameraOffset = GetCameraFloorOffsetObject();
                if (cameraOffset != null)
                {
                    pose.position.y += cameraOffset.transform.position.y - origin.position.y;
                }
            }
        }
    }
}