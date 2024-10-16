// /******************************************************************************
//  * File: XRHandFilter.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using QCHT.Interactions.Core;
using UnityEngine.Serialization;

namespace QCHT.Interactions.Hands
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public class XRHandFilter : MonoBehaviour, IXRSelectFilter, IXRHoverFilter
    {
        private XRBaseInteractable _interactable;
        public bool canProcess => isActiveAndEnabled;

        [FormerlySerializedAs("_selectHand")] [SerializeField]
        private SelectHand selectHand;
        public SelectHand Hand => selectHand;

        [FormerlySerializedAs("_selectGesture")] [SerializeField]
        private SelectGesture selectGesture;
        public SelectGesture Gesture => selectGesture;

        public void Awake()
        {
            _interactable = GetComponent<XRBaseInteractable>();
            if (_interactable == null)
            {
                Debug.LogWarning("[XRHandFilter:Awake] Unable to find interactable");
                enabled = false;
                return;
            }
        }

        public void OnEnable()
        {
            if (_interactable != null)
            {
                _interactable.hoverFilters.Add(this);
                _interactable.selectFilters.Add(this);
            }
        }

        public void OnDisable()
        {
            if (_interactable != null)
            {
                _interactable.hoverFilters.Remove(this);
                _interactable.selectFilters.Remove(this);
            }
        }

        /// <summary>
        /// Hovering filter process 
        /// </summary>
        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            var xrBaseInteractor = interactor as XRBaseInteractor;
            if (xrBaseInteractor == null) return false;

            return FilterHandedness(xrBaseInteractor) && FilterHandGesture(xrBaseInteractor);
        }

        /// <summary>
        /// Selecting filter process 
        /// </summary>
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            var xrBaseInteractor = interactor as XRBaseInteractor;
            if (xrBaseInteractor == null) return false;

            return FilterHandedness(xrBaseInteractor) && FilterHandGesture(xrBaseInteractor);
        }

        private bool FilterHandedness(XRBaseInteractor interactor)
        {
            var hand = interactor.GetComponentInParent<IHandedness>();
            return hand == null || CheckHand(hand.Handedness);
        }

        private bool FilterHandGesture(XRBaseInteractor interactor)
        {
            var hand = interactor.GetComponentInParent<IHandGesture>();
            return hand == null || CheckGesture(hand.Gesture);
        }

        private bool CheckHand(XrHandedness handedness) => selectHand switch
        {
            SelectHand.Both => true,
            SelectHand.Left => handedness == XrHandedness.XR_HAND_LEFT,
            SelectHand.Right => handedness == XrHandedness.XR_HAND_RIGHT,
            _ => true
        };

        private bool CheckGesture(XrHandGesture gesture) => selectGesture switch
        {
            SelectGesture.Both => true,
            SelectGesture.Pinch => gesture == XrHandGesture.XR_HAND_PINCH,
            SelectGesture.Grab => gesture == XrHandGesture.XR_HAND_GRAB,
            _ => true
        };
    }

    public enum SelectGesture
    {
        Pinch,
        Grab,
        Both
    }

    public enum SelectHand
    {
        Both,
        Right,
        Left
    }
}