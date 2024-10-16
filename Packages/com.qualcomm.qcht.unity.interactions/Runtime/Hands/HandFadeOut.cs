// /******************************************************************************
//  * File: HandFadeOut.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    public class HandFadeOut : MonoBehaviour
    {
        private static readonly int s_globalAlpha = Shader.PropertyToID("_Alpha");
        
        private const float kFadeDuration = 0.33f;

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake() {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _propertyBlock = new MaterialPropertyBlock();
        }

        public void TakeSnapShot(SkinnedMeshRenderer skinnedMeshRenderer) {
            if (skinnedMeshRenderer == null) {
                _meshFilter.mesh = null;
                _meshRenderer.material = null;
                return;
            }

            var mesh = _meshFilter.mesh;
            mesh.Clear();
            skinnedMeshRenderer.BakeMesh(mesh);
            _meshRenderer.material = skinnedMeshRenderer.material;
        }
        
        public void StartFading(float startAlpha) {
            StartCoroutine(FadeAndHide(startAlpha));
        }

        private IEnumerator FadeAndHide(float startAlpha) {
            float time = 0;
            while (time < kFadeDuration) {
                time += Time.deltaTime;
                var dt = time / kFadeDuration;
                var alpha = Mathf.Lerp(startAlpha, 0f, dt);
                SetAlpha(alpha);
                yield return null; // Wait for next frame
            }
            gameObject.SetActive(false);
        }

        private void SetAlpha(float alpha) {
            _propertyBlock.SetFloat(s_globalAlpha, alpha);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}