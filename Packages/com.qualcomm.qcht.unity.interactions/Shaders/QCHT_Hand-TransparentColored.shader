// /******************************************************************************
//  * File: QCHT_Hand-TransparentColored.shader
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

Shader "Qualcomm/Hand/TransparentColored"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        [HideInInspector]_Alpha("Hand Alpha", Range(0,1)) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    ENDCG

    CGINCLUDE
    #pragma multi_compile_fog
    
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;
    fixed4 _Color;
    float _Alpha;

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        UNITY_TRANSFER_FOG(o, o.vertex);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        fixed4 col = _Color;
        UNITY_APPLY_FOG(i.fogCoord, col);
        return col * _Alpha;
    }
    ENDCG

    // URP
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent"
        }
        
        Pass
        {
            Name "HAND"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }

    // Built-in
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType" = "Transparent"
        }

        Pass
        {
            Name "HAND"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}