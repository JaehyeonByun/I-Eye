// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace MixedReality.Toolkit.SpatialManipulation
{
    [RequireComponent(typeof(HandBounds))]
    [AddComponentMenu("MRTK/Spatial Manipulation/Solvers/Hand Constraint")]
    public class HandConstraint : Solver
    {
        private static readonly SolverSafeZone[] handSafeZonesClockWiseRightHand =
            new SolverSafeZone[]
            {
                SolverSafeZone.UlnarSide,
                SolverSafeZone.AboveFingerTips,
                SolverSafeZone.RadialSide,
                SolverSafeZone.BelowWrist
            };

        [Header("Hand Constraint")]
        [SerializeField]
        [Tooltip("Which part of the hand to move the solver towards. The ulnar side of the hand is recommended for most situations.")]
        private SolverSafeZone safeZone = SolverSafeZone.UlnarSide;
        public SolverSafeZone SafeZone
        {
            get => safeZone;
            set => safeZone = value;
        }

        [SerializeField]
        [Tooltip("Additional offset to apply to the intersection point with the hand bounds along the intersection point normal.")]
        private float safeZoneBuffer = 0.15f;
        public float SafeZoneBuffer
        {
            get => safeZoneBuffer;
            set => safeZoneBuffer = value;
        }

        [SerializeField]
        [Tooltip("Should the solver continue to move when the opposite hand (hand which is not being tracked) is near the tracked hand. This can improve stability when one hand occludes the other.")]
        private bool updateWhenOppositeHandNear = false;
        public bool UpdateWhenOppositeHandNear
        {
            get => updateWhenOppositeHandNear;
            set => updateWhenOppositeHandNear = value;
        }

        [SerializeField]
        [Tooltip("When a hand is activated for tracking, should the cursor(s) be disabled on that hand?")]
        private bool hideHandCursorsOnActivate = true;
        public bool HideHandCursorsOnActivate
        {
            get => hideHandCursorsOnActivate;
            set => hideHandCursorsOnActivate = value;
        }

        [SerializeField]
        [Tooltip("Specifies how the solver should rotate when tracking the hand.")]
        private SolverRotationBehavior rotationBehavior = SolverRotationBehavior.LookAtMainCamera;
        public SolverRotationBehavior RotationBehavior
        {
            get => rotationBehavior;
            set => rotationBehavior = value;
        }

        [SerializeField]
        [Tooltip("Specifies how the solver's offset relative to the hand will be computed.")]
        private SolverOffsetBehavior offsetBehavior = SolverOffsetBehavior.LookAtCameraRotation;
        public SolverOffsetBehavior OffsetBehavior
        {
            get => offsetBehavior;
            set => offsetBehavior = value;
        }

        [SerializeField]
        [Tooltip("Additional offset to apply towards the user.")]
        private float forwardOffset = 0;
        public float ForwardOffset
        {
            get => forwardOffset;
            set => forwardOffset = value;
        }

        [SerializeField]
        [Tooltip("Additional degree offset to apply from the stated SafeZone. Ignored if Safe Zone is Atop Palm." +
        " Direction is clockwise on the left hand and anti-clockwise on the right hand.")]
        private float safeZoneAngleOffset = 0;
        public float SafeZoneAngleOffset
        {
            get => safeZoneAngleOffset;
            set => safeZoneAngleOffset = value;
        }

        [SerializeField]
        [Tooltip("Event which is triggered when zero hands to one hand is tracked.")]
        private UnityEvent onFirstHandDetected = new UnityEvent();
        public UnityEvent OnFirstHandDetected
        {
            get => onFirstHandDetected;
            set => onFirstHandDetected = value;
        }

        [SerializeField]
        [Tooltip("Event which is triggered when all hands are lost.")]
        private UnityEvent onLastHandLost = new UnityEvent();
        public UnityEvent OnLastHandLost
        {
            get => onLastHandLost;
            set => onLastHandLost = value;
        }

        [SerializeField]
        [Tooltip("Event which is triggered when a hand begins being tracked.")]
        private UnityEvent onHandActivate = new UnityEvent();
        public UnityEvent OnHandActivate
        {
            get => onHandActivate;
            set => onHandActivate = value;
        }

        [SerializeField]
        [Tooltip("Event which is triggered when a hand stops being tracked.")]
        private UnityEvent onHandDeactivate = new UnityEvent();
        public UnityEvent OnHandDeactivate
        {
            get => onHandDeactivate;
            set => onHandDeactivate = value;
        }

        private Handedness previousHandedness = Handedness.None;
        public Handedness Handedness => previousHandedness;

        private XRNode? trackedNode = null;

        private HandBounds handBounds = null;
        protected HandBounds HandBounds => handBounds;

        private readonly Quaternion handToWorldRotation = Quaternion.Euler(-90.0f, 0.0f, 180.0f);

        private static readonly ProfilerMarker SolverUpdatePerfMarker =
            new ProfilerMarker("[MRTK] HandConstraint.SolverUpdate");
        public override void SolverUpdate()
        {
            using (SolverUpdatePerfMarker.Auto())
            {
                if (SolverHandler.TrackedTargetType != TrackedObjectType.HandJoint &&
                    SolverHandler.TrackedTargetType != TrackedObjectType.ControllerRay)
                {
                    return;
                }

                XRNode? prevTrackedNode = trackedNode;

                if (SolverHandler.CurrentTrackedHandedness != Handedness.None)
                {
                    trackedNode = GetControllerNode(SolverHandler.CurrentTrackedHandedness);
                    bool isValidController = IsValidController(trackedNode);
                    if (!isValidController)
                    {
                        // Attempt to switch hands by asking solver handler to prefer the other controller if available
                        SolverHandler.PreferredTrackedHandedness = SolverHandler.CurrentTrackedHandedness.GetOppositeHandedness();
                        SolverHandler.RefreshTrackedObject();

                        trackedNode = GetControllerNode(SolverHandler.CurrentTrackedHandedness);
                        isValidController = IsValidController(trackedNode);
                        if (!isValidController)
                        {
                            trackedNode = null;
                        }
                    }

                    if (isValidController && SolverHandler.TransformTarget != null)
                    {
                        if (updateWhenOppositeHandNear || !IsOppositeHandNear(trackedNode))
                        {
                            GoalPosition = CalculateGoalPosition();
                            GoalRotation = CalculateGoalRotation();
                        }
                    }
                }
                else
                {
                    trackedNode = null;
                }

                // Calculate if events should be fired
                Handedness newHandedness = trackedNode.HasValue ? trackedNode.Value.ToHandedness() : Handedness.None;
                if (previousHandedness == Handedness.None && newHandedness != Handedness.None)
                {
                    previousHandedness = newHandedness;
                    OnFirstHandDetected.Invoke();
                    OnHandActivate.Invoke();
                }
                else if (previousHandedness != Handedness.None && newHandedness == Handedness.None)
                {
                    previousHandedness = newHandedness;
                    OnLastHandLost.Invoke();
                    OnHandDeactivate.Invoke();
                }
                else if (previousHandedness != newHandedness)
                {
                    OnHandDeactivate.Invoke();
                    previousHandedness = newHandedness;
                    OnHandActivate.Invoke();
                }

                UpdateWorkingPositionToGoal();
                UpdateWorkingRotationToGoal();
            }
        }
        protected virtual bool IsValidController(XRNode? hand)
        {
            return (hand.HasValue &&
                ((hand.Value == XRNode.LeftHand) || (hand.Value == XRNode.RightHand)));
        }

        private static readonly ProfilerMarker CalculateGoalPositionPerfMarker =
            new ProfilerMarker("[MRTK] HandConstraint.CalculateGoalPosition");
        protected virtual Vector3 CalculateGoalPosition()
        {
            using (CalculateGoalPositionPerfMarker.Auto())
            {
                Vector3 goalPosition = SolverHandler.TransformTarget.position;

                if (trackedNode.HasValue &&
                    handBounds.LocalBounds.TryGetValue(trackedNode.Value.ToHandedness(), out Bounds trackedHandBounds))
                {
                    HandJointPose? palmPose = GetPalmPose(trackedNode);
                    if (palmPose.HasValue == false)
                    {
                        return goalPosition;
                    }

                    Ray ray = CalculateGoalPositionRay(
                        goalPosition,
                        SolverHandler.TransformTarget,
                        trackedNode,
                        safeZone,
                        OffsetBehavior,
                        safeZoneAngleOffset);
                    trackedHandBounds.Expand(safeZoneBuffer);
                    ray.origin = Quaternion.Inverse(palmPose.Value.Rotation) * (ray.origin - palmPose.Value.Position);
                    ray.direction = Quaternion.Inverse(palmPose.Value.Rotation) * ray.direction;

                    if (trackedHandBounds.IntersectRay(ray, out float distance))
                    {
                        var localSpaceHit = ray.origin + ray.direction * distance;
                        if (palmPose.HasValue)
                        {
                            goalPosition = palmPose.Value.Rotation * (localSpaceHit) + palmPose.Value.Position;
                            Vector3 goalToCam = Camera.main.transform.position - goalPosition;
                            if (goalToCam.magnitude > Mathf.Epsilon)
                            {
                                goalPosition += (goalToCam).normalized * ForwardOffset;
                            }
                        }
                    }
                }

                return goalPosition;
            }
        }

        private static readonly ProfilerMarker CalculateGoalRotationPerfMarker =
            new ProfilerMarker("[MRTK] HandConstraint.CalculateGoalRotation");
        protected virtual Quaternion CalculateGoalRotation()
        {
            using (CalculateGoalRotationPerfMarker.Auto())
            {
                Quaternion goalRotation = SolverHandler.TransformTarget.rotation;

                switch (rotationBehavior)
                {
                    case SolverRotationBehavior.LookAtMainCamera:
                        {
                            goalRotation = Quaternion.LookRotation(GoalPosition - Camera.main.transform.position);
                        }
                        break;

                    case SolverRotationBehavior.LookAtTrackedObject:
                        {
                            goalRotation *= handToWorldRotation;
                        }
                        break;
                }

                if (rotationBehavior != SolverRotationBehavior.None)
                {
                    var additionalRotation = SolverHandler.AdditionalRotation;
                    if (trackedNode.Value == XRNode.RightHand)
                    {
                        additionalRotation.y *= -1.0f;
                    }

                    goalRotation *= Quaternion.Euler(additionalRotation.x, additionalRotation.y, additionalRotation.z);
                }

                return goalRotation;
            }
        }

        private static readonly ProfilerMarker IsOppositeHandNearPerfMarker =
            new ProfilerMarker("[MRTK] HandConstraint.IsOppositeHandNear");
        protected virtual bool IsOppositeHandNear(XRNode? hand)
        {
            using (IsOppositeHandNearPerfMarker.Auto())
            {
                if (IsValidController(hand))
                {
                    Handedness handedness = hand.Value.ToHandedness();

                    if (handBounds.GlobalBounds.TryGetValue(handedness.GetOppositeHandedness(), out Bounds oppositeHandBounds) &&
                        handBounds.GlobalBounds.TryGetValue(handedness, out Bounds trackedHandBounds))
                    {
                        // Double the size of the hand bounds to allow for greater tolerance.
                        trackedHandBounds.Expand(trackedHandBounds.extents);
                        oppositeHandBounds.Expand(oppositeHandBounds.extents);

                        if (trackedHandBounds.Intersects(oppositeHandBounds))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private static readonly ProfilerMarker CalculateRayForSafeZonePerfMarker =
            new ProfilerMarker("[MRTK] HandConstraint.CalculateRayForSafeZone");

        private Ray CalculateRayForSafeZone(
            Vector3 origin,
            Transform targetTransform,
            XRNode? hand,
            SolverSafeZone handSafeZone,
            SolverOffsetBehavior offsetBehavior,
            float angleOffset = 0)
        {
            using (CalculateRayForSafeZonePerfMarker.Auto())
            {
                Debug.Assert(hand.HasValue);

                Vector3 direction;
                Vector3 lookAtCamera = targetTransform.transform.position - Camera.main.transform.position;

                switch (handSafeZone)
                {
                    default:
                    case SolverSafeZone.UlnarSide:
                        {
                            if (offsetBehavior == SolverOffsetBehavior.TrackedObjectRotation)
                            {
                                direction = targetTransform.right;
                            }
                            else
                            {
                                direction = Vector3.Cross(lookAtCamera, Vector3.up);
                                direction = IsPalmFacingCamera(hand) ? direction : -direction;
                            }

                            if (hand == XRNode.LeftHand)
                            {
                                direction = -direction;
                            }
                        }
                        break;

                    case SolverSafeZone.RadialSide:
                        {

                            if (offsetBehavior == SolverOffsetBehavior.TrackedObjectRotation)
                            {
                                direction = -targetTransform.right;
                            }
                            else
                            {
                                direction = Vector3.Cross(lookAtCamera, Vector3.up);
                                direction = IsPalmFacingCamera(hand) ? direction : -direction;
                            }

                            if (hand == XRNode.RightHand)
                            {
                                direction = -direction;
                            }
                        }
                        break;

                    case SolverSafeZone.AboveFingerTips:
                        {
                            if (offsetBehavior == SolverOffsetBehavior.TrackedObjectRotation)
                            {
                                direction = targetTransform.forward;
                            }
                            else
                            {
                                direction = Camera.main.transform.up;
                            }
                        }
                        break;

                    case SolverSafeZone.BelowWrist:
                        {
                            if (offsetBehavior == SolverOffsetBehavior.TrackedObjectRotation)
                            {
                                direction = -targetTransform.forward;
                            }
                            else
                            {
                                direction = -Camera.main.transform.up;
                            }
                        }
                        break;

                    case SolverSafeZone.AtopPalm:
                        {
                            HandJointPose? palmPose = GetPalmPose(hand);
                            if (palmPose.HasValue)
                            {
                                direction = Quaternion.AngleAxis((hand.Value == XRNode.LeftHand) ? angleOffset : -angleOffset, palmPose.Value.Forward) * -palmPose.Value.Up;
                            }
                            else
                            {
                                direction = -lookAtCamera;
                            }
                        }
                        break;
                }

                return new Ray(origin + direction, -direction);
            }
        }

        private static readonly ProfilerMarker CalculateGoalPositionRayPerfMarker =
            new ProfilerMarker("[MRTK] HandConstraint.CalculateGoalPositionRay");
        private Ray CalculateGoalPositionRay(
            Vector3 origin,
            Transform targetTransform,
            XRNode? hand,
            SolverSafeZone handSafeZone,
            SolverOffsetBehavior offsetBehavior,
            float angleOffset)
        {
            using (CalculateGoalPositionRayPerfMarker.Auto())
            {
                Debug.Assert(hand.HasValue);

                if (angleOffset == 0)
                {
                    return CalculateRayForSafeZone(
                        origin,
                        targetTransform,
                        hand,
                        handSafeZone,
                        offsetBehavior);
                }

                angleOffset %= 360;
                while (angleOffset < 0)
                {
                    angleOffset = (angleOffset + 360) % 360;
                }

                if (handSafeZone == SolverSafeZone.AtopPalm)
                {
                    return CalculateRayForSafeZone(
                        origin,
                        targetTransform,
                        hand,
                        handSafeZone,
                        offsetBehavior,
                        angleOffset);
                }

                float offset = angleOffset / 90;
                int intOffset = Mathf.FloorToInt(offset);
                float fracOffset = offset - intOffset;

                int currentSafeZoneClockwiseIdx = Array.IndexOf(handSafeZonesClockWiseRightHand, handSafeZone);

                SolverSafeZone intPartSafeZoneClockwise =
                    handSafeZonesClockWiseRightHand[(currentSafeZoneClockwiseIdx + intOffset) % handSafeZonesClockWiseRightHand.Length];

                SolverSafeZone fracPartSafeZoneClockwise =
                    handSafeZonesClockWiseRightHand[(currentSafeZoneClockwiseIdx + intOffset + 1) % handSafeZonesClockWiseRightHand.Length];

                Ray intSafeZoneRay = CalculateRayForSafeZone(
                    origin,
                    targetTransform,
                    hand,
                    intPartSafeZoneClockwise,
                    offsetBehavior);

                Ray fracPartSafeZoneRay = CalculateRayForSafeZone(
                    origin,
                    targetTransform,
                    hand,
                    fracPartSafeZoneClockwise,
                    offsetBehavior);

                Vector3 direction = Vector3.Lerp(
                    -intSafeZoneRay.direction,
                    -fracPartSafeZoneRay.direction,
                    fracOffset).normalized;

                return new Ray(origin + direction, -direction);
            }
        }
        private bool IsPalmFacingCamera(XRNode? hand)
        {
            Debug.Assert(hand.HasValue);

            HandJointPose? palmPose = GetPalmPose(hand);

            if (palmPose.HasValue)
            {
                return (Vector3.Dot(palmPose.Value.Up, Camera.main.transform.forward) > 0.0f);
            }

            return false;
        }

        private static readonly ProfilerMarker GetPalmPosePerfMarker =
            new ProfilerMarker("[MRTK] HandConstraint.GetPalmPose");
        private HandJointPose? GetPalmPose(XRNode? hand)
        {
            using (GetPalmPosePerfMarker.Auto())
            {
                Debug.Assert(hand.HasValue);
                HandJointPose? jointPose = null;

                if (XRSubsystemHelpers.HandsAggregator != null &&
                    XRSubsystemHelpers.HandsAggregator.TryGetJoint(
                    TrackedHandJoint.Palm,
                    hand.Value,
                    out HandJointPose pose))
                {
                    jointPose = pose;
                }

                return jointPose;
            }
        }
        protected XRNode? GetControllerNode(Handedness handedness)
        {
            if (!SolverHandler.IsValidHandedness(handedness)) { return null; }
            return (handedness == Handedness.Left) ? XRNode.LeftHand : XRNode.RightHand;
        }

        #region MonoBehaviour Implementation

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            handBounds = GetComponent<HandBounds>();

            // Initially no hands are tacked or active.
            trackedNode = null;
            OnHandDeactivate.Invoke();

            if (SolverHandler.TrackedTargetType != TrackedObjectType.HandJoint &&
                SolverHandler.TrackedTargetType != TrackedObjectType.Interactor)
            {
                Debug.LogWarning("Solver HandConstraint requires TrackedObjectType of type HandJoint or Interactor.");
            }
        }

        #endregion MonoBehaviour Implementation

        #region Enums

        /// <summary>
        /// Specifies how the solver's offset relative to the hand / controller will be computed.
        /// </summary>
        public enum SolverOffsetBehavior
        {
            /// <summary>
            /// Uses the object-to-head vector to compute an offset independent
            /// of hand / controller rotation.
            /// </summary>
            LookAtCameraRotation,

            /// <summary>
            /// Uses the hand / controller rotation to compute an offset independent
            /// of look at camera rotation.
            /// </summary>
            TrackedObjectRotation
        }

        /// <summary>
        /// Specifies how the solver should rotate when tracking a hand or motion controller. 
        /// </summary>
        public enum SolverRotationBehavior
        {
            /// <summary>
            /// The solver simply follows the rotation of the tracked object. 
            /// </summary>
            None = 0,

            /// <summary>
            /// The solver faces the main camera (user).
            /// </summary>
            LookAtMainCamera = 2,

            /// <summary>
            /// The solver faces the tracked object. A controller to world transformation is
            /// applied to work with traditional user facing UI (-z is forward).
            /// </summary>
            LookAtTrackedObject = 3
        }

        /// <summary>
        /// Specifies a zone that is safe for the constraint to solve to without intersecting the hand.
        /// Safe zones may differ slightly from motion controller to motion controller, it's recommended to
        /// pick the safe zone best suited for your intended controller and application.
        /// </summary>
        public enum SolverSafeZone
        {
            /// <summary>
            /// On the left controller with palm up, the area right of the palm.
            /// </summary>
            UlnarSide = 0,

            /// <summary>
            /// On the left controller with palm up, the area left of the palm.
            /// </summary>
            RadialSide = 1,

            /// <summary>
            /// Above the longest finger tips.
            /// </summary>
            AboveFingerTips = 2,

            /// <summary>
            /// Below where the controller meets the arm.
            /// </summary>
            BelowWrist = 3,

            /// <summary>
            /// Floating above the palm, towards the "inside" of the hand (opposite side of knuckles),
            /// based on the "down" vector of the palm joint (i.e., the grabbing-side of the hand)
            /// </summary>
            AtopPalm = 4
        }

        #endregion Enums
    }
}
