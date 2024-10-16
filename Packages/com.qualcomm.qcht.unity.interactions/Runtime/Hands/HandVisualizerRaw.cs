// /******************************************************************************
//  * File: HandVisualizerRaw.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Hands
{
    /// <summary>
    /// Instantiates a mesh at each hand joints
    /// This class is commonly used to debug hand tracking
    /// </summary>
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_LineVisual)]
    public class HandVisualizerRaw : MonoBehaviour
    {
        [SerializeField, Tooltip("Mesh that will be instanced for each hand joint")]
        protected Mesh mesh;
        [SerializeField, Tooltip("Mesh scale for each hand joints"), Range(0.005f, 1f)]
        protected float scale = 0.01f;
        [SerializeField, Tooltip("Should enable GPU instancing for performance")]
        protected Material material;
        [SerializeField, Tooltip("Use hand tracking normalized colors")]
        protected bool useNormalizedColors = true;

        /// <summary>
        /// Point instances matrices
        /// </summary>
        protected readonly Matrix4x4[] _pointsMatrix = new Matrix4x4[(int) XrHandJoint.XR_HAND_JOINT_MAX * 2];

        /// <summary>
        /// Tweak color in property blocks for GPU instancing.
        /// Used only when UseNormalized colors is checked
        /// </summary>
        protected readonly MaterialPropertyBlock[] _propertyBlock =
            new MaterialPropertyBlock[(int) XrHandJoint.XR_HAND_JOINT_MAX * 2];

        protected XRHandTrackingSubsystem _subsystem;

        private static readonly int s_color = Shader.PropertyToID("_Color");

        public float Scale
        {
            get => scale;
            set => scale = value;
        }

        #region MonoBehaviour Functions

        protected void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRenderUpdate;
        }

        protected void OnDisable()
        {
            if (_subsystem != null)
            {
                _subsystem.OnHandsUpdated -= OnHandsUpdated;
                _subsystem = null;
            }

            Application.onBeforeRender -= OnBeforeRenderUpdate;
        }

        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderLineVisual)]
        protected void OnBeforeRenderUpdate()
        {
            FindXRHandTrackingSubsystem();

            if (_subsystem != null && _subsystem.running)
            {
                DrawInstancedJoints();
            }
        }

        /// <summary>
        /// Draws joints in one batch or using GPU instancing instead
        /// </summary>
        protected void DrawInstancedJoints()
        {
            if (useNormalizedColors)
            {
                for (var i = 0; i < _pointsMatrix.Length; i++)
                {
                    _propertyBlock[i] ??= new MaterialPropertyBlock();
                    var colorId = i % (int) XrHandJoint.XR_HAND_JOINT_MAX;
                    _propertyBlock[i].SetColor(s_color, GetColorForJoint((XrHandJoint) colorId));
                    // Draws instance by instance but will be merge if the GPU instancing is enabled
                    Graphics.DrawMesh(mesh, _pointsMatrix[i], material, 0, null, 0, _propertyBlock[i]);
                }
            }
            else
            {
                // Directly renders instances in batch
                Graphics.DrawMeshInstanced(mesh, 0, material, _pointsMatrix, _pointsMatrix.Length);
            }
        }

        /// <summary>
        /// Attempts to find the Hand tracking subsystem and registers to its callbacks
        /// </summary>
        private void FindXRHandTrackingSubsystem()
        {
            if (_subsystem != null)
                return;

            _subsystem = XRHandTrackingSubsystem.GetSubsystemInManager();

            if (_subsystem != null)
            {
                _subsystem.OnHandsUpdated += OnHandsUpdated;
            }
        }

        #endregion

        protected void OnHandsUpdated(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            for (var i = 0; i < _pointsMatrix.Length; i++)
            {
                var pose = Pose.identity;
                var s = Vector3.zero;
                var hand = i < (int) XrHandJoint.XR_HAND_JOINT_MAX ? _subsystem.LeftHand : _subsystem.RightHand;
                var id = i % (int) XrHandJoint.XR_HAND_JOINT_MAX;

                if (hand.IsTracked)
                {
                    pose = hand.Joints[id];
                    s = Vector3.one * (scale * hand.Scale);
                }

                XROriginUtility.TransformPose(ref pose, true);

                _pointsMatrix[i].SetTRS(pose.position, pose.rotation, s);
            }
        }

        public static Color GetColorForJoint(XrHandJoint joint) => joint switch
        {
            XrHandJoint.XR_HAND_JOINT_WRIST => s_handColors[0],
            XrHandJoint.XR_HAND_JOINT_PALM => s_handColors[1],
            >= XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL and <= XrHandJoint.XR_HAND_JOINT_THUMB_TIP => s_handColors[2],
            >= XrHandJoint.XR_HAND_JOINT_INDEX_METACARPAL and <= XrHandJoint.XR_HAND_JOINT_INDEX_TIP => s_handColors[3],
            >= XrHandJoint.XR_HAND_JOINT_MIDDLE_METACARPAL and <= XrHandJoint.XR_HAND_JOINT_MIDDLE_TIP => s_handColors
                [4],
            >= XrHandJoint.XR_HAND_JOINT_RING_METACARPAL and <= XrHandJoint.XR_HAND_JOINT_RING_TIP => s_handColors[5],
            >= XrHandJoint.XR_HAND_JOINT_LITTLE_METACARPAL and <= XrHandJoint.XR_HAND_JOINT_LITTLE_TIP => s_handColors
                [6],
            _ => Color.white
        };

        private static readonly Color[] s_handColors =
        {
            Color.white, // Wrist
            new Color(159f / 255f, 72f / 255f, 0f), // Palm
            new Color(232f / 255f, 0f / 255f, 11f / 255f), // Thumb
            new Color(255f / 255f, 196f / 255f, 0f / 255f), // Index
            new Color(26f / 255f, 201f / 255f, 56f / 255f), // Middle
            new Color(0f / 255f, 215f / 255f, 255f / 255f), // Ring
            new Color(139f / 255f, 43f / 255f, 226f / 255f), // Pinky
        };
    }
}