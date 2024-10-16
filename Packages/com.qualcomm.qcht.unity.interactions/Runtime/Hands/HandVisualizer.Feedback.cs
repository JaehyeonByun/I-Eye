using System;
using System.Collections.Generic;
using System.Linq;
using QCHT.Interactions.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Hands
{
    public enum HandPart
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky,
        Palm
    }

    public enum AnimType
    {
        None,
        PingPong,
        Increase,
        Decrease
    }

    [Serializable]
    public struct HandPartState
    {
        public HandPart HandPart;
        [ColorUsage(true, true)]
        public Color Color;
        public AnimType AnimType;
        public Vector2 IntensityRange;
        public float AnimDuration;
    }

    public partial class HandVisualizer
    {
        [SerializeField] private XrHandedness xrHandedness;

        private XRHandTrackingSubsystem _subsystem;

        private List<XRBaseInteractor> _interactors = new List<XRBaseInteractor>();

        private readonly List<XRBaseInteractable> _selectedInteractables = new List<XRBaseInteractable>();
        private readonly List<XRBaseInteractable> _hoveredInteractables = new List<XRBaseInteractable>();

        private float _animTime;

        private void OnEnable()
        {
            _subsystem ??= XRHandTrackingSubsystem.GetSubsystemInManager();

            if (handSkin.hasFeedbacks)
            {
                RegisterFeedbackInteractors();
            }
        }

        private void OnDisable()
        {
            UnRegisterFeedbackInteractors();
        }

        private void UpdateFeedbacks()
        {
            var t = 0f;
            ref var time = ref t;
            for (var i = 0; i <= (int) HandPart.Palm; i++)
            {
                var state = new HandPartState();

                // Selected
                var selectedInteractable = _selectedInteractables.LastOrDefault();
                if (selectedInteractable != null)
                {
                    // Is modified by interactable?
                    if (selectedInteractable.TryGetComponent<IHandFeedbackModifier>(out var selectModifier))
                    {
                        foreach (var s in selectModifier.Selected)
                        {
                            if (s.HandPart == (HandPart) i)
                            {
                                state = s;
                                goto update;
                            }
                        }
                    }

                    foreach (var s in handSkin.Selected)
                    {
                        if (s.HandPart == (HandPart) i)
                        {
                            state = s;
                            goto update;
                        }
                    }
                }

                // Hovered
                var hoveredInteractable = _hoveredInteractables.LastOrDefault();
                if (hoveredInteractable != null)
                {
                    // Is modified by interactable?
                    if (hoveredInteractable.TryGetComponent<IHandFeedbackModifier>(out var hoveredModifier))
                    {
                        foreach (var s in hoveredModifier.Hovered)
                        {
                            if (s.HandPart == (HandPart) i)
                            {
                                state = s;
                                goto update;
                            }
                        }
                    }

                    foreach (var s in handSkin.Hovered)
                    {
                        if (s.HandPart == (HandPart) i)
                        {
                            state = s;
                            goto update;
                        }
                    }
                }

                // Idle
                foreach (var s in handSkin.Idle)
                {
                    if (s.HandPart == (HandPart) i)
                    {
                        state = s;
                        goto update;
                    }
                }

                update:
                UpdateFinger(state);
            }
        }

        private void RegisterFeedbackInteractors()
        {
            _interactors = new List<XRBaseInteractor>();

            var interactors = FindObjectsOfType<XRBaseInteractor>();
            foreach (var interactor in interactors)
            {
                var handedness = interactor.GetComponentInParent<XRHandedness>();
                if (handedness != null && handedness.Handedness == xrHandedness)
                {
                    _interactors.Add(interactor);
                }
            }

            foreach (var interactor in _interactors)
            {
                interactor.hoverEntered.AddListener(HoverEntered);
                interactor.hoverExited.AddListener(HoverExited);

                interactor.selectEntered.AddListener(SelectEntered);
                interactor.selectExited.AddListener(SelectExited);
            }
        }

        private void UnRegisterFeedbackInteractors()
        {
            _selectedInteractables.Clear();
            _hoveredInteractables.Clear();

            foreach (var interactor in _interactors)
            {
                interactor.hoverEntered.RemoveListener(HoverEntered);
                interactor.hoverExited.RemoveListener(HoverExited);

                interactor.selectEntered.RemoveListener(SelectEntered);
                interactor.selectExited.RemoveListener(SelectExited);
            }
        }

        private void InitFeedbacks()
        {
            for (var i = 0; i <= (int) HandPart.Palm; i++)
            {
                var found = handSkin.Idle.Any(s => s.HandPart == (HandPart) i);

                if (found)
                    continue;

                var propertyColor = GetPropertyColorFromHandPart((HandPart) i);
                if (meshRenderer.material.HasColor(propertyColor))
                {
                    var handPartState = new HandPartState
                    {
                        HandPart = (HandPart) i,
                        Color = meshRenderer.material.GetColor(propertyColor)
                    };

                    handSkin.Idle.Add(handPartState);
                }
            }
        }

        private void HoverEntered(HoverEnterEventArgs arg)
        {
            var interactable = arg.interactableObject as XRBaseInteractable;
            if (interactable == null)
                return;

            _hoveredInteractables.Add(interactable);

            _animTime = Time.time;
        }

        private void HoverExited(HoverExitEventArgs arg)
        {
            var interactable = arg.interactableObject as XRBaseInteractable;
            if (interactable == null)
                return;

            _hoveredInteractables.Remove(interactable);
        }

        private void SelectEntered(SelectEnterEventArgs arg)
        {
            var baseInteractable = arg.interactableObject as XRBaseInteractable;
            if (baseInteractable == null)
                return;

            _selectedInteractables.Add(baseInteractable);

            _animTime = Time.time;
        }

        private void SelectExited(SelectExitEventArgs arg)
        {
            var interactable = arg.interactableObject as XRBaseInteractable;
            if (interactable == null)
                return;

            _selectedInteractables.Remove(interactable);
        }

        private void UpdateFinger(in HandPartState handPart)
        {
            var intensity = 0f;
            var scaledTime = (Time.time - _animTime) / handPart.AnimDuration;
            switch (handPart.AnimType)
            {
                case AnimType.PingPong:
                {
                    var range = (handPart.IntensityRange.y - handPart.IntensityRange.x) * .5F;
                    intensity = range + range * Mathf.Sin(scaledTime * Mathf.PI * 2f);
                    break;
                }
                case AnimType.Increase:
                    intensity = Mathf.Lerp(handPart.IntensityRange.x, handPart.IntensityRange.y, scaledTime);
                    break;
                case AnimType.Decrease:
                    intensity = Mathf.Lerp(handPart.IntensityRange.y, handPart.IntensityRange.x, scaledTime);
                    break;
                case AnimType.None:
                    intensity = 1f;
                    break;
            }

            var propertyColor = GetPropertyColorFromHandPart(handPart.HandPart);
            if (meshRenderer.material.HasColor(propertyColor))
                meshRenderer.material.SetColor(propertyColor, handPart.Color * intensity);
        }

        private static string GetPropertyColorFromHandPart(in HandPart part) =>
            part switch
            {
                HandPart.Thumb => "_ColorThumb",
                HandPart.Index => "_ColorIndex",
                HandPart.Middle => "_ColorMiddle",
                HandPart.Ring => "_ColorRing",
                HandPart.Pinky => "_ColorPinky",
                HandPart.Palm => "_ColorPalm",
                _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
            };
    }
}