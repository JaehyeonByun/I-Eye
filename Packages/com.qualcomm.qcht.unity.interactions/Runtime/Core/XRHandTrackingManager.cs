// /******************************************************************************
//  * File: XRHandTrackingManager.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Runtime.CompilerServices;
using UnityEngine;
using QCHT.Interactions.Hands;
using QCHT.Interactions.Hands.VFF;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Interaction.Toolkit;

#if UNITY_EDITOR
using UnityEditor;
using System.Globalization;
#endif

namespace QCHT.Interactions.Core
{
    public partial class XRHandTrackingManager : MonoBehaviour
    {
        private const string kResourcesHandLeft = "QualcommHandLeft";
        private const string kResourcesHandRight = "QualcommHandRight";
        private const string kTrackableLeftName = "QC Hand Left";
        private const string kTrackableRightName = "QC Hand Right";

        [SerializeField, Tooltip("Left prefab object that will be instantiated")]
        private GameObject leftHandPrefab;

        /// <summary>
        /// Left prefab object that will be instantiated.
        /// If the prefab changed after Instantiation time, it can be refreshed by calling RefreshLeftHand
        /// </summary>
        public GameObject LeftHandPrefab
        {
            get => leftHandPrefab;
            set => leftHandPrefab = value;
        }

        [SerializeField, Tooltip("Right prefab object that will be instantiated")]
        private GameObject rightHandPrefab;

        /// <summary>
        /// Right prefab object that will be instantiated.
        /// If the prefab changed after Instantiation time, it can be refreshed by calling RefreshRightHand.
        /// </summary>
        public GameObject RightHandPrefab
        {
            get => rightHandPrefab;
            set => rightHandPrefab = value;
        }

        [Space]
        [SerializeField, Tooltip("Should it try to start the hand tracking subsystem when enabling this component?")]
        private bool startSubsystemOnEnable;

        /// <summary>
        /// Should it try to start the hand tracking subsystem when enabling this component?
        /// </summary>
        public bool StartSubsystemOnEnable
        {
            get => startSubsystemOnEnable;
            set => startSubsystemOnEnable = value;
        }

        private GameObject _leftHand;

        /// <summary>
        /// Returns the current left hand game object instance.
        /// </summary>
        public GameObject LeftHand => _leftHand;

        private GameObject _rightHand;

        /// <summary>
        /// Returns the current right hand game object instance.
        /// </summary>
        public GameObject RightHand => _rightHand;

        /// <summary>
        /// Stores the left hand skin set.
        /// In case the skin provided can't be applied to the hand object, this field stores it to attempt applying it at next hand object's instantiation time.
        /// </summary>
        private HandSkin _leftHandSkin;

        /// <summary>
        /// Current left hand skin.
        /// </summary>
        public HandSkin LeftHandSkin
        {
            get => _leftHandSkin;
            set
            {
                _leftHandSkin = value;
                TrySetSkin(_leftHand, value);
            }
        }

        /// <summary>
        /// Stores the right hand skin set.
        /// In case the skin provided can't be applied to the hand object, this field stores it to attempt applying it at next hand object's instantiation time.
        /// </summary>
        private HandSkin _rightHandSkin;

        /// <summary>
        /// Current right hand skin.
        /// </summary>
        public HandSkin RightHandSkin
        {
            get => _rightHandSkin;
            set
            {
                _rightHandSkin = value;
                TrySetSkin(_rightHand, value);
            }
        }

        /// <summary>
        /// Stores if left hand object should be visible.
        /// Used by ToggleHand to force disabling hand visibility even if left hand is tracked by the subsystem.  
        /// </summary>
        private bool _forceLeftDisabled;

        /// <summary>
        /// Stores if right hand object should be visible.
        /// Used by ToggleHand to force disabling hand visibility even if right hand is tracked by the subsystem.  
        /// </summary>
        private bool _forceRightDisabled;

        /// <summary>
        /// Stores if left hand object should enable vff in case of new instance.
        /// </summary>
        private bool _shouldLeftBeVff;

        /// <summary>
        /// Stores if right hand object should enable vff in case of new instance.
        /// </summary>
        private bool _shouldRightBeVff;

        protected XRHandTrackingSubsystem _subsystem;

        protected void OnEnable()
        {
            FindXRHandTrackingSubsystem();

            if (_subsystem != null)
            {
                if (startSubsystemOnEnable)
                {
                    _subsystem.Start();
                }

                OnHandTracked(_subsystem.LeftHand);
                OnHandTracked(_subsystem.RightHand);
            }
        }

        protected void OnDisable()
        {
            if (_subsystem != null)
            {
                _subsystem.OnHandTracked -= OnHandTracked;
                _subsystem.OnHandUntracked -= OnHandUntracked;
                _subsystem.OnHandsUpdated -= OnHandsUpdated;
                _subsystem = null;
            }

            UpdateHandVisible(XrHandedness.XR_HAND_LEFT, false);
            UpdateHandVisible(XrHandedness.XR_HAND_RIGHT, false);
        }

        protected void Update()
        {
            FindXRHandTrackingSubsystem();

            if (_leftHand == null && leftHandPrefab != null)
            {
                InitializeLeftHand();
            }

            if (_rightHand == null && rightHandPrefab != null)
            {
                InitializeRightHand();
            }
        }

        protected void OnDestroy()
        {
            if (_leftHand != null)
            {
                Destroy(_leftHand);
            }

            if (_rightHand != null)
            {
                Destroy(_rightHand);
            }
        }

        private void FindXRHandTrackingSubsystem()
        {
            if (_subsystem != null)
            {
                return;
            }

            _subsystem = XRHandTrackingSubsystem.GetSubsystemInManager();

            if (_subsystem != null)
            {
                _subsystem.OnHandTracked += OnHandTracked;
                _subsystem.OnHandUntracked += OnHandUntracked;
                _subsystem.OnHandsUpdated += OnHandsUpdated;
            }
        }

#if UNITY_EDITOR
        protected void OnGUI()
        {
            var simSubsystem = _subsystem?.GetProvider() as XRHandSimulationProvider;
            if (simSubsystem != null && simSubsystem.running)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Space(20);
                GUILayout.Label("Hand scale");
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                simSubsystem.HandScale =
                    GUILayout.HorizontalSlider(simSubsystem.HandScale, .1f, 2f, GUILayout.MinWidth(100));
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.Label(simSubsystem.HandScale.ToString("#.##", CultureInfo.InvariantCulture),
                    GUILayout.MinWidth(30));
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
        }
#endif

        #region Subsystem callbacks

        private void OnHandTracked(XRHandTrackingSubsystem.Hand data)
        {
            UpdateHandVisible(data.Handedness, true);
        }

        private void OnHandUntracked(XRHandTrackingSubsystem.Hand data)
        {
            UpdateHandVisible(data.Handedness, false);
        }

        private void OnHandsUpdated(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
            {
                return;
            }

            if (_subsystem != null)
            {
                UpdateHand(_subsystem.LeftHand, _leftHand);
                UpdateHand(_subsystem.RightHand, _rightHand);
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateHand(XRHandTrackingSubsystem.Hand hand, GameObject handObj)
        {
            if (handObj == null)
            {
                return;
            }

            handObj.transform.localPosition = hand.Root.position;
            handObj.transform.localRotation = hand.Root.rotation;
        }

        #region Hand Objects

        /// <summary>
        /// Forces regenerating the left hand trackable object.
        /// </summary>
        public void RefreshLeftHand()
        {
            RemoveLeftHand();
            InitializeLeftHand();
        }

        /// <summary>
        /// Forces regenerating the right hand trackable object.
        /// </summary>
        public void RefreshRightHand()
        {
            RemoveRightHand();
            InitializeRightHand();
        }

        private void InitializeLeftHand()
        {
            InstantiateHand(kTrackableLeftName, leftHandPrefab, ref _leftHand);

            if (_leftHandSkin != null)
            {
                TrySetSkin(_leftHand, _leftHandSkin);
            }
            else
                _leftHandSkin = TryGetComponentOnHand<IHandSkinnable>(_leftHand, out var skinnable)
                    ? skinnable.HandSkin
                    : null;

            if (_subsystem != null)
            {
                UpdateHandVisible(_subsystem.LeftHand.Handedness, _subsystem.LeftHand.IsTracked);
            }

            TrySetVff(XrHandedness.XR_HAND_LEFT, _shouldLeftBeVff);
        }

        private void InitializeRightHand()
        {
            InstantiateHand(kTrackableRightName, rightHandPrefab, ref _rightHand);

            if (_rightHandSkin != null)
            {
                TrySetSkin(_rightHand, _rightHandSkin);
            }
            else
                _rightHandSkin = TryGetComponentOnHand<IHandSkinnable>(_rightHand, out var skinnable)
                    ? skinnable.HandSkin
                    : null;

            if (_subsystem != null)
            {
                UpdateHandVisible(_subsystem.RightHand.Handedness, _subsystem.RightHand.IsTracked);
            }

            TrySetVff(XrHandedness.XR_HAND_RIGHT, _shouldRightBeVff);
        }

        private void InstantiateHand(string objectName, GameObject prefab, ref GameObject hand)
        {
            if (hand != null)
            {
                return;
            }

            hand = InstantiateHandTrackable(objectName, prefab);
            SetParentTrackable(hand);
        }

        private void RemoveLeftHand()
        {
            RemoveHand(ref _leftHand);
        }

        private void RemoveRightHand()
        {
            RemoveHand(ref _rightHand);
        }

        private static void RemoveHand(ref GameObject hand)
        {
            if (hand != null)
            {
                Destroy(hand);
                hand = null;
            }
        }

        private static bool TrySetSkin(GameObject hand, HandSkin skin)
        {
            if (TryGetComponentOnHand(hand, out IHandSkinnable skinnable))
            {
                skinnable.HandSkin = skin;
                return true;
            }

            Debug.Log("[XRHandTrackingManager:TrySetSkin] Hand object is not skinnable.");
            return false;
        }

        /// <summary>
        /// Toggles hand game object visibility.
        /// It won't stop the hand tracking subsystem and interactions, simply the game object visibility.
        /// To start or stop the subsystem, get the instance with `XRHandTrackingSubsystem.GetSubsystemInManager()` in your script then call subsystem.Start() or subsystem.Stop().
        /// </summary>
        /// <param name="handedness"> Handedness to toggle </param>
        /// <param name="visible"> Is the object visible? </param>
        public void ToggleHand(XrHandedness handedness, bool visible)
        {
            var isLeft = handedness == XrHandedness.XR_HAND_LEFT;
            var isTracked = false;
            if (_subsystem != null)
            {
                isTracked = isLeft ? _subsystem.LeftHand.IsTracked : _subsystem.RightHand.IsTracked;
            }

            ref var forceDisabled = ref isLeft ? ref _forceLeftDisabled : ref _forceRightDisabled;
            forceDisabled = !visible;

            UpdateHandVisible(handedness, isTracked);
        }

        private void UpdateHandVisible(XrHandedness handedness, bool visible)
        {
            var isLeft = handedness == XrHandedness.XR_HAND_LEFT;
            var hand = isLeft ? _leftHand : _rightHand;
            if (hand == null)
            {
                return;
            }

            var forceDisabled = isLeft ? _forceLeftDisabled : _forceRightDisabled;
            if (visible && !forceDisabled)
            {
                hand.gameObject.SetActive(true);
                TryShow(hand);
            }
            else
            {
                TryHide(hand);
                hand.gameObject.SetActive(false);
            }
        }

        private static bool TryShow(GameObject hand)
        {
            if (TryGetComponentOnHand(hand, out IHideable hideable))
            {
                hideable.Show();
                return true;
            }

            return false;
        }

        private static bool TryHide(GameObject hand)
        {
            if (TryGetComponentOnHand(hand, out IHideable hideable))
            {
                hideable.Hide();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to set custom hand pose on hand game object.
        /// </summary>
        /// <param name="handedness"> Hand to modify. </param>
        /// <param name="data"> Hand pose data. </param>
        /// <param name="mask"> Hand pose mask. </param>
        /// <returns> True if hand pose receiver has been found. </returns>
        public bool TrySetHandPose(XrHandedness handedness, HandData? data, HandMask? mask)
        {
            var hand = handedness == XrHandedness.XR_HAND_LEFT ? _leftHand : _rightHand;
            return TrySetHandPose(hand, data, mask);
        }

        private static bool TrySetHandPose(GameObject hand, HandData? pose = null, HandMask? mask = null)
        {
            if (TryGetComponentOnHand(hand, out IHandPoseReceiver poseOverrider))
            {
                poseOverrider.HandPoseOverride = pose;
                poseOverrider.HandPoseMask = mask;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to set poking point reference on hand game object
        /// Set point null to release poking effect on hand
        /// </summary>
        /// <param name="handedness"> Hand to modify. </param>
        /// <param name="pokePoint"> Hand point reference. If null poking will be un-active. </param>
        /// <returns> True if hand poke receiver has been found. </returns>
        public bool TrySetPoking(XrHandedness handedness, Vector3? pokePoint)
        {
            var hand = handedness == XrHandedness.XR_HAND_LEFT ? _leftHand : _rightHand;
            return TrySetPoking(hand, pokePoint);
        }

        private static bool TrySetPoking(GameObject hand, Vector3? pokePoint)
        {
            if (TryGetComponentOnHand(hand, out IHandPokeReceiver pokable))
            {
                pokable.PokePoint = pokePoint;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to set virtual force feed back system state if available on right hand object.
        /// </summary>
        /// <param name="handedness"> Hand to set. </param>
        /// <param name="active"> Is Vff active? </param>
        /// <returns></returns>
        public bool TrySetVff(XrHandedness handedness, bool active)
        {
            var isLeft = handedness == XrHandedness.XR_HAND_LEFT;
            var hand = isLeft ? _leftHand : _rightHand;

            ref var shouldBeVff = ref isLeft ? ref _shouldLeftBeVff : ref _shouldRightBeVff;
            shouldBeVff = active;

            return TrySetVff(hand, active);
        }

        private static bool TrySetVff(GameObject hand, bool active)
        {
            if (TryGetComponentOnHand(hand, out IHandPhysible vffable))
            {
                vffable.IsPhysible = active;
                return true;
            }

            return false;
        }

        private static bool TryGetComponentOnHand<T>(GameObject hand, out T component)
        {
            if (hand == null)
            {
                component = default;
                return false;
            }

            return hand.TryGetComponent(out component);
        }

        private void SetParentTrackable(GameObject hand)
        {
            if (hand == null) return;
            var trackablesParent = XROriginUtility.GetTrackablesParent();
            trackablesParent = trackablesParent ? trackablesParent : transform;
            var handTransform = hand.transform;
            handTransform.SetParent(trackablesParent);
            handTransform.localPosition = Vector3.zero;
            handTransform.localRotation = Quaternion.identity;
        }

        #endregion

        #region static

        /// <summary>
        /// Default left hand prefab loaded in resources.
        /// </summary>
        public static GameObject DefaultLeftHandPrefab => Resources.Load<GameObject>(kResourcesHandLeft);

        /// <summary>
        /// Default right hand prefab loaded in resources.
        /// </summary>
        public static GameObject DefaultRightHandPrefab => Resources.Load<GameObject>(kResourcesHandRight);

        /// <summary>
        /// Instantiates a hand tracking manager with default hands prefabs.
        /// </summary>
        public static XRHandTrackingManager InstantiateHandTrackingManager()
        {
            return GetOrCreate(DefaultLeftHandPrefab, DefaultRightHandPrefab);
        }

        /// <summary>
        /// Gets existing or creates a Hand Tracking Manager if it doesn't exist.
        /// If an XRHandTrackingManager instance already exists, Set Left/Right prefabs fields on exiting instance then use RefreshHandLeft/Right functions to update hand models.
        /// </summary>
        /// <param name="leftPrefab"> Left prefab reference in case of XRHandTrackingManager instance is created. </param>
        /// <param name="rightPrefab"> Right prefab reference in case of XRHandTrackingManager instance is created. </param>
        /// <returns> New or existing hand tracking manager instance. </returns>
        public static XRHandTrackingManager GetOrCreate(GameObject leftPrefab = null, GameObject rightPrefab = null)
        {
            var manager = FindObjectOfType<XRHandTrackingManager>(true);
            if (manager != null)
            {
#if UNITY_EDITOR
                Selection.activeGameObject = manager.gameObject;
#endif
                return manager;
            }

            GameObject go = null;
#if UNITY_EDITOR
            go = Selection.activeGameObject;
#endif
            if (!go)
            {
                go = new GameObject("XR Hand Tracking Manager");
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
#endif
            }

            manager = go.AddComponent<XRHandTrackingManager>();

            manager.leftHandPrefab = leftPrefab;
            manager.rightHandPrefab = rightPrefab;

            if (Application.isPlaying)
            {
                manager.RefreshLeftHand();
                manager.RefreshRightHand();
            }

            return manager;
        }

        /// <summary>
        /// Destroys Hand Tracking Manager instance if it does exist.
        /// </summary>
        public static void Destroy(XRHandTrackingManager manager = null)
        {
            manager = manager ? manager : FindObjectOfType<XRHandTrackingManager>();
            if (manager == null) return;
            GameObject.Destroy(manager);
        }

        private static GameObject InstantiateHandTrackable(string handName, GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            GameObject instance;
            if (prefab.scene.rootCount == 0)
            {
                instance = Instantiate(prefab);
                instance.name = handName;
            }
            else
            {
                instance = prefab;
            }

            return instance;
        }

        #endregion
    }
}