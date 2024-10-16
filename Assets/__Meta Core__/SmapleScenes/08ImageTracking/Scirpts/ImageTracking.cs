using Qualcomm.Snapdragon.Spaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracking : MonoBehaviour
{
    [Serializable]
    public struct TrackableInfo
    {
        public string TrackingStatusText;
    }

    public ARTrackedImageManager arImageManager;
    public SpacesReferenceImageConfigurator referenceImageConfigurator;
    public TrackableInfo[] trackableInfos;
    private readonly string _referenceImageName = "Spaces Town";
    private readonly Dictionary<TrackableId, TrackableInfo> _trackedImages = new Dictionary<TrackableId, TrackableInfo>();

    public  void OnEnable()
    {
        arImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        if (referenceImageConfigurator.HasReferenceImageTrackingMode(_referenceImageName))
        {
            switch (referenceImageConfigurator.GetTrackingModeForReferenceImage(_referenceImageName))
            {
                case SpacesImageTrackingMode.STATIC:
                    break;
                case SpacesImageTrackingMode.DYNAMIC:
                    break;
                case SpacesImageTrackingMode.ADAPTIVE:
                    break;
                case SpacesImageTrackingMode.INVALID:
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"Could not find reference image: {_referenceImageName} ");
        }
    }

    public  void OnDisable()
    {
        arImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        foreach (var trackedImage in _trackedImages)
        {
            referenceImageConfigurator.StopTrackingImageInstance(_referenceImageName, trackedImage.Key);
        }
    }

 

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            if (trackedImage.referenceImage.name == _referenceImageName)
            {
                _trackedImages.Add(trackedImage.trackableId, trackableInfos[0]);
                UpdateTrackedText(trackedImage, trackableInfos[0]);
            }
        }

        foreach (var trackedImage in args.updated)
        {
            if (_trackedImages.TryGetValue(trackedImage.trackableId, out TrackableInfo info))
            {
                UpdateTrackedText(trackedImage, info);
            }
        }

        foreach (var trackedImage in args.removed)
        {
            if (_trackedImages.TryGetValue(trackedImage.trackableId, out TrackableInfo info))
            {
                _trackedImages.Remove(trackedImage.trackableId);
            }
        }
    }

    // Updates Tracked Image UI texts.
    private void UpdateTrackedText(ARTrackedImage trackedImage, TrackableInfo info)
    {
        Vector3 position = trackedImage.transform.position;
    }

 
}
