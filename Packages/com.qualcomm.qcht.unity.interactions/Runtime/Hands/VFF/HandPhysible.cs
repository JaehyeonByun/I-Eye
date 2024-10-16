// /******************************************************************************
//  * File: HandPhysible.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QCHT.Interactions.Hands.VFF
{
    public class HandPhysible : MonoBehaviour, IHandPhysible
    {
        [SerializeField] private HandPhysicsConfiguration configuration;
        [SerializeField] private List<HandPhysicsPart> physicsParts = new List<HandPhysicsPart>();

        private void OnValidate() =>
            physicsParts = physicsParts.Count == 0 ? GetComponentsInChildren<HandPhysicsPart>().ToList() : physicsParts;

        private void Awake()
        {
            foreach (var part in physicsParts)
            {
                var boneConfiguration = configuration.Standard;
                if (part.IsThumb) boneConfiguration = configuration.Thumb;
                if (part.IsRoot) boneConfiguration = configuration.Root;
                part.Configuration = boneConfiguration;
            }

            if (physicsParts.Count == 0)
                physicsParts = GetComponentsInChildren<HandPhysicsPart>().ToList();
        }

        #region IHandPhysible

        public bool IsPhysible
        {
            get => physicsParts.Count > 0 && physicsParts[0].IsPhysible;
            set
            {
                foreach (var part in physicsParts)
                    part.IsPhysible = value;
            }
        }

        [Obsolete]
        public void SetPhysible(bool active)
        {
            IsPhysible = active;
        }

        #endregion
    }
}