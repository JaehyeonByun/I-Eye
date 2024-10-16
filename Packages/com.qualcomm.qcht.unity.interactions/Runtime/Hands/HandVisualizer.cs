// /******************************************************************************
//  * File: HandVisualizer.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace QCHT.Interactions.Hands
{
    public partial class HandVisualizer : MonoBehaviour, IHandSkinnable, IHideable
    {
        private static readonly int s_globalAlpha = Shader.PropertyToID("_Alpha");
        private static readonly int s_userAlpha = Shader.PropertyToID("_OverrideAlpha");

        private const float kFadeDuration = 0.33f;

        private HandFadeOut _fadeOutInstance; // Use one single fade out instance

        [SerializeField] protected SkinnedMeshRenderer meshRenderer;

        [FormerlySerializedAs("_handSkin")] [SerializeField]
        protected HandSkin handSkin;

        private MaterialPropertyBlock _propertyBlock;

        private Coroutine _coroutine;

        protected void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _fadeOutInstance = CreateFadeInstance();

            UpdateHandSkin();
        }

        protected void Start() { }

        protected void Update()
        {
            if (handSkin.hasFeedbacks)
            {
                UpdateFeedbacks();
            }
        }

        protected void OnDestroy()
        {
            if (_fadeOutInstance != null)
            {
                Destroy(_fadeOutInstance.gameObject);
            }
        }

        #region IHandSkinnable

        public HandSkin HandSkin
        {
            get => handSkin;
            set
            {
                handSkin = value;
                UpdateHandSkin();
                
                UnRegisterFeedbackInteractors();
                
                if (handSkin.hasFeedbacks)
                {
                    RegisterFeedbackInteractors();
                    InitFeedbacks();
                }
            }
        }

        protected virtual void UpdateHandSkin()
        {
            if (meshRenderer)
            {
                meshRenderer.sharedMesh = handSkin != null ? handSkin.MainMesh : null;
                meshRenderer.material = handSkin != null ? handSkin.MainMaterial : null;
            }
        }

        #endregion

        #region IHandFadable

        public void Hide()
        {
            if (_fadeOutInstance)
            {
                _fadeOutInstance.gameObject.SetActive(true);
                var fadeOutInstanceTransform = _fadeOutInstance.transform;
                var thisTransform = transform;
                fadeOutInstanceTransform.position = thisTransform.position;
                fadeOutInstanceTransform.rotation = thisTransform.rotation * Quaternion.AngleAxis(90f, Vector3.left);
                _fadeOutInstance.TakeSnapShot(meshRenderer);
                _fadeOutInstance.StartFading(GetAlpha());
            }

            SetAlpha(0f); // Set alpha to 0, for next show call
        }

        public void Show()
        {
            if (_fadeOutInstance)
            {
                _fadeOutInstance.gameObject.SetActive(false);
            }

            if (_coroutine != null) StopCoroutine(_coroutine);
            if (isActiveAndEnabled) _coroutine = StartCoroutine(FadeInAsync());
        }

        private static HandFadeOut CreateFadeInstance()
        {
            // Instantiate fade out instance
            var go = new GameObject("FadeOutHand")
            {
                hideFlags = HideFlags.HideInHierarchy
            };
            return go.AddComponent<HandFadeOut>();
        }

        protected virtual IEnumerator FadeInAsync()
        {
            yield return Fade(1f);
        }

        protected IEnumerator Fade(float targetAlpha)
        {
            var startAlpha = GetAlpha();
            float time = 0;
            while (time < kFadeDuration)
            {
                time += Time.deltaTime;
                var dt = time / kFadeDuration;
                var alpha = Mathf.Lerp(startAlpha, targetAlpha, dt);
                SetAlpha(alpha);
                yield return null; // Wait for next frame
            }
        }

        protected void SetAlpha(float alpha)
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.SetFloat(s_globalAlpha, alpha);
            if (meshRenderer) meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        protected float GetAlpha()
        {
            if (!meshRenderer) return 0f;
            _propertyBlock ??= new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(_propertyBlock);
            return _propertyBlock.GetFloat(s_globalAlpha);
        }

        protected void SetOptionalAlpha(float alpha)
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.SetFloat(s_userAlpha, alpha);
            if (meshRenderer) meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        #endregion
    }
}