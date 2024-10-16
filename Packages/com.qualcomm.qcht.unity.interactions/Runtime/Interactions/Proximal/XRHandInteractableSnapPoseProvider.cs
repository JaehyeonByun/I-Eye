// /******************************************************************************
//  * File: XRHandInteractableSnapPoseProvider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using QCHT.Interactions.Core;
using QCHT.Interactions.Hands;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace QCHT.Interactions.Proximal
{
    [DisallowMultipleComponent]
    public class XRHandInteractableSnapPoseProvider : XRBaseGrabTransformer
    {
        [SerializeField] private XRGrabInteractable interactable;
        [SerializeField] private XrHandedness handedness;
        [SerializeField] private HandMask mask;
        [SerializeField] private List<XRHandInteractableSnapPose> poses;

        public List<XRHandInteractableSnapPose> Poses
        {
            get => poses;
            set =>  poses = value;
        }
        
        public XrHandedness Handedness => handedness;

        private XRHandTrackingSubsystem _subsystem;
        private XRHandInteractableSnapPoseManager _snapPoseManager;
        private XRDirectInteractor _interactor;

        protected void OnEnable()
        {
            if (!TryFindInteractable())
            {
                enabled = false;
                return;
            }

            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);

            FindCreateSnapPoseManager();
            SanitizePoseList();
        }

        protected void OnDisable()
        {
            ReleasePose();

            if (interactable)
            {
                interactable.selectEntered.RemoveListener(OnSelectEntered);
                interactable.selectExited.RemoveListener(OnSelectExited);
            }
        }

        protected void Update()
        {
            FindHandTrackingSubsystem();

            if (_interactor != null)
            {
                UpdateSnapPose();
            }
        }

        private bool TryFindInteractable()
        {
            interactable = interactable ? interactable : GetComponentInParent<XRGrabInteractable>(true);

            if (interactable == null)
            {
                Debug.LogWarning(
                    "[XRHandInteractableSnapPoseProvider:TryFindInteractable] Unable to find interactable attached to snap pose provider");
                return false;
            }

            if (interactable.useDynamicAttach)
                Debug.LogWarning(
                    "[XRHandInteractableSnapPoseProvider:TryFindInteractable] Using dynamic attach on interactable with snap pose provider may not work as expected.");

            return interactable;
        }

        private void FindCreateSnapPoseManager()
        {
            if (_snapPoseManager != null)
            {
                return;
            }

            _snapPoseManager = FindObjectOfType<XRHandInteractableSnapPoseManager>();

            if (_snapPoseManager == null)
            {
                _snapPoseManager = new GameObject(nameof(XRHandInteractableSnapPoseManager),
                    typeof(XRHandInteractableSnapPoseManager)).GetComponent<XRHandInteractableSnapPoseManager>();
            }
        }

        private void FindHandTrackingSubsystem()
        {
            if (_subsystem != null)
            {
                return;
            }

            _subsystem = XRHandTrackingSubsystem.GetSubsystemInManager();
        }

        public void FindPoses()
        {
            poses = GetComponentsInChildren<XRHandInteractableSnapPose>().ToList();
        }
        
        public void SanitizePoseList()
        {
            var validPoses = poses.Where(pose => pose != null).ToList();
            validPoses.Sort((s1, s2) => s1.Scale.CompareTo(s2.Scale));
            poses = validPoses;
        }
        
        #region XR Grab Interactable callbacks

        protected void OnSelectEntered(SelectEnterEventArgs args)
        {
            FindCreateSnapPoseManager();

            var interactor = args.interactorObject as XRDirectInteractor;
            if (interactor == null)
                return;

            var handed = interactor.xrController.GetComponentInParent<IHandedness>();
            if (handed == null || handed.Handedness != handedness)
                return;

            UpdateSnapPose();
            interactable.AddSingleGrabTransformer(this);

            _interactor = interactor;
        }

        protected void OnSelectExited(SelectExitEventArgs args)
        {
            var interactor = args.interactorObject as XRDirectInteractor;
            if (interactor == null || interactor != _interactor)
                return;

            ReleasePose();
            interactable.RemoveSingleGrabTransformer(this);

            _interactor = null;
        }

        protected void UpdateSnapPose()
        {
            if (_subsystem == null || !_subsystem.running)
                return;

            var interpolatedHandPose = new HandData();
            var interpolatedRootPose = new Pose();

            if (TryGetInterpolatedHandPoseFromScale(ref interpolatedHandPose, ref interpolatedRootPose, hand.Scale))
            {
                ApplyPose(interpolatedHandPose);
            }
        }

        protected void ApplyPose(HandData pose)
        {
            if (_snapPoseManager == null)
            {
                return;
            }

            _snapPoseManager.SetHandPose(handedness, pose, mask, null);
        }

        protected void ReleasePose()
        {
            if (_snapPoseManager == null)
                return;

            _snapPoseManager.SetHandPose(handedness, null, null, null);
        }

        internal bool TryGetInterpolatedHandPoseFromScale(ref HandData handData, ref Pose rootPose, float scale)
        {
            if (poses == null || poses.Count == 0)
                return false;

            rootPose = Pose.identity;
            
            Vector3 lPos;
            Quaternion lRot;
            
            if (poses.Count == 1)
            {
                handData = poses[0].Data;
                lRot = poses[0].transform.localRotation;
                lPos = poses[0].transform.localPosition;
            }
            else
            {
                var i1 = 0;
                for (var i = 0; i <= poses.Count - 2; i++)
                {
                    if (scale > poses[i].Scale)
                    {
                        i1 = i;
                    }
                }
                
                var i2 = i1 + 1;
                var t = (scale - poses[i1].Scale) / (poses[i2].Scale - poses[i1].Scale);
                if (t > Mathf.Epsilon)
                {
                    handData = HandData.Lerp(poses[i1].Data, poses[i2].Data, t);
                
                    lPos = Vector3.Lerp(poses[i1].transform.localPosition, poses[i2].transform.localPosition, t);
                    lRot = Quaternion.Lerp(poses[i1].transform.localRotation, poses[i2].transform.localRotation, t);
                }
                else
                {
                    handData = poses[i1].Data;
                
                    lPos = poses[i1].transform.localPosition;
                    lRot = poses[i1].transform.localRotation;
                }
            }
            
            rootPose.position = lPos;
            rootPose.rotation = lRot;
            
            return true;
        }

        #endregion

        #region Transformer

        private Transform _origin;
        private Transform _cameraOffset;

        private XRHandTrackingSubsystem.Hand hand =>
            handedness == XrHandedness.XR_HAND_LEFT ? _subsystem.LeftHand : _subsystem.RightHand;

        public override void Process(XRGrabInteractable grabInteractable,
            XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    UpdateTarget(grabInteractable, ref targetPose);
                    break;
            }
        }

        private void UpdateTarget(XRGrabInteractable grabInteractable, ref Pose targetPose)
        {
            FindHandTrackingSubsystem();

            if (_subsystem == null || !_subsystem.running)
                return;

            _origin = _origin != null ? _origin : XROriginUtility.GetOriginTransform();

            if (_cameraOffset == null)
            {
                var cameraFloorOffsetObject = XROriginUtility.GetCameraFloorOffsetObject();
                if (cameraFloorOffsetObject)
                {
                    _cameraOffset = cameraFloorOffsetObject.transform;
                }
            }

            var interpolatedHandPose = new HandData();
            var interpolatedRootPose = new Pose();

            if (!TryGetInterpolatedHandPoseFromScale(ref interpolatedHandPose, ref interpolatedRootPose, hand.Scale))
            {
                return;
            }

            var rootPose = hand.Root;
            
            if (_origin)
            {
                rootPose = _origin.TransformPose(rootPose);
            }

            if (_cameraOffset)
            {
                rootPose.position += _cameraOffset.localPosition;
            }
            
            rootPose.rotation *= Quaternion.AngleAxis(90f, Vector3.right);

            var posOffset = interpolatedRootPose.position;
            posOffset.Scale(grabInteractable.transform.lossyScale);

            targetPose.rotation = rootPose.rotation * Quaternion.Inverse(interpolatedRootPose.rotation);
            targetPose.position = rootPose.position + rootPose.rotation * (Quaternion.Inverse(interpolatedRootPose.rotation) * -posOffset);
        }

        #endregion

#if UNITY_EDITOR
        protected void Reset()
        {
            TryFindInteractable();
            FindPoses();
        }

        protected void OnValidate()
        {
            TryFindInteractable();
            FindPoses();
        }
#endif
    }
}