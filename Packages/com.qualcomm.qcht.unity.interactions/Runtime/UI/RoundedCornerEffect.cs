// /******************************************************************************
//  * File: RoundedCornerEffect.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace QCHT.Interactions.UI
{
	[ExecuteInEditMode]
	public class RoundedCornerEffect : BaseMeshEffect
	{
		[SerializeField] private float _cornerRadius = 1.0f;

		public override void ModifyMesh(VertexHelper vertexHelper)
		{
			var v = new UIVertex();
			var rectTransform = GetComponent<RectTransform>().rect;
			for (int i = 0; i < vertexHelper.currentVertCount; i++)
			{
				vertexHelper.PopulateUIVertex(ref v, i);
				v.uv1 = new Vector2(rectTransform.width, rectTransform.height);
				v.uv2 = new Vector2(_cornerRadius, 0f);
				vertexHelper.SetUIVertex(v, i);
			}
		}
	}
}