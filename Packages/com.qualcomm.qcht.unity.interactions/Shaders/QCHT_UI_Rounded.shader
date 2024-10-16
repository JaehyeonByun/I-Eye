// /******************************************************************************
//  * File: QCHT_UI_Rounded.shader
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

Shader "Qualcomm/UI/Rounded" 
{
    Properties 
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [Toggle] _UseDistance("Use distance", FLoat) = 0
        _Edge1("Edge1", Range(0, 1)) = 0
        _Edge2("Edge2", Range(0, 1)) = 0
    }

    SubShader 
    {
        Tags 
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil 
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        //ZTest [unity_GUIZTestMode]
        Offset 1, 1 
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"

            //#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 uv1 : TEXCOORD1;
                float3 uv2 : TEXCOORD2;
                float4 color : COLOR; // set from Image component property
            };

            struct v2f 
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 uv1 : TEXCOORD1;
                float3 uv2 : TEXCOORD2;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _UseDistance;
            float _Edge1;
            float _Edge2;
            
            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = v.uv1;
                o.uv2 = v.uv2;
                o.color = v.color;
                return o;
            }

            float RClip(float2 uv, float w, float h, float r)
            {
	            float2 wh = float2(w, h);
	            float2 nUV = (uv - 0.5) * wh + .5;
	            float2 muv = abs(nUV * 2 - 1) - wh + r * 2 ;
	            float d = length(max(0, muv)) / (r * 2);

                float fw = fwidth(d);
	            if(fw <= .0) fw = 0.001;
	            clip(saturate((1. - d) / fw) - 0.001);
                return d;
            }

            fixed4 frag (v2f i) : SV_Target 
            {
                float d = RClip(i.uv, i.uv1.x, i.uv1.y, i.uv2.x);
                
                if (_UseDistance > 0)
                {
                   i.color.a *= smoothstep(0 + _Edge1, 1 - _Edge2,  d);
                }

                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
    }
}
