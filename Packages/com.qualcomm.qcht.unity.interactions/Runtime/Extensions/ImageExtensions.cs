// /******************************************************************************
//  * File: ImageExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace QCHT.Interactions.Extensions
{
    public static class ImageExtensions
    {
        public static void SetAlpha(this Image image, float value)
        {
            image.color = new Color(image.color.r,
                image.color.g,
                image.color.b,
                value);
        }

        public static float GetAlpha(this Image image)
        {
            return image.color.a;
        }

        public static IEnumerator FadeOut(this Image image, float duration)
        {
            //if (!image.enabled) image.enabled = true;
            for (float i = duration; i >= 0; i -= Time.deltaTime)
            {
                image.SetAlpha(i / duration);
                yield return null;
            }
        }

        public static IEnumerator FadeIn(this Image image, float duration)
        {
            //if (!image.enabled) image.enabled = true;
            for (float i = 0; i <= duration; i += Time.deltaTime)
            {
                image.SetAlpha(i / duration);
                yield return null;
            }
        }
        
        public static bool IsFullyFadeOut(this Image image)
        {
            bool fully = image.color.a <= 0.1f;
            if (fully)
            {
                image.SetAlpha(0.0f);
                //image.enabled = false;
            }

            return fully;
        }

        public static bool IsFullyFadeIn(this Image image)
        {
            bool fully = image.color.a >= 0.9f;
            if (fully)
            {
                image.SetAlpha(1.0f);
            }

            return fully;
        }
    }
}
