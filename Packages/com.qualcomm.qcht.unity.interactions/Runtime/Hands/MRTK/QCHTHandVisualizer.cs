// /******************************************************************************
//  * File: QCHTHandVisualizer.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

#if UNITY_MRTK_3
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Subsystems;

namespace QCHT.Interactions.Hands.MRTK3
{
    public class QCHTHandVisualizer : MonoBehaviour
    {
        [SerializeField] protected XRNode handNode = XRNode.LeftHand;
        
        [SerializeField, Tooltip("Renderer of the hand mesh")] protected SkinnedMeshRenderer handRenderer;
        
        [SerializeField] protected Transform _handRoot;
        [SerializeField] protected Transform _thumb001;
        [SerializeField] protected Transform _thumb002;
        [SerializeField] protected Transform _thumb003;
        [SerializeField] protected Transform _index001;
        [SerializeField] protected Transform _index002;
        [SerializeField] protected Transform _index003;
        [SerializeField] protected Transform _middle001;
        [SerializeField] protected Transform _middle002;
        [SerializeField] protected Transform _middle003;
        [SerializeField] protected Transform _ring001;
        [SerializeField] protected Transform _ring002;
        [SerializeField] protected Transform _ring003;
        [SerializeField] protected Transform _pinky001;
        [SerializeField] protected Transform _pinky002;
        [SerializeField] protected Transform _pinky003;

        protected readonly Transform[] _riggedVisualJointsArray = new Transform[(int)TrackedHandJoint.TotalJoints];
        protected HandsAggregatorSubsystem _handsSubsystem;
        
        protected void Start()
        {
            LoadNodes();
        }
        
        protected void OnEnable()
        {
            handRenderer.enabled = false;
            Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand,
                $"HandVisualizer has an invalid XRNode ({handNode})!");

            _handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

            if (_handsSubsystem == null)
            {
                StartCoroutine(WaitForSubsystem());
            }
        }

        protected void OnDisable()
        {
            handRenderer.enabled = false;
        }
        
        protected IEnumerator WaitForSubsystem()
        {
            yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
            OnEnable();
        }
        
        protected void LoadNodes()
        {
            for (var i = 0; i < (int)TrackedHandJoint.TotalJoints; i++)
            {
                _riggedVisualJointsArray[i] = (TrackedHandJoint) i switch
                {
                    TrackedHandJoint.Wrist => _handRoot,
                    TrackedHandJoint.ThumbMetacarpal => _thumb001,
                    TrackedHandJoint.ThumbProximal => _thumb002,
                    TrackedHandJoint.ThumbDistal => _thumb003,
                    TrackedHandJoint.IndexProximal => _index001,
                    TrackedHandJoint.IndexIntermediate => _index002,
                    TrackedHandJoint.IndexDistal => _index003,
                    TrackedHandJoint.MiddleProximal => _middle001,
                    TrackedHandJoint.MiddleIntermediate => _middle002,
                    TrackedHandJoint.MiddleDistal => _middle003,
                    TrackedHandJoint.RingProximal => _ring001,
                    TrackedHandJoint.RingIntermediate => _ring002,
                    TrackedHandJoint.RingDistal => _ring003,
                    TrackedHandJoint.LittleProximal => _pinky001,
                    TrackedHandJoint.LittleIntermediate => _pinky002,
                    TrackedHandJoint.LittleDistal => _pinky003,
                    _ => _riggedVisualJointsArray[i]
                };
            }
        }

        protected void Update()
        {
            if (!ShouldRenderHand() ||
                !_handsSubsystem.TryGetEntireHand(handNode, out var joints))
            {
                handRenderer.enabled = false;
                return;
            }

            handRenderer.enabled = true;
            UpdateQCHTAvatar(joints);
        }
        

        protected void UpdateQCHTAvatar(IReadOnlyList<HandJointPose> joints)
        {
            for (var i = 0; i < (int) TrackedHandJoint.TotalJoints; i++)
            {
                var jointTransform = _riggedVisualJointsArray[i];
                var jointPose = joints[i];

                if (jointTransform != null)
                {
                    switch ((TrackedHandJoint) i)
                    {
                        case TrackedHandJoint.Wrist:
                            jointTransform.position = jointPose.Position;
                            jointTransform.rotation = jointPose.Rotation;
                            break;
                        case TrackedHandJoint.IndexMetacarpal:
                        case TrackedHandJoint.MiddleMetacarpal:
                        case TrackedHandJoint.RingMetacarpal:
                        case TrackedHandJoint.LittleMetacarpal:
                        case TrackedHandJoint.Palm:
                            break;
                        default:
                            jointTransform.rotation = jointPose.Rotation;
                            break;
                    }
                }
            }
        }
        
        protected bool ShouldRenderHand()
        {
            return _handsSubsystem != null && _handRoot != null && handRenderer != null;
        }
    }
}
#endif