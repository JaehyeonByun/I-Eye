// /******************************************************************************
//  * File: QCHTControlBox.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Distal.ControlBox
{
    public class QCHTControlBox : MonoBehaviour
    {
        private const string InteractionGridResourceName = "InteractionGrid";

        [SerializeField] private QCHTControlBoxSettings settings;

        private XRInteractionGrid _interactionGrid;
        private Rigidbody _rigidBody;
        private bool _wasNotKinematic;

        public UnityEvent selectEntered;
        public UnityEvent selectExited;

        private void Awake() => _rigidBody = GetComponent<Rigidbody>();

        private void OnEnable()
        {
            if (!settings)
                settings = ScriptableObject.CreateInstance<QCHTControlBoxSettings>();
            
            if (!_interactionGrid)
                if (!TryToLoadGrid(out _interactionGrid))
                    enabled = false;
                    
            if (_interactionGrid)
            {
                _interactionGrid.gameObject.SetActive(true);
                _interactionGrid.onHandled += OnHandled;
                _interactionGrid.onReleased += OnReleased;
            }
        }
        
        private void OnDisable()
        {
            if (_interactionGrid)
            {
                _interactionGrid.gameObject.SetActive(false);
                _interactionGrid.onHandled -= OnHandled;
                _interactionGrid.onReleased -= OnReleased;
            }
        }
        
        private void Update()
        {
            if (_interactionGrid)
                _interactionGrid.Settings = settings;
        }

        private void OnHandled()
        {
            if (_rigidBody)
            {
                _wasNotKinematic = !_rigidBody.isKinematic;
                _rigidBody.isKinematic = true;
            }
            
            selectEntered?.Invoke();
        }

        private void OnReleased()
        {
            if (_rigidBody && _wasNotKinematic)
                _rigidBody.isKinematic = false;
            
            selectExited?.Invoke();
        }

        private bool TryToLoadGrid(out XRInteractionGrid grid)
        {
            var prefab = Resources.Load(InteractionGridResourceName) as GameObject;
            if (prefab == null)
            {
                Debug.LogError("[QCHTInteractionGizmo:Start] Can't find InteractionGrid in resources");
                grid = null;
                return false;
            }

            // Setup grid object
            var t = transform;
            var bounds = gameObject.GetMeshBoundingBox();
            var gridObject = Instantiate(prefab, t);
            //gridObject.hideFlags = HideFlags.HideAndDontSave;

            var test = t.rotation * bounds.size;
            var scale = settings ? Vector3.one * settings.ScaleOffset : Vector3.one;
            gridObject.transform.localScale = Vector3Extensions.Abs(test.Divide(t.lossyScale)) + scale;
            gridObject.transform.localPosition = t.InverseTransformPoint(bounds.center);

            grid = gridObject.GetComponent<XRInteractionGrid>();
            return true;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(QCHTControlBox))]
        public class QCHTControlBoxEditor : Editor
        {
            private SerializedProperty _settingsProperty;
            private Editor _editor;

            public void OnEnable()
            {
                _settingsProperty = serializedObject.FindProperty("settings");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(_settingsProperty);
                GUILayout.Space(10);

                if (_settingsProperty.objectReferenceValue == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Create new control box settings", GUILayout.Width(300), GUILayout.Height(25)))
                    {
                        var settings = CreateControlBoxSettings();
                        _settingsProperty.objectReferenceValue = settings;
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    CreateCachedEditor(_settingsProperty.objectReferenceValue, null, ref _editor);
                    _editor.DrawDefaultInspector();
                }

                serializedObject.ApplyModifiedProperties();
            }

            private static QCHTControlBoxSettings CreateControlBoxSettings()
            {
                var settings = CreateInstance<QCHTControlBoxSettings>();
                if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
                var newFile = "Assets/Settings/NewControlBoxSettings.asset";
                newFile = AssetDatabase.GenerateUniqueAssetPath(newFile);
                AssetDatabase.CreateAsset(settings, newFile);
                EditorUtility.FocusProjectWindow();
                var obj = AssetDatabase.LoadAssetAtPath<Object>(newFile);
                EditorGUIUtility.PingObject(obj);
                Debug.LogWarning($"Asset has been created at {newFile}");
                return settings;
            }
        }
#endif
    }
}