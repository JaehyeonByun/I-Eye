// /******************************************************************************
//  * File: HandPose.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Core;
using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace QCHT.Interactions.Hands
{
    [Serializable]
    public struct BoneData
    {
        public bool IsRoot;
        public bool IsThumb;
        public bool UpdatePosition;
        public bool UpdateRotation;
        public Vector3 Position;
        public Quaternion Rotation;

        public static BoneData DefaultRoot => new BoneData
        {
            IsRoot = true,
            UpdatePosition = false,
            UpdateRotation = false,
            Position = Vector3.zero,
            Rotation = Quaternion.identity
        };

        public static BoneData Default => new BoneData
        {
            UpdateRotation = true,
            Position = Vector3.zero,
            Rotation = Quaternion.identity
        };

        public static BoneData DefaultThumb => new BoneData
        {
            IsThumb = true,
            UpdateRotation = true,
            Position = Vector3.zero,
            Rotation = Quaternion.identity
        };

        public void Flip()
        {
            if (IsRoot)
            {
                // Ignored
            }
            else if (IsThumb)
            {
                Rotation = Rotation.FlipZAxis();
                Rotation = Rotation.FlipXZAxis();
            }
            else
            {
                Rotation = Rotation.FlipXAxis();
                Rotation = Rotation.FlipZXAxis();
            }
        }

        public static BoneData Lerp(BoneData a, BoneData b, float t)
        {
            var bone = a;
            bone.Position = Vector3.Lerp(a.Position, b.Position, t);
            bone.Rotation = Quaternion.Lerp(a.Rotation, b.Rotation, t);
            return bone;
        }

        public Pose ToPose() => new Pose(Position, Rotation);
        public void FromPose(Pose pose) => (Position, Rotation) = (pose.position, pose.rotation);
    }

    [Serializable]
    public struct FingerData
    {
        public BoneData BaseData;
        public BoneData MiddleData;
        public BoneData TopData;

        public static FingerData Default => new FingerData
        {
            BaseData = BoneData.Default,
            MiddleData = BoneData.Default,
            TopData = BoneData.Default
        };

        public static FingerData DefaultThumb => new FingerData
        {
            BaseData = BoneData.DefaultThumb,
            MiddleData = BoneData.DefaultThumb,
            TopData = BoneData.DefaultThumb
        };

        public void Flip()
        {
            BaseData.Flip();
            MiddleData.Flip();
            TopData.Flip();
        }

        public static FingerData Lerp(FingerData a, FingerData b, float t)
        {
            var finger = a;
            finger.BaseData = BoneData.Lerp(finger.BaseData, b.BaseData, t);
            finger.MiddleData = BoneData.Lerp(finger.MiddleData, b.MiddleData, t);
            finger.TopData = BoneData.Lerp(finger.TopData, b.TopData, t);
            return finger;
        }
    }

    [Serializable]
    public struct HandData
    {
        public XrHandedness Handedness;
        public XrSpace Space;

        [Space]
        public BoneData Root;

        public FingerData Thumb;
        public FingerData Index;
        public FingerData Middle;
        public FingerData Ring;
        public FingerData Pinky;

        public void Flip()
        {
            Handedness = Handedness == XrHandedness.XR_HAND_LEFT
                ? XrHandedness.XR_HAND_RIGHT
                : XrHandedness.XR_HAND_LEFT;

            Root.Flip();
            Thumb.Flip();
            Index.Flip();
            Middle.Flip();
            Ring.Flip();
            Pinky.Flip();
        }

        public static HandData Default => new HandData
        {
            Space = XrSpace.XR_HAND_LOCAL,
            Root = BoneData.DefaultRoot,
            Thumb = FingerData.DefaultThumb,
            Index = FingerData.Default,
            Middle = FingerData.Default,
            Ring = FingerData.Default,
            Pinky = FingerData.Default
        };

        public static HandData Lerp(HandData a, HandData b, float t)
        {
            var hand = a;
            hand.Thumb = FingerData.Lerp(a.Thumb, b.Thumb, t);
            hand.Index = FingerData.Lerp(a.Index, b.Index, t);
            hand.Middle = FingerData.Lerp(a.Middle, b.Middle, t);
            hand.Ring = FingerData.Lerp(a.Ring, b.Ring, t);
            hand.Pinky = FingerData.Lerp(a.Pinky, b.Pinky, t);
            return hand;
        }
    }

    /// <summary>
    /// Stores hand pose.
    /// All position and rotation are stored local.
    /// </summary>
    [CreateAssetMenu(menuName = "QCHT/Interactions/HandPose")]
    public sealed class HandPose : ScriptableObject, ICloneable
    {
        [FormerlySerializedAs("_handedness")] 
        [FormerlySerializedAs("_type")]
        [SerializeField]
        private XrHandedness handedness;

        public XrHandedness Handedness
        {
            set
            {
                if (handedness == value)
                    return;

                handedness = value;

                Flip();
            }
            get => handedness;
        }

        public bool IsLeft => Handedness == XrHandedness.XR_HAND_LEFT;

        [SerializeField] private XrSpace space;

        public XrSpace Space
        {
            get => space;
            set
            {
                if (space == value)
                    return;

                space = value;
                
                if (space == XrSpace.XR_HAND_LOCAL)
                    this.ConvertToLocal();
                else
                    this.ConvertToWorld();
            }
        }

        public Vector3 Scale = Vector3.one;

        [Space]
        public BoneData Root;
        public BoneData Palm;

        public FingerData Thumb;
        public FingerData Index;
        public FingerData Middle;
        public FingerData Ring;
        public FingerData Pinky;

        public HandPose()
        {
            Root = BoneData.DefaultRoot;
            Palm = BoneData.Default;
            Thumb = FingerData.DefaultThumb;
            Index = FingerData.Default;
            Middle = FingerData.Default;
            Ring = FingerData.Default;
            Pinky = FingerData.Default;
        }

        public object Clone()
        {
            var handPose = CreateInstance<HandPose>();
            handPose.CopyFrom(this);
            return handPose;
        }

        private void Flip()
        {
            Root.Flip();
            Thumb.Flip();
            Index.Flip();
            Middle.Flip();
            Ring.Flip();
            Pinky.Flip();
        }

        public BoneData GetFingerTip(XrFinger fingerId)
        {
            return fingerId switch
            {
                XrFinger.XR_HAND_FINGER_THUMB => Thumb.TopData,
                XrFinger.XR_HAND_FINGER_INDEX => Index.TopData,
                XrFinger.XR_HAND_FINGER_MIDDLE => Middle.TopData,
                XrFinger.XR_HAND_FINGER_RING => Ring.TopData,
                XrFinger.XR_HAND_FINGER_PINKY => Pinky.TopData,
                _ => throw new ArgumentOutOfRangeException(nameof(fingerId), fingerId, null)
            };
        }
    }

    public static class BoneDataExtensions
    {
        public static void CopyFrom(ref this BoneData boneData, BoneData data)
        {
            boneData.IsRoot = data.IsRoot;
            boneData.IsThumb = data.IsThumb;
            boneData.Position = data.Position;
            boneData.Rotation = data.Rotation;
        }

        public static void ConvertToNew(ref this BoneData boneData)
        {
            boneData.Rotation *= Quaternion.AngleAxis(90f, Vector3.left);
        }
    }

    public static class FingerDataExtensions
    {
        public static void CopyFrom(ref this FingerData fingerData, FingerData finger)
        {
            fingerData.BaseData.CopyFrom(finger.BaseData);
            fingerData.MiddleData.CopyFrom(finger.MiddleData);
            fingerData.TopData.CopyFrom(finger.TopData);
        }

        public static void ConvertToNew(ref this FingerData fingerData)
        {
            fingerData.BaseData.ConvertToNew();
            fingerData.MiddleData.ConvertToNew();
            fingerData.TopData.ConvertToNew();
        }

        public static void ConvertToLocal(ref this FingerData fingerData, BoneData root)
        {
            fingerData.TopData.Rotation =
                Quaternion.Inverse(fingerData.MiddleData.Rotation) * fingerData.TopData.Rotation;
            fingerData.TopData.Position = Quaternion.Inverse(fingerData.MiddleData.Rotation) *
                                          (fingerData.TopData.Position - fingerData.MiddleData.Position);

            fingerData.MiddleData.Rotation =
                Quaternion.Inverse(fingerData.BaseData.Rotation) * fingerData.MiddleData.Rotation;
            fingerData.MiddleData.Position = Quaternion.Inverse(fingerData.MiddleData.Rotation) *
                                             (fingerData.MiddleData.Position - fingerData.BaseData.Position);

            fingerData.BaseData.Rotation = Quaternion.Inverse(root.Rotation) * fingerData.BaseData.Rotation;
            fingerData.BaseData.Position = Quaternion.Inverse(fingerData.BaseData.Rotation) *
                                           (fingerData.BaseData.Position - root.Position);
        }

        public static void ConvertToWorld(ref this FingerData fingerData, BoneData root)
        {
            fingerData.BaseData.Rotation = root.Rotation * fingerData.BaseData.Rotation;
            fingerData.BaseData.Position = root.Position + fingerData.BaseData.Rotation * fingerData.BaseData.Position;

            fingerData.MiddleData.Rotation = fingerData.BaseData.Rotation * fingerData.MiddleData.Rotation;
            fingerData.MiddleData.Position = fingerData.BaseData.Position +
                                             fingerData.MiddleData.Rotation * fingerData.MiddleData.Position;

            fingerData.TopData.Rotation = fingerData.MiddleData.Rotation * fingerData.TopData.Rotation;
            fingerData.TopData.Position = fingerData.MiddleData.Position +
                                          fingerData.TopData.Rotation * fingerData.TopData.Position;
        }
    }

    public static class HandDataExtensions
    {
        public static void ConvertToNew(ref this HandData handData)
        {
            var space = handData.Space;

            if (space == XrSpace.XR_HAND_LOCAL)
                handData.ConvertToWorld();

            handData.Root.ConvertToNew();
            handData.Thumb.ConvertToNew();
            handData.Index.ConvertToNew();
            handData.Middle.ConvertToNew();
            handData.Pinky.ConvertToNew();
            handData.Ring.ConvertToNew();

            if (space == XrSpace.XR_HAND_LOCAL)
                handData.ConvertToLocal();
        }

        public static void ConvertToLocal(ref this HandData handData)
        {
            handData.Space = XrSpace.XR_HAND_LOCAL;
            handData.Thumb.ConvertToLocal(handData.Root);
            handData.Index.ConvertToLocal(handData.Root);
            handData.Middle.ConvertToLocal(handData.Root);
            handData.Ring.ConvertToLocal(handData.Root);
            handData.Pinky.ConvertToLocal(handData.Root);
        }

        public static void ConvertToWorld(ref this HandData handData)
        {
            handData.Space = XrSpace.XR_HAND_WORLD;
            handData.Thumb.ConvertToWorld(handData.Root);
            handData.Index.ConvertToWorld(handData.Root);
            handData.Middle.ConvertToWorld(handData.Root);
            handData.Ring.ConvertToWorld(handData.Root);
            handData.Pinky.ConvertToWorld(handData.Root);
        }
    }

    public static class HandPoseExtensions
    {
        public static void CopyFrom(this HandPose handPose, HandPose pose)
        {
            handPose.Root.CopyFrom(pose.Root);
            handPose.Thumb.CopyFrom(pose.Thumb);
            handPose.Index.CopyFrom(pose.Index);
            handPose.Middle.CopyFrom(pose.Middle);
            handPose.Ring.CopyFrom(pose.Ring);
            handPose.Pinky.CopyFrom(pose.Pinky);
        }

        public static HandData ToHandData(this HandPose handPose) => new HandData
        {
            Handedness = handPose.Handedness,
            Space = handPose.Space,
            Root = handPose.Root,
            Thumb = handPose.Thumb,
            Index = handPose.Index,
            Middle = handPose.Middle,
            Pinky = handPose.Pinky,
            Ring = handPose.Ring
        };

        public static void FromHandData(this HandPose handPose, HandData handData)
        {
            handPose.Handedness = handData.Handedness;
            handPose.Space = handData.Space;
            handPose.Root = handData.Root;
            handPose.Thumb = handData.Thumb;
            handPose.Index = handData.Index;
            handPose.Middle = handData.Middle;
            handPose.Pinky = handData.Pinky;
            handPose.Ring = handData.Ring;
        }

        public static void ConvertToLocal(this HandPose handPose)
        {
            handPose.Space = XrSpace.XR_HAND_LOCAL;
            handPose.Thumb.ConvertToLocal(handPose.Root);
            handPose.Index.ConvertToLocal(handPose.Root);
            handPose.Middle.ConvertToLocal(handPose.Root);
            handPose.Ring.ConvertToLocal(handPose.Root);
            handPose.Pinky.ConvertToLocal(handPose.Root);
        }

        public static void ConvertToWorld(this HandPose handPose)
        {
            handPose.Space = XrSpace.XR_HAND_WORLD;
            handPose.Thumb.ConvertToWorld(handPose.Root);
            handPose.Index.ConvertToWorld(handPose.Root);
            handPose.Middle.ConvertToWorld(handPose.Root);
            handPose.Ring.ConvertToWorld(handPose.Root);
            handPose.Pinky.ConvertToWorld(handPose.Root);
        }

        public static void ConvertToNew(this HandPose handPose)
        {
            var space = handPose.Space;

            if (space == XrSpace.XR_HAND_LOCAL)
                handPose.ConvertToWorld();

            handPose.Root.ConvertToNew();
            handPose.Thumb.ConvertToNew();
            handPose.Index.ConvertToNew();
            handPose.Middle.ConvertToNew();
            handPose.Pinky.ConvertToNew();
            handPose.Ring.ConvertToNew();

            if (space == XrSpace.XR_HAND_LOCAL)
                handPose.ConvertToLocal();
        }
    }
}