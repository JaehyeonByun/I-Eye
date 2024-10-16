/******************************************************************************
 * File: XrPlaneLocationQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrPlaneLocationQCOM
    {
        private ulong _locationFlags;
        private XrPosef _pose;
        private uint _id;
        private XrPlaneTypeQCOM _planeType;
        private float _confidence;
        private XrPlaneExtentQCOM _size;
        private ulong _convexHullBufferId;

        public ulong ConvexHullId => _convexHullBufferId;

        public BoundedPlane GetBoundedPlane(ulong planeDetectionHandle)
        {
            float sizeX = _size.ExtentX.Max - _size.ExtentX.Min;
            // NOTE(AF): Min and Max are inversed for Y because of the OpenXR -> Unity coordinate system conversion.
            float sizeY = _size.ExtentY.Min - _size.ExtentY.Max;
            return new BoundedPlane(new TrackableId(planeDetectionHandle, _id),
                TrackableId.invalidId,
                _pose.ToPose(),
                // NOTE(AF): Min and Max are inversed for Y because of the OpenXR -> Unity coordinate system conversion.
                new Vector2(_size.ExtentX.Min + (sizeX / 2f), _size.ExtentY.Max + (sizeY / 2f)),
                new Vector2(sizeX, sizeY),
                XrPlaneTypeToPlaneAlignment(_planeType),
                TrackingState.Tracking,
                IntPtr.Zero,
                PlaneClassification.None);
        }

        private PlaneAlignment XrPlaneTypeToPlaneAlignment(XrPlaneTypeQCOM type)
        {
            switch (type)
            {
                case XrPlaneTypeQCOM.XR_PLANE_TYPE_HORIZONTAL_UPWARD_QCOM: return PlaneAlignment.HorizontalUp;
                case XrPlaneTypeQCOM.XR_PLANE_TYPE_HORIZONTAL_DOWNWARD_QCOM: return PlaneAlignment.HorizontalDown;
                case XrPlaneTypeQCOM.XR_PLANE_TYPE_VERTICAL_QCOM: return PlaneAlignment.Vertical;
                case XrPlaneTypeQCOM.XR_PLANE_TYPE_ARBITRARY_QCOM: return PlaneAlignment.NotAxisAligned;
                default:
                    Debug.LogWarning("XrPlaneTypeQCOM [ " + type + "]: not supported!");
                    return PlaneAlignment.None;
            }
        }
    }
}
