// /******************************************************************************
//  * File: OpenXRInterop.NativeTypes.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrHandGestureQCOM
    {
        private XrHandGestureTypeQCOM _gesture;
        private float _gestureRatio;
        private float _flipRatio;

        public XrHandGestureTypeQCOM Gesture => _gesture;
        public float GestureRatio => _gestureRatio;
        public float FlipRatio => _flipRatio;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XrHandGestureV2QCOM
    {
        private float _aimValue;
        private float _graspValue;
        private float _pinchValue;
        private XrPosef _aimPose;
        private XrPosef _gripPose;
        private XrPosef _pinchPose;
        private XrPosef _pokePose;
        private XrPosef _palmPose;

        public float AimValue => _aimValue;
        public float GraspValue => _graspValue;
        public float PinchValue => _pinchValue;
        public Pose AimPose => _aimPose.ToPose();
        public Pose GripPose => _gripPose.ToPose();
        public Pose PinchPose => _pinchPose.ToPose();
        public Pose PokePose => _pokePose.ToPose();
        public Pose PalmPose => _palmPose.ToPose();
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XrPosef
    {
        private XrQuaternionf _orientation;
        private XrVector3f _position;

        public XrPosef(Pose pose)
        {
            _orientation = new XrQuaternionf(pose.rotation);
            _position = new XrVector3f(pose.position);
        }

        public XrPosef(XrQuaternionf orientation, XrVector3f position)
        {
            _orientation = orientation;
            _position = position;
        }

        public Pose ToPose()
        {
            return new Pose(_position.ToVector3(), _orientation.ToQuaternion());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XrQuaternionf
    {
        private float _x;
        private float _y;
        private float _z;
        private float _w;

        public XrQuaternionf(Quaternion quaternion)
        {
            _x = quaternion.x;
            _y = quaternion.y;
            _z = -quaternion.z;
            _w = -quaternion.w;
        }

        public static XrQuaternionf identity => new XrQuaternionf(new Quaternion(0, 0, -0, -1));

        public Quaternion ToQuaternion()
        {
            return new Quaternion(_x, _y, -_z, -_w);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XrVector3f
    {
        private float _x;
        private float _y;
        private float _z;

        public XrVector3f(Vector3 position)
        {
            _x = position.x;
            _y = position.y;
            _z = -position.z;
        }

        public static XrVector3f zero => new XrVector3f(new Vector3(0, 0, -0));

        public Vector3 ToVector3()
        {
            return new Vector3(_x, _y, -_z);
        }
    }

    internal enum XrHandGestureTypeQCOM
    {
        XR_HAND_GESTURE_TYPE_UNKNOWN_QCOM = -1,
        XR_HAND_GESTURE_TYPE_OPEN_HAND_QCOM = 0,
        XR_HAND_GESTURE_TYPE_GRAB_QCOM = 2,
        XR_HAND_GESTURE_TYPE_PINCH_QCOM = 7,
        // XR_HAND_GESTURE_TYPE_POINT_QCOM = 8,
        // XR_HAND_GESTURE_TYPE_VICTORY_QCOM = 9,
        // XR_HAND_GESTURE_TYPE_METAL_QCOM = 11,
        // XR_HAND_GESTURE_TYPE_MAX_ENUM_QCOM = 0x7FFFFFFF
    }

    internal enum XrHandEXT
    {
        XR_HAND_LEFT = 1,
        XR_HAND_RIGHT = 2,
        XR_HAND_MAX_ENUM = 0x7FFFFFFF
    }

    internal enum XrResult
    {
        XR_SUCCESS = 0,
        XR_TIMEOUT_EXPIRED = 1,
        XR_SESSION_LOSS_PENDING = 3,
        XR_EVENT_UNAVAILABLE = 4,
        XR_SPACE_BOUNDS_UNAVAILABLE = 7,
        XR_SESSION_NOT_FOCUSED = 8,
        XR_FRAME_DISCARDED = 9,
        XR_ERROR_VALIDATION_FAILURE = -1,
        XR_ERROR_RUNTIME_FAILURE = -2,
        XR_ERROR_OUT_OF_MEMORY = -3,
        XR_ERROR_API_VERSION_UNSUPPORTED = -4,
        XR_ERROR_INITIALIZATION_FAILED = -6,
        XR_ERROR_FUNCTION_UNSUPPORTED = -7,
        XR_ERROR_FEATURE_UNSUPPORTED = -8,
        XR_ERROR_EXTENSION_NOT_PRESENT = -9,
        XR_ERROR_LIMIT_REACHED = -10,
        XR_ERROR_SIZE_INSUFFICIENT = -11,
        XR_ERROR_HANDLE_INVALID = -12,
        XR_ERROR_INSTANCE_LOST = -13,
        XR_ERROR_SESSION_RUNNING = -14,
        XR_ERROR_SESSION_NOT_RUNNING = -16,
        XR_ERROR_SESSION_LOST = -17,
        XR_ERROR_SYSTEM_INVALID = -18,
        XR_ERROR_PATH_INVALID = -19,
        XR_ERROR_PATH_COUNT_EXCEEDED = -20,
        XR_ERROR_PATH_FORMAT_INVALID = -21,
        XR_ERROR_PATH_UNSUPPORTED = -22,
        XR_ERROR_LAYER_INVALID = -23,
        XR_ERROR_LAYER_LIMIT_EXCEEDED = -24,
        XR_ERROR_SWAPCHAIN_RECT_INVALID = -25,
        XR_ERROR_SWAPCHAIN_FORMAT_UNSUPPORTED = -26,
        XR_ERROR_ACTION_TYPE_MISMATCH = -27,
        XR_ERROR_SESSION_NOT_READY = -28,
        XR_ERROR_SESSION_NOT_STOPPING = -29,
        XR_ERROR_TIME_INVALID = -30,
        XR_ERROR_REFERENCE_SPACE_UNSUPPORTED = -31,
        XR_ERROR_FILE_ACCESS_ERROR = -32,
        XR_ERROR_FILE_CONTENTS_INVALID = -33,
        XR_ERROR_FORM_FACTOR_UNSUPPORTED = -34,
        XR_ERROR_FORM_FACTOR_UNAVAILABLE = -35,
        XR_ERROR_API_LAYER_NOT_PRESENT = -36,
        XR_ERROR_CALL_ORDER_INVALID = -37,
        XR_ERROR_GRAPHICS_DEVICE_INVALID = -38,
        XR_ERROR_POSE_INVALID = -39,
        XR_ERROR_INDEX_OUT_OF_RANGE = -40,
        XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED = -41,
        XR_ERROR_ENVIRONMENT_BLEND_MODE_UNSUPPORTED = -42,
        XR_ERROR_NAME_DUPLICATED = -44,
        XR_ERROR_NAME_INVALID = -45,
        XR_ERROR_ACTIONSET_NOT_ATTACHED = -46,
        XR_ERROR_ACTIONSETS_ALREADY_ATTACHED = -47,
        XR_ERROR_LOCALIZED_NAME_DUPLICATED = -48,
        XR_ERROR_LOCALIZED_NAME_INVALID = -49,
        XR_ERROR_GRAPHICS_REQUIREMENTS_CALL_MISSING = -50,
        XR_ERROR_RUNTIME_UNAVAILABLE = -51,
        XR_ERROR_ANDROID_THREAD_SETTINGS_ID_INVALID_KHR = -1000003000,
        XR_ERROR_ANDROID_THREAD_SETTINGS_FAILURE_KHR = -1000003001,
        XR_ERROR_CREATE_SPATIAL_ANCHOR_FAILED_MSFT = -1000039001,
        XR_ERROR_SECONDARY_VIEW_CONFIGURATION_TYPE_NOT_ENABLED_MSFT = -1000053000,
        XR_ERROR_CONTROLLER_MODEL_KEY_INVALID_MSFT = -1000055000,
        XR_ERROR_REPROJECTION_MODE_UNSUPPORTED_MSFT = -1000066000,
        XR_ERROR_COMPUTE_NEW_SCENE_NOT_COMPLETED_MSFT = -1000097000,
        XR_ERROR_SCENE_COMPONENT_ID_INVALID_MSFT = -1000097001,
        XR_ERROR_SCENE_COMPONENT_TYPE_MISMATCH_MSFT = -1000097002,
        XR_ERROR_SCENE_MESH_BUFFER_ID_INVALID_MSFT = -1000097003,
        XR_ERROR_SCENE_COMPUTE_FEATURE_INCOMPATIBLE_MSFT = -1000097004,
        XR_ERROR_SCENE_COMPUTE_CONSISTENCY_MISMATCH_MSFT = -1000097005,
        XR_ERROR_DISPLAY_REFRESH_RATE_UNSUPPORTED_FB = -1000101000,
        XR_ERROR_COLOR_SPACE_UNSUPPORTED_FB = -1000108000,
        XR_ERROR_UNEXPECTED_STATE_PASSTHROUGH_FB = -1000118000,
        XR_ERROR_FEATURE_ALREADY_CREATED_PASSTHROUGH_FB = -1000118001,
        XR_ERROR_FEATURE_REQUIRED_PASSTHROUGH_FB = -1000118002,
        XR_ERROR_NOT_PERMITTED_PASSTHROUGH_FB = -1000118003,
        XR_ERROR_INSUFFICIENT_RESOURCES_PASSTHROUGH_FB = -1000118004,
        XR_ERROR_UNKNOWN_PASSTHROUGH_FB = -1000118050,
        XR_ERROR_MARKER_NOT_TRACKED_VARJO = -1000124000,
        XR_ERROR_MARKER_ID_INVALID_VARJO = -1000124001,
        XR_ERROR_SPATIAL_ANCHOR_NAME_NOT_FOUND_MSFT = -1000142001,
        XR_ERROR_SPATIAL_ANCHOR_NAME_INVALID_MSFT = -1000142002,
        XR_ERROR_IMAGE_TARGET_TRACKING_MODE_INVALID_QCOM = -1000303000,
        XR_ERROR_OBJECT_TARGET_TRACKING_MODE_INVALID_QCOM = -1000304000,
        XR_ERROR_OBJECT_TRACKER_NOT_INITIALIZED_QCOM = -1000304001,
        XR_ERROR_OBJECT_TRACKER_DATA_SET_IN_USE_QCOM = -1000304002,
        XR_ERROR_PLANE_DETECTION_NOT_INITIALIZED_QCOM = -1000305000,
        XR_ERROR_RAY_CAST_TARGET_TYPE_INVALID_QCOM = -1000307000,
        XR_ERROR_RAY_MAX_DISTANCE_INVALID_QCOM = -1000307001,
        XR_ERROR_RAY_CAST_NOT_FINISHED_QCOM = -1000307002,
        XR_ERROR_CAMERA_ALREADY_IN_USE_QCOMX = -1000311000,
        XR_ERROR_FRAME_CONFIGURATION_INVALID_QCOMX = -1000311001,
        XR_ERROR_CONCURRENT_FRAME_CONFIGURATION_UNSUPPORTED_QCOMX = -1000311002,
        XR_ERROR_SPATIAL_ANCHOR_INSUFFICIENT_QUALITY_QCOM = -1000313000,
        XR_RESULT_MAX_ENUM = 0x7FFFFFFF
    }
}