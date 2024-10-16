// /******************************************************************************
//  * File: XRHandInteractableSnapPoseProviderEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using QCHT.Interactions.Hands;

namespace QCHT.Interactions.Proximal.Editor
{
    [CustomEditor(typeof(XRHandInteractableSnapPoseProvider))]
    public sealed class XRHandInteractableSnapPoseProviderEditor : UnityEditor.Editor
    {
        private VisualElement _inspector;

        [SerializeField] private GameObject leftGhost;
        [SerializeField] private GameObject rightGhost;

        private XRHandInteractableSnapPoseProvider _provider;
        private HandGhost _handGhost;

        private float _scale = 1f;

        #region Editor

        private void OnEnable()
        {
            _provider = target as XRHandInteractableSnapPoseProvider;

            if (_provider == null)
            {
                return;
            }
            
            _provider.FindPoses();
            _provider.SanitizePoseList();
            
            InstantiateGhost(_provider.Handedness);
            UpdateGhost();

            var center = _provider.transform.position;
            var size = Vector3.one * 0.2f;
            var bounds = new Bounds(center, size);
            SceneView.lastActiveSceneView.Frame(bounds, false);
        }

        private void OnDisable()
        {
            DestroyGhost();
        }

        public override void OnInspectorGUI()
        {
            const string kHandedness = "handedness";
            var handedness = (XrHandedness) serializedObject.FindProperty(kHandedness).intValue;

            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                var newHandedness = (XrHandedness) serializedObject.FindProperty(kHandedness).intValue;
                if (newHandedness != handedness)
                {
                    // Flip all children poses
                    foreach (var pose in _provider.Poses)
                    {
                        var data = pose.Data;
                        data.Flip();
                        pose.Data = data;
                    }
                    
                    DestroyGhost();
                    InstantiateGhost(newHandedness);
                    UpdateGhost();
                }
            }

            EditorGUI.BeginChangeCheck();
            
            var minPose = _provider.Poses[0];
            var maxPose = _provider.Poses[_provider.Poses.Count - 1];

            var minScale = minPose != null ? minPose.Scale : 0f;
            var maxScale = maxPose != null ? maxPose.Scale : 1f;
            
            if (Mathf.Abs(minScale - maxScale) < Mathf.Epsilon)
            {
                GUI.enabled = false;
            }

            _scale = EditorGUILayout.Slider("Scale", _scale, minScale, maxScale);
            
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            UpdateGhost();
        }

        private void UpdateGhost()
        {
            if (_handGhost == null)
                return;

            var interHandPose = new HandData();
            var interRootPose = new Pose();

            if (_provider.TryGetInterpolatedHandPoseFromScale(ref interHandPose, ref interRootPose, _scale))
            {
                _handGhost.HandPose = interHandPose;
                var transform = _provider.transform;
                var ghostTransform = _handGhost.transform;
                ghostTransform.rotation = transform.rotation * interRootPose.rotation;
                ghostTransform.position = transform.TransformPoint(interRootPose.position);

                var scale = Vector3.one * _scale;
                
                if (ghostTransform.transform.parent != null)
                {
                    scale = scale.Divide(transform.lossyScale);
                }
                
                ghostTransform.localScale = scale;
            }
        }

        private void InstantiateGhost(XrHandedness handedness)
        {
            var ghostPrefab = handedness == XrHandedness.XR_HAND_LEFT ? leftGhost : rightGhost;
            var obj = Instantiate(ghostPrefab, null, true);
            obj.hideFlags = HideFlags.HideAndDontSave;
            StageUtility.PlaceGameObjectInCurrentStage(obj);
            _handGhost = obj.GetComponent<HandGhost>();
        }

        private void DestroyGhost()
        {
            if (_handGhost == null)
                return;

            DestroyImmediate(_handGhost.gameObject);
        }

        #endregion
    }
}