/******************************************************************************
 * File: SpacesReferenceImageConfiguratorEditor.cs
 * Copyright (c) 2023-2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [CustomEditor(typeof(SpacesReferenceImageConfigurator))]
    public class SpacesReferenceImageConfiguratorEditor : UnityEditor.Editor
    {
        private SerializedObject _trackedImageManager;
        private SerializedProperty _arManagerReferenceLibrary;
        private SerializedProperty _referenceImages;
        private Dictionary<string, SpacesImageTrackingMode> _trackingModesDict;
        private const int maxImagePreviewSize = 64;

        // This is a QOL helper
        // It helps prevent data loss when switching between different reference libraries on the scriptable object
        // This is not permanent data storage - it doesn't keep data after recompilation
        // or between editor sessions for any except the last selected reference library (because that data was serialized to disk)
        private static readonly Dictionary<string, SpacesImageTrackingMode> _lastSavedValues = new Dictionary<string, SpacesImageTrackingMode>();

        // Set of all valid reference image names. If there are ever values in the internal dictionary which are not in this set, they should be removed
        // this can happen when removing images from the reference libraries and then continuing editing the tracking modes asset
        private readonly HashSet<string> _validTrackingModeNames = new HashSet<string>();

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoCallback;
            _trackedImageManager = new SerializedObject(FindObjectOfType<ARTrackedImageManager>(true));
            InitialiseEditorValues();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoCallback;
        }

        private void UndoCallback()
        {
            serializedObject.Update();
            InitialiseEditorValues();
            EditorUtility.SetDirty(target);
        }

        private void InitialiseEditorValues()
        {
            GetArManagerReferenceLibrary();
            GetReferenceImages();
            _trackingModesDict = (target as SpacesReferenceImageConfigurator)?.CreateTrackingModesDictionary() ?? new Dictionary<string, SpacesImageTrackingMode>();

            // store the initial values (read in from serialized scriptable object ground truth) in semi-permanent data
            foreach (var trackingModeKvp in _trackingModesDict)
            {
                SetLastSavedValue(trackingModeKvp.Key, trackingModeKvp.Value);
            }
        }

        private void GetArManagerReferenceLibrary()
        {
            _arManagerReferenceLibrary = _trackedImageManager.FindProperty("m_SerializedLibrary");
        }

        private void GetReferenceImages()
        {
            if (_arManagerReferenceLibrary?.objectReferenceValue != null)
            {
                _referenceImages = new SerializedObject(_arManagerReferenceLibrary.objectReferenceValue).FindProperty("m_Images");
            }
            else
            {
                _referenceImages = null;
            }
        }

        /// <summary>
        ///     Sync valid reference image names with the contents of the tracking modes dictionary.
        ///     Removes all elements from the dictionary which are not currently considered to be valid reference image names
        ///     Use case for this: user edits a modes asset. Then removes an image from the reference library. Then reedits the
        ///     modes asset
        /// </summary>
        /// <param name="validNames">A set of all reference image names which are currently valid</param>
        /// <returns>True if any elements were removed, False otherwise.</returns>
        private bool SyncValidReferenceImageNames(HashSet<string> validNames)
        {
            bool anyRemoved = false;
            foreach (var referenceImageName in _trackingModesDict.Keys.ToList())
            {
                if (!validNames.Contains(referenceImageName))
                {
                    _trackingModesDict.Remove(referenceImageName);
                    anyRemoved = true;
                }
            }

            return anyRemoved;
        }

        /// <summary>
        ///     Set the tracking mode for the reference image with the given name
        /// </summary>
        /// <param name="referenceImageName">The name of the reference image to get the tracking mode for</param>
        /// <param name="spacesImageTrackingMode">The value to set for the tracking mode</param>
        /// <returns>True if the tracking mode was changed, False otherwise.</returns>
        private bool SetTrackingMode(string referenceImageName, SpacesImageTrackingMode spacesImageTrackingMode)
        {
            SpacesImageTrackingMode oldValue = _trackingModesDict[referenceImageName];
            if (oldValue != spacesImageTrackingMode)
            {
                SetLastSavedValue(referenceImageName, spacesImageTrackingMode);
                _trackingModesDict[referenceImageName] = spacesImageTrackingMode;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets or creates a default value for the tracking mode of the chosen reference image name.
        ///     Will attempt to repopulate the tracking mode for this reference image with previous data from the session if
        ///     possible,
        ///     otherwise initialises to a default value.
        /// </summary>
        /// <param name="referenceImageName">The name of the reference image to get the tracking mode for</param>
        /// <param name="spacesImageTrackingMode">
        ///     The tracking mode for the chosen reference image. TrackingMode.DYNAMIC by
        ///     default.
        /// </param>
        /// <returns>
        ///     True if the tracking mode was added to the configurator, False otherwise. The out value trackingMode is
        ///     correct and useable in both cases.
        /// </returns>
        private bool GetOrAddDefaultTrackingMode(string referenceImageName, out SpacesImageTrackingMode spacesImageTrackingMode)
        {
            _validTrackingModeNames.Add(referenceImageName);
            bool result = false;
            if (!_trackingModesDict.ContainsKey(referenceImageName))
            {
                var defaultTrackingMode = TryGetLastSavedValue(referenceImageName);
                _trackingModesDict.Add(referenceImageName, defaultTrackingMode);
                result = true;
            }

            spacesImageTrackingMode = _trackingModesDict[referenceImageName];
            return result;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var prevReferenceValue = _arManagerReferenceLibrary.objectReferenceValue;
            _trackedImageManager.Update();
            if (_referenceImages == null || prevReferenceValue != _arManagerReferenceLibrary.objectReferenceValue)
            {
                GetReferenceImages();
            }

            if (_referenceImages == null)
            {
                EditorGUILayout.HelpBox("Needs a valid XR Reference Image Library.\n" +
                    "Make sure that AR Tracked Image Manager has a valid Serialized Library",
                    MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            int referenceImageCount = _referenceImages.arraySize;
            if (referenceImageCount == 0)
            {
                EditorGUILayout.HelpBox("No reference images found in the selected XR Reference Image Library.", MessageType.Warning);
                if (SyncValidReferenceImageNames(new HashSet<string>()))
                {
                    SyncTrackingModes();
                }

                return;
            }

            EditorGUILayout.Space(5);

            // iterate over the list of reference images and draw the controls for them
            _validTrackingModeNames.Clear();
            bool hasAnyChanged = false;
            var library = _arManagerReferenceLibrary.objectReferenceValue as XRReferenceImageLibrary;
            for (int index = 0; index < referenceImageCount; ++index)
            {
                TrackingModeField(library, index, ref hasAnyChanged);
                if (index < referenceImageCount - 1)
                {
                    EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                }
            }

            hasAnyChanged |= SyncValidReferenceImageNames(_validTrackingModeNames);
            if (hasAnyChanged)
            {
                SyncTrackingModes();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static readonly GUIContent _dynamicMode = new GUIContent("Dynamic (Default)",
            "Dynamic mode updates the position of tracked images frequently, and works on moving and static targets.\n\n" +
            "If the tracked image cannot be found, no location or pose is reported.\n\n" +
            "This mode has higher power consumption relative to the other tracking modes.");

        private static readonly GUIContent _staticMode = new GUIContent("Static",
            "Static mode is useful for tracking images that are known to be static, which leads to less power consumption and greater performance." +
            "This mode is useful for continuing to show augmentations that should still be visible even after the image is no longer visible.\n\n" +
            "The position of images tracked in static mode is never updated, regardless of whether the image has moved or is out of sight.");

        private static readonly GUIContent _adaptiveMode = new GUIContent("Adaptive",
            "Adaptive mode works on static images, but periodically updates the pose of tracked images if they have moved slightly.\n\n" +
            "Tracking for images that are out of sight will eventually be lost.\n\n" +
            "This mode balances power consumption and tracking accuracy for unmoving images.");

        private static readonly GUIContent _disabledMode = new GUIContent("Disabled", "Tracking for this image target will be disabled.");

        private static readonly GUIContent[] _toolbarEntries =
        {
            _dynamicMode,
            _staticMode,
            _adaptiveMode /*, _disabledMode*/
        };

        private SpacesImageTrackingMode TrackingModeField(XRReferenceImageLibrary library, int index, ref bool hasAnyChanged)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var referenceImage = library[index];
                var texturePath = AssetDatabase.GUIDToAssetPath(referenceImage.textureGuid.ToString("N"));
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                TexturePreviewField(texture);
                var referenceImageProperty = _referenceImages.GetArrayElementAtIndex(index);
                var nameProperty = referenceImageProperty.FindPropertyRelative("m_Name");
                using (new EditorGUILayout.VerticalScope())
                {
                    if (nameProperty != null && nameProperty.stringValue != string.Empty)
                    {
                        EditorGUILayout.LabelField(new GUIContent(nameProperty.stringValue), EditorStyles.boldLabel);
                    }
                    else
                    {
                        EditorStyles.label.fontStyle = FontStyle.Italic;
                        EditorGUILayout.LabelField(new GUIContent("No reference image name"));
                        EditorStyles.label.fontStyle = FontStyle.Normal;
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PrefixLabel("Tracking Mode: ");
                    hasAnyChanged |= GetOrAddDefaultTrackingMode(nameProperty.stringValue, out var trackingMode);
                    trackingMode = (SpacesImageTrackingMode)GUILayout.Toolbar((int)trackingMode, _toolbarEntries);
                    hasAnyChanged |= SetTrackingMode(nameProperty.stringValue, trackingMode);
                    return trackingMode;
                }
            }
        }

        /// <summary>
        ///     Update the serialized lists on the target object from the editor dictionary
        /// </summary>
        private void SyncTrackingModes()
        {
            Undo.RecordObject(target, "Modified tracking mode");
            (target as SpacesReferenceImageConfigurator)?.SyncTrackingModes(_trackingModesDict);
            EditorUtility.SetDirty(target);
        }

#if !UNITY_2021_2_OR_NEWER
        private delegate void GetWidthAndHeight(TextureImporter importer, ref int width, ref int height);
        private static GetWidthAndHeight getWidthAndHeightDelegate;
#endif

        private static Vector2Int CalculateAspectRatio(Texture2D texture)
        {
            Vector2Int textureSize = new Vector2Int(maxImagePreviewSize, maxImagePreviewSize);
            if (texture == null)
            {
                return textureSize;
            }

            var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            if (textureImporter == null)
            {
                return textureSize;
            }

            int width = maxImagePreviewSize;
            int height = maxImagePreviewSize;
#if UNITY_2021_2_OR_NEWER
            textureImporter.GetSourceTextureWidthAndHeight(out width, out height);
#else
            if (getWidthAndHeightDelegate == null) {
                var methodInfo = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                getWidthAndHeightDelegate = Delegate.CreateDelegate(typeof(GetWidthAndHeight), null, methodInfo) as GetWidthAndHeight;
            }

            getWidthAndHeightDelegate(textureImporter, ref width, ref height);
#endif
            if (width > height)
            {
                textureSize.y = maxImagePreviewSize * height / width;
            }
            else
            {
                textureSize.x = maxImagePreviewSize * width / height;
            }

            return textureSize;
        }

        private static Texture2D TexturePreviewField(Texture2D texture)
        {
            Vector2Int textureSize = CalculateAspectRatio(texture);
            using (new EditorGUI.DisabledScope(true))
            {
                return (Texture2D)EditorGUILayout.ObjectField(texture,
                    typeof(Texture2D),
                    true,
                    GUILayout.Width(textureSize.x),
                    GUILayout.Height(textureSize.y));
            }
        }

        private void SetLastSavedValue(string referenceImageName, SpacesImageTrackingMode spacesImageTrackingMode)
        {
            _lastSavedValues[GetLastSavedValueLibraryKeyName(referenceImageName)] = spacesImageTrackingMode;
        }

        private SpacesImageTrackingMode TryGetLastSavedValue(string referenceImageName, SpacesImageTrackingMode defaultOnFail = SpacesImageTrackingMode.DYNAMIC)
        {
            if (_lastSavedValues.ContainsKey(GetLastSavedValueLibraryKeyName(referenceImageName)))
            {
                return _lastSavedValues[GetLastSavedValueLibraryKeyName(referenceImageName)];
            }

            return defaultOnFail;
        }

        private string GetLastSavedValueLibraryKeyName(string referenceImageName)
        {
            return _arManagerReferenceLibrary?.objectReferenceValue?.name + referenceImageName;
        }
    }
}
