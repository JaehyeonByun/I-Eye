// /******************************************************************************
//  * File: XRPokeReticle.cs
//  * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace QCHT.Interactions.Core
{
    [RequireComponent(typeof(XRPokeInteractor))]
    public class XRPokeReticle : MonoBehaviour
    {
        private XRPokeInteractor _pokeInteractor;

        [FormerlySerializedAs("reticlePrefab")] [SerializeField] private GameObject prefab;

        public GameObject Prefab
        {
            get => prefab;
            set
            {
                if (ReferenceEquals(value, prefab)) 
                    return;

                prefab = value;
                
                DestroyReticle();
                InstantiateReticle();
            }
        }

        [SerializeField] private Sprite hoverSprite;

        public Sprite HoverSprite
        {
            get => hoverSprite;
            set => hoverSprite = value;
        }

        [SerializeField] private Color hoverMinColor;

        public Color HoverMinColor
        {
            get => hoverMinColor;
            set => hoverMinColor = value;
        }
        
        [SerializeField] private Color hoverMaxColor;

        public Color HoverMaxColor
        {
            get => hoverMaxColor;
            set => hoverMaxColor = value;
        }
        
        [SerializeField] private Sprite selectSprite;

        public Sprite SelectSprite
        {
            get => selectSprite;
            set => selectSprite = value;
        }

        [SerializeField] private Color selectColor;

        public Color SelectColor
        {
            get => selectColor;
            set => selectColor = value;
        }

        [Tooltip("Reticle size when selecting")]
        [Range(0, 2f), SerializeField] private float selectSize;

        public float SelectSize
        {
            get => selectSize;
            set => selectSize = value;
        }

        [Tooltip("Reticle size when far hovering")]
        [Range(0, 2f), SerializeField] private float maxSize;

        public float MaxSize
        {
            get => maxSize;
            set => maxSize = value;
        }

        [Tooltip("Reticle size when near hovering")]
        [Range(0, 2f), SerializeField] private float minSize;

        public float MinSize
        {
            get => minSize;
            set => minSize = value;
        }
        
        private GameObject _reticle;
        private SpriteRenderer _spriteRenderer;
        
        private bool _isOnUI;
        
        private void Awake()
        {
            _pokeInteractor = GetComponent<XRPokeInteractor>();
        }

        private void OnEnable()
        {
            if (_reticle == null)
            {
                InstantiateReticle();
            }
            
            _pokeInteractor.uiHoverEntered.AddListener(OnUIEntered);
            _pokeInteractor.uiHoverExited.AddListener(OnUIExited);
        }
        
        private void OnDisable()
        {
            _pokeInteractor.uiHoverEntered.RemoveListener(OnUIEntered);
            _pokeInteractor.uiHoverExited.RemoveListener(OnUIExited);
            
            SetReticleActive(false);
        }

        private void OnDestroy()
        {
            DestroyReticle();
        }
        
        private void Update()
        {
            if (_isOnUI)
            {
                if (_pokeInteractor.TryGetUIModel(out var model))
                {
                    if (_reticle != null)
                    {
                        _reticle.transform.position = model.currentRaycast.worldPosition;
                        _reticle.transform.rotation = Quaternion.LookRotation(model.currentRaycast.worldNormal);

                        if (model.select)
                        {
                            _reticle.transform.localScale = Vector3.one * selectSize;

                            if (_spriteRenderer != null)
                            {
                                _spriteRenderer.sprite = selectSprite;
                                _spriteRenderer.color = selectColor;
                            }
                        }
                        else
                        {
                            var dist = 0f;
                            if (_pokeInteractor.pokeHoverRadius > Mathf.Epsilon)
                            {
                                dist = Vector3.Distance(model.currentRaycast.worldPosition,
                                    _pokeInteractor.transform.position) / _pokeInteractor.pokeHoverRadius;
                            }
                            
                            _reticle.transform.localScale = Vector3.one * Mathf.Lerp(minSize, maxSize, dist);
                            
                            if (_spriteRenderer != null)
                            {
                                _spriteRenderer.sprite = hoverSprite;
                                _spriteRenderer.color = Color.Lerp(hoverMinColor, hoverMaxColor, dist);
                            }
                        }
                    }
                }
            }
            else
            {
                // TODO: On 3D object
            }
        }
        
        private void OnUIEntered(UIHoverEventArgs arg0)
        {
            _isOnUI = true;
            SetReticleActive(true);
        }
        
        private void OnUIExited(UIHoverEventArgs arg0)
        {
            _isOnUI = false;
            SetReticleActive(false);
        }

        private void InstantiateReticle()
        {
            if (prefab == null)
            {
                return;
            }
            
            _reticle = Instantiate(prefab);

            if (_reticle == null)
            {
                return;
            }
            
            _reticle.SetActive(false);
            _spriteRenderer = _reticle.GetComponentInChildren<SpriteRenderer>();
        }
        
        private void DestroyReticle()
        {
            if (_reticle == null)
            {
                return;
            }
            
            Destroy(_reticle);
            _reticle = null;
        }
        
        private void SetReticleActive(bool enable)
        {
            if (_reticle == null)
            {
                return;
            }
            
            _reticle.SetActive(enable);
        }
    }
}

