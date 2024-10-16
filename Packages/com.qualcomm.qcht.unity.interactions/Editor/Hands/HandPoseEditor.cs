// /******************************************************************************
//  * File: HandPoseEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Core;
using QCHT.Interactions.Editor;
using UnityEditor;
using UnityEngine;

namespace QCHT.Interactions.Hands.Editor
{
    [CustomEditor(typeof(HandPose))]
    public sealed class HandPoseEditor : UnityEditor.Editor
    {
        [SerializeField] private GameObject leftGhost;
        [SerializeField] private GameObject rightGhost;
        
        private GameObject _handGhost;
        private Vector2 _previewDir;
        
        private void OnDisable() => CleanupPreviewRenderUtility();
        
        #region Inspector

        public override void OnInspectorGUI()
        {
            var handPose = serializedObject.targetObject as HandPose;
            if (handPose == null) return;
            
            var handSide = (XrHandedness) EditorGUILayout.EnumPopup("Hand Side", handPose.Handedness);
            if (handSide != handPose.Handedness)
            {
                handPose.Handedness = handSide;
                EditorUtility.SetDirty(handPose);
                AssetDatabase.SaveAssetIfDirty(handPose);
            }            
            
            var handSpace = (XrSpace) EditorGUILayout.EnumPopup("Space", handPose.Space);
            if (handSpace != handPose.Space)
            {
                handPose.Space = handSpace;
                EditorUtility.SetDirty(handPose);
                AssetDatabase.SaveAssetIfDirty(handPose);
            }

            if (GUILayout.Button("Convert to new"))
            {
                handPose.ConvertToNew();
                EditorUtility.SetDirty(handPose);
                AssetDatabase.SaveAssetIfDirty(handPose);
            }
        }

        #endregion

        #region Preview

        private const string PreviewGhostInstanceName = "ghostPreview";

        private PreviewRenderUtility _previewRenderUtility;

        public override bool HasPreviewGUI() => true;

        public override void OnInteractivePreviewGUI(Rect rect, GUIStyle background)
        {
            base.OnInteractivePreviewGUI(rect, background);
            
            if (_previewRenderUtility == null)
            {
                _previewRenderUtility = new PreviewRenderUtility();
                InitPreviewCamera();
            }

            _previewRenderUtility.BeginPreview(rect, background);
            DoRenderPreview(rect);
            _previewRenderUtility.EndAndDrawPreview(rect);
        }

        private void InitPreviewCamera()
        {
            var camera = _previewRenderUtility.camera;
            var cameraTransform = camera.transform;
            cameraTransform.position = new Vector3(0f, .07f, -1f);
            cameraTransform.LookAt(new Vector3(0f, .07f, 0f));
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = new Color(.2f, .2f, .2f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
        }

        private void DoRenderPreview(Rect r)
        {
            _previewDir = PreviewGUI.Drag2D(_previewDir, r);

            if (!_handGhost) 
                ReloadGhostPreview();
            
            if (_handGhost)
            {
                _handGhost.transform.rotation = Quaternion.Euler(_previewDir.y, 0.0f, 0.0f) *
                                                Quaternion.Euler(0.0f, _previewDir.x, 0.0f);
            }

            _previewRenderUtility.Render();
        }

        private void ReloadGhostPreview()
        {
            var handPose = serializedObject.targetObject as HandPose;
            if (!handPose) return;

            var obj = handPose.Handedness == XrHandedness.XR_HAND_LEFT ? leftGhost : rightGhost;
            if (!obj) return;

            var ghostGo = _previewRenderUtility.InstantiatePrefabInScene(obj.gameObject);
            ghostGo.name = PreviewGhostInstanceName;
            ghostGo.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            if (!ghostGo)
            {
                OnDisable();
                return;
            }

            _handGhost = ghostGo;

            if (_handGhost.TryGetComponent<HandGhost>(out var handGhost))
            {
                handGhost.HandPose = handPose.ToHandData();
                var ghostTransform = handGhost.transform;
                ghostTransform.position = Vector3.zero;
                ghostTransform.rotation = handGhost.HandPose.Root.Rotation;
            }
        }

        private void CleanupPreviewRenderUtility()
        {
            if (_previewRenderUtility == null)
                return;

            _previewRenderUtility.Cleanup();
            _previewRenderUtility = null;
        }

        #endregion
        
        #region Asset

        public static HandPose CreateNewHandPoseAsset()
        {
            var handPose = CreateInstance<HandPose>();
            AssetUtils.CreateAssetInSettingsFromObj(handPose, "HandPoses", "NewHandPose");
            return handPose;
        }

        public static HandPose DuplicateHandPoseAsset(HandPose handPose)
        {
            var newPose = Instantiate(handPose);
            AssetUtils.CreateAssetInSettingsFromObj(newPose, "HandPoses", handPose.name);
            return newPose;
        }

        #endregion
    }

    internal static class PreviewGUI
    {
        private static readonly int s_sliderHash = "Slider".GetHashCode();

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            var idControl = GUIUtility.GetControlID(s_sliderHash, FocusType.Passive);
            var cEvent = Event.current;

            switch (cEvent.GetTypeForControl(idControl))
            {
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == idControl)
                    {
                        scrollPosition -= cEvent.delta * (cEvent.shift ? 3f : 1f) /
                            Mathf.Min(position.width, position.height) * 140f;
                        cEvent.Use();
                        GUI.changed = true;
                    }

                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == idControl)
                    {
                        GUIUtility.hotControl = 0;
                    }

                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;

                case EventType.MouseDown:
                    if (position.Contains(cEvent.mousePosition) && position.width > 50.0f)
                    {
                        GUIUtility.hotControl = idControl;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        cEvent.Use();
                    }

                    break;
            }

            return scrollPosition;
        }
    }
}