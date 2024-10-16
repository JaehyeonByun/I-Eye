// /******************************************************************************
//  * File: XRPointerEventData.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal
{
    public class XRPointerEventData : PointerEventData
    {
        public XRPointerEventData(EventSystem eventSystem)
            : base(eventSystem)
        {
        }

        public bool isLeftHand;
        public Ray worldSpaceRay;
        public Vector3 dragPosition;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>IsLeft</b>: " + isLeftHand);
            sb.AppendLine("<b>Position</b>: " + position);
            sb.AppendLine("<b>Press Position</b>: " + pressPosition);
            sb.AppendLine("<b>delta</b>: " + delta);
            sb.AppendLine("<b>eligibleForClick</b>: " + eligibleForClick);
            sb.AppendLine("<b>pointerEnter</b>: " + pointerEnter);
            sb.AppendLine("<b>pointerPress</b>: " + pointerPress);
            sb.AppendLine("<b>lastPointerPress</b>: " + lastPress);
            sb.AppendLine("<b>pointerDrag</b>: " + pointerDrag);
            sb.AppendLine("<b>worldSpaceRay</b>: " + worldSpaceRay);
            sb.AppendLine("<b>Use Drag Threshold</b>: " + useDragThreshold);
            return sb.ToString();
        }
    }

    public class QCHTDoublePointerEventData : BaseEventData
    {
        public QCHTDoublePointerEventData(EventSystem eventSystem) : base(eventSystem)
        {
        }

        public bool doubleDragging;
        public Vector3 doubleDragPosition;
        public Vector3 doubleDragPressPosition;
        public XRPointerEventData leftData;
        public XRPointerEventData rightData;
    }

    public static class PointerEventDataExtensions
    {
        public static bool IsQCHTPointerEventData(this PointerEventData eventData)
        {
            return eventData is XRPointerEventData;
        }

        public static Ray GetRay(this PointerEventData pointerEventData)
        {
            var vrPointerEventData = pointerEventData as XRPointerEventData;
            Assert.IsNotNull(vrPointerEventData);
            return vrPointerEventData.worldSpaceRay;
        }
    }
}