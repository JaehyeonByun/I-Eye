// /******************************************************************************
//  * File: HandVisualizerGhost.cs
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
    public class HandVisualizerGhost : HandVisualizer
    {
        [SerializeField] private Transform mainRoot;
        [SerializeField, MinMax(0.01f, 1f)]
        private Vector2 distanceBlendAlpha = new Vector2(0.01f, 1f);
        [SerializeField] private Vector2 size;
        [FormerlySerializedAs("_lineRenderer")] [SerializeField] private LineRenderer lineRenderer;

        private GradientAlphaKey[] _alphaKeys;

        public Transform MainRoot
        {
            get => mainRoot;
            set => mainRoot = value;
        }

        private new void Start()
        {
            base.Start();
            
            if (lineRenderer)
            {
                _alphaKeys = new GradientAlphaKey[lineRenderer.colorGradient.alphaKeys.Length];

                for (var i = 0; i < _alphaKeys.Length; i++)
                {
                    _alphaKeys[i] = lineRenderer.colorGradient.alphaKeys[i];
                }
            }
        }

        private new void Update()
        {
            if (mainRoot == null)
                return;

            var dist = GetNormalizedDistanceToMain();
            SetAlpha(dist);

            if (lineRenderer)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, mainRoot.position);
                lineRenderer.SetPosition(1, transform.position);
                var colorGradient = lineRenderer.colorGradient;
                var newAlphaKeys = new GradientAlphaKey[_alphaKeys.Length];
                for (var i = 0; i < _alphaKeys.Length; i++)
                {
                    newAlphaKeys[i].alpha = _alphaKeys[i].alpha * dist;
                }

                colorGradient.SetKeys(colorGradient.colorKeys, newAlphaKeys);
                lineRenderer.colorGradient = colorGradient;
                lineRenderer.endWidth = Mathf.Lerp(size.x, size.y, dist);
            }
        }

        public void OnEnable()
        {
            lineRenderer.enabled = true;
        }

        public void OnDisable()
        {
            lineRenderer.enabled = false;
        }

        #region ISkinnable

        protected override void UpdateHandSkin()
        {
            if (meshRenderer)
            {
                meshRenderer.sharedMesh = handSkin != null ? handSkin.GhostMesh : null;
                meshRenderer.material = handSkin != null ? handSkin.GhostMaterial: null;
            }
        }

        #endregion
        
        protected override IEnumerator FadeInAsync()
        {
            yield return Fade(GetNormalizedDistanceToMain());
        }

        private float GetNormalizedDistanceToMain()
        {
            var distance = Vector3.Distance(mainRoot.position, transform.position);
            distance = (distance - distanceBlendAlpha.x) / distanceBlendAlpha.y - distanceBlendAlpha.x;
            return Mathf.Clamp01(distance);
        }
    }
}