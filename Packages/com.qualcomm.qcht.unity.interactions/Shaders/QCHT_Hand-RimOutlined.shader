// /******************************************************************************
//  * File: QCHT_Hand-RimOutlined.shader
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

Shader "Qualcomm/Hand/RimOutlined"
{
    Properties
    {
        [HideInInspector]_Alpha("Hand Alpha", Range(0,1)) = 1 // Handled by the HandPresenter when hand is detected
        _OverrideAlpha("Override Alpha", Range(0,1)) = 1
        _Color("Main Color", Color) = (1, 1, 1, 1)
        _FingerIdMap("Finger Id Map", 2D) = "white" {}
        [MaterialToggle] _isRimInverted("isRimInverted", Float) = 0
        _RimColor("Rim Color", Color) = (0.26, 0.19, 0.16, 0.0)
        _RimPower("Rim Power", Range(0.01, 20.0)) = 3.0
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline width", Range(0, 0.01)) = 0.0
        [HDR] _ColorIndex("Color Index", Color) = (1, 1, 1, 0)
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    ENDCG

    CGINCLUDE
    uniform sampler2D _FingerIdMap;
    float4 _FingerIdMap_ST;

    uniform float _Alpha;
    uniform float _OverrideAlpha;

    uniform half4 _Color;
    uniform half4 _RimColor;
    uniform half4 _ColorIndex;
    uniform float _isRimInverted;
    uniform float _RimPower;

    uniform float _OutlineWidth;
    uniform float4 _OutlineColor;

    struct v2f
    {
        float4 pos : SV_POSITION;
        float4 color: COLOR;
        float2 uv : TEXCOORD0;
        float3 wPos : TEXCOORD1;
        float3 normal : TEXCOORD2;
    };

    v2f vert(appdata_base v)
    {
        v2f o;
        o.uv = TRANSFORM_TEX(v.texcoord, _FingerIdMap);
        o.pos = UnityObjectToClipPos(v.vertex);
        o.wPos = mul(unity_ObjectToWorld, v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.color = _Color;
        return o;
    }

    fixed4 frag(const v2f i) : SV_Target
    {
        const float3 v = normalize(_WorldSpaceCameraPos - i.wPos);
        const float r = 1. - pow(max(0, dot(v, i.normal)), _RimPower);
        const fixed4 id = tex2D(_FingerIdMap, i.uv);
        fixed4 color = lerp(i.color, _RimColor, r * id.r);
        color = lerp(color, _ColorIndex, (id.g + id.b) * color.a * _ColorIndex.a);
        color.a = abs(_isRimInverted - color.a);
        return color * _Alpha * _OverrideAlpha;
    }

    v2f vertOutline(appdata_base v)
    {
        v2f o = (v2f)0;
        o.uv = TRANSFORM_TEX(v.texcoord, _FingerIdMap);
        v.vertex.xyz += v.normal * _OutlineWidth;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.color = _OutlineColor;
        return o;
    }

    half4 fragOutline(v2f i) : SV_Target
    {
        const fixed4 id = tex2D(_FingerIdMap, i.uv);
        i.color = lerp(i.color, _ColorIndex, (id.g + id.b) * id.a * _ColorIndex.a) * id.r;
        return i.color * _Alpha * _OverrideAlpha;
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
            Name "DEPTH-PREPASS"
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }
            ZWrite On
            Cull Off
            ColorMask 0
        }

        Pass
        {
            Name "DEPTH-ONLY"
            Tags
            {
                "LightMode" = "DepthOnly"
            }
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "OUTLINE"
            Tags
            {
                "LightMode" = "UniversalForwardOnly" // Dirty tweak to apply another pass
            }
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Less
            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
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
            Name "DEPTH-PREPASS"
            ZWrite On
            Cull Off
            ColorMask 0
        }

        Pass
        {
            Name "HAND"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "OUTLINE"
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Less
            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            ENDCG
        }
    }

    Fallback "Diffuse"
}