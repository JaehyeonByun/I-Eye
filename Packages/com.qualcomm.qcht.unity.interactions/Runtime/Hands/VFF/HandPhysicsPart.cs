// /******************************************************************************
//  * File: HandPhysicsPart.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using QCHT.Interactions.Core;

namespace QCHT.Interactions.Hands.VFF
{
    public class HandPhysicsPart : HandJointUpdater, IHandPhysible
    {
        public bool IsRoot;
        public bool IsThumb;
        public HandPhysicsPart ConnectedPhysicsPart;
        [Space]
        public Collider Collider;
        public Rigidbody ConnectedRigidBody;

        private Rigidbody _rigidbody;
        private ConfigurableJoint _joint;

        private Transform _centerOfMass;
        private Transform _anchor;
        private Transform _connectedAnchor;

        public HandsPhysicsBoneConfiguration Configuration { get; set; }

        private bool _initialized;

        #region MonoBehaviour Functions

        private void Start()
        {
            if (ConnectedPhysicsPart)
                Physics.IgnoreCollision(ConnectedPhysicsPart.Collider, Collider);
        }

        private void FixedUpdate()
        {
            if (!IsPhysible)
                return;

            if (!_rigidbody)
                TryGetRigidBody();

            if (!_joint.connectedBody)
                TryGetConnectedBody();

            if (_centerOfMass && _rigidbody)
            {
                var localCenterOfMass = _rigidbody.transform.InverseTransformPoint(_centerOfMass.position);
                if (_rigidbody.centerOfMass != localCenterOfMass) _rigidbody.centerOfMass = localCenterOfMass;
            }

            if (_connectedAnchor)
            {
                var tConnect = _joint.connectedBody
                    ? _joint.connectedBody.transform.InverseTransformPoint(_connectedAnchor.position)
                    : _connectedAnchor.position;

                _joint.connectedAnchor = tConnect;
            }

            if (_joint)
            {
                _joint.autoConfigureConnectedAnchor = false;

                // Target Rotation
                if (_joint.configuredInWorldSpace)
                    _joint.SetTargetRotation(_connectedAnchor.rotation, _anchor.rotation);
                else
                    _joint.SetTargetRotationLocal(_connectedAnchor.rotation, _anchor.rotation);

                // Target position
                _joint.targetPosition = Vector3.zero;
            }
        }

        #endregion

        public override void UpdateJoint(XrSpace space, BoneData data)
        {
            if (!IsPhysible)
            {
                base.UpdateJoint(space, data);
                return;
            }

            if (space == XrSpace.XR_HAND_WORLD)
            {
                if (data.UpdatePosition)
                    _connectedAnchor.position = data.Position;

                if (data.UpdateRotation)
                    _connectedAnchor.rotation = data.Rotation;
            }
            else
            {
                if (data.UpdatePosition)
                    _connectedAnchor.localPosition = data.Position;

                if (data.UpdateRotation)
                    _connectedAnchor.localRotation = data.Rotation;
            }
        }

        public void TriggerCollisionsDetection(bool enable)
        {
            if (!_rigidbody)
                return;

            _rigidbody.detectCollisions = enable;
        }

        private void InitConfigurableJoint()
        {
            if (_initialized)
                return;

            _joint = gameObject.AddComponent<ConfigurableJoint>();
            _joint.massScale = Configuration.JointMassScale;
            _joint.connectedMassScale = Configuration.JointConnectedMassScale;
            _joint.rotationDriveMode = RotationDriveMode.Slerp;
            _joint.autoConfigureConnectedAnchor = false;

            var joint = GetComponent<ConfigurableJoint>();

            // Angular
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = Configuration.AngularMotion;
            joint.angularXDrive = joint.angularYZDrive = Configuration.AngularDrive.ToJointDrive;
            joint.slerpDrive = Configuration.AngularDrive.ToJointDrive;

            //Motion
            joint.xMotion = joint.yMotion = joint.zMotion = Configuration.LinearMotion;
            joint.xDrive = joint.yDrive = joint.zDrive = Configuration.MotionDrive.ToJointDrive;

            // Control
            joint.configuredInWorldSpace = true;

            InstantiateAnchor();
            InstantiateConnectedAnchor();

            _initialized = true;
        }

        private void TryGetRigidBody()
        {
            if (!_rigidbody && !TryGetComponent(out _rigidbody))
                return;

            _rigidbody.mass = Configuration.RigidbodyMass;
            _rigidbody.drag = Configuration.RigidbodyDrag;
            _rigidbody.angularDrag = Configuration.RigidbodyAngularDrag;
            _rigidbody.useGravity = Configuration.UseGravity;
            InstantiateCenterOfMass();
        }

        private void TryGetConnectedBody()
        {
            var connectedRigidBody = ConnectedRigidBody;
            if (!connectedRigidBody)
                if (ConnectedPhysicsPart)
                    ConnectedPhysicsPart.TryGetComponent(out connectedRigidBody);

            _joint.connectedBody = connectedRigidBody;
            _joint.enableCollision = false;
            _joint.enablePreprocessing = false;
        }

        private void InstantiateCenterOfMass()
        {
            var centerOfMass = new GameObject(name + ".CenterOfMass").transform;
            centerOfMass.SetParent(transform);
            centerOfMass.localPosition = _rigidbody.centerOfMass;
            _centerOfMass = centerOfMass;
        }

        private void InstantiateAnchor()
        {
            var anchor = new GameObject(name + ".Anchor").transform;
            var jointTransform = _joint.transform;
            anchor.position = jointTransform.position;
            anchor.rotation = jointTransform.rotation;
            anchor.SetParent(jointTransform);
            _anchor = anchor;
        }

        private void InstantiateConnectedAnchor()
        {
            var connectedAnchor = new GameObject(name + ".ConnectedAnchor").transform;
            var jointTransform = _joint.transform;
            connectedAnchor.SetParent(ConnectedPhysicsPart
                ? ConnectedPhysicsPart.transform
                : jointTransform.parent);
            connectedAnchor.position = jointTransform.position;
            connectedAnchor.rotation = jointTransform.rotation;
            _connectedAnchor = connectedAnchor;
        }

        private bool _isPhysible;

        public bool IsPhysible
        {
            get => _isPhysible;
            set
            {
                _isPhysible = value;
                Collider.isTrigger = !value;

                if (_isPhysible)
                    InitConfigurableJoint();
                else
                {
                    if (_joint != null)
                        Destroy(_joint);

                    if (_rigidbody != null)
                        Destroy(_rigidbody);
                }
            }
        }
    }

    public static class ConfigurableJointExtensions
    {
        public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation,
            Quaternion startLocalRotation)
        {
            if (joint.configuredInWorldSpace)
                return;

            SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
        }

        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation,
            Quaternion startWorldRotation)
        {
            if (!joint.configuredInWorldSpace)
                return;

            SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
        }

        private static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation,
            Quaternion startRotation, Space space)
        {
            var axis = joint.axis;
            var forward = Vector3.Cross(axis, joint.secondaryAxis).normalized;
            var up = Vector3.Cross(forward, axis).normalized;
            var worldToJointSpace = Quaternion.LookRotation(forward, up);
            var resultRotation = Quaternion.Inverse(worldToJointSpace);

            if (space == Space.World)
                resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
            else
                resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;

            resultRotation *= worldToJointSpace;
            joint.targetRotation = resultRotation;
        }
    }
}