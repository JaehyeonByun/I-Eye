// /******************************************************************************
//  * File: QCHT_ControlBox.shader
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

Shader "Qualcomm/ControlBox"
{
    Properties
    {
        [HideInInspector] _MainTex ("MainTex", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _LineColor1("Line Color 1", Color) = (1,1,1,1)
        _LineColor2("Line Color 2", Color) = (1,1,1,1)
        [HDR]_LightColor("Light Color", Color) = (1,1,1,1)
        _LineWidth("Line Width", Range(0,1)) = 0.01
        _Power("Power", Float) = 1.0
        [HideInInspector]_LightPosL ("Left Light Position", Vector) = (0,0,0,0)
        [HideInInspector]_LightPosR ("Right Light Position", Vector) = (0,0,0,0)
        [Toggle]_Toggle ("Simple sided", Int) = 1
    }

    CGINCLUDE

	#pragma multi_compile _ LIGHT_L
   	#pragma multi_compile _ LIGHT_R
    #pragma multi_compile _ SELECTED

    #include "UnityCG.cginc"
    
    struct v2f
    {
        float4 pos : SV_POSITION;
        float4 color: COLOR;
        float2 uv : TEXCOORD0;
        #if defined(LIGHT_L) || defined(LIGHT_R)
        float3 wPos: TEXCOORD1;
        #endif
    };
    
    fixed4 _Color;
    fixed4 _LineColor1;
    fixed4 _LineColor2;
    float _LineWidth;
    float _Power;
    
    #if defined(LIGHT_L) || defined(LIGHT_R)
    fixed4 _LightColor;
    #endif

    #ifdef LIGHT_L
    float4 _LightPosL;
    #endif

    #ifdef LIGHT_R
    float4 _LightPosR;
    #endif

    sampler2D _MainTex;
    float4 _MainTex_ST;
    
    float distanceMap(float3 a, float3 b)
    {
        return 1 - clamp(distance(a, b) * _Power, 0, 1);
    }

    v2f vert(appdata_base v)
    {
        v2f o;
        o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.pos = UnityObjectToClipPos(v.vertex);
        #if defined(LIGHT_L) || defined(LIGHT_R)
        o.wPos = mul(unity_ObjectToWorld, v.vertex);
        #endif
        o.color = _Color;
        return o;
    }
    
    fixed4 frag(v2f IN) : SV_Target
    {
        fixed4 color = _Color;
        float l1 = 0.f;
        float l2 = 0.f;
        float2 uv = IN.uv;
        float l = _LineWidth;
        float t = 0;
        #ifdef LIGHT_L
        l1 = distanceMap(_LightPosL, IN.wPos);
        #endif

        #ifdef LIGHT_R
        l2 = distanceMap(_LightPosR, IN.wPos);
        #endif

        #if defined(LIGHT_L) || defined(LIGHT_R)
        t = (pow(l1, 8) + pow(l2, 8));
        color += _LightColor * t;
        #endif

        if (uv.x < l || uv.x > 1 - l || uv.y < l || uv.y > 1 - l)
        {
            #if defined(SELECTED)
            float s;

            if(uv.x < l)
                s = -uv.y;
            else if(uv.y < l)
                s = uv.x;
            else if(uv.x > 1-l)
                s = uv.y;
            else if(uv.y > 1-l)
                s = -uv.x;
            
            float d = .5f + sin(s * 5.f + _Time * 100.f) * 0.5f;
            color = lerp(_LineColor1, _LineColor2, float4(d,d,d,d));
            #else
            color = _LineColor1 + color;
            #endif
        }

        return color;
    }
    
	ENDCG

    //URP
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull [_Toggle]

        Pass
        {
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
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }

    FallBack "Diffuse"
}