// /******************************************************************************
//  * File: QCHT_RobotHand.shader
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

Shader "Qualcomm/Hand/RobotHand"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _EmissiveTex ("Emissive (RGB)", 2D) = "white" {}
        _MapId ("MapID (RGB)", 2D) = "black" {}

        [HDR]_ColorThumb("Color Thumb", Color) = (1,1,1,1)
        [HDR]_ColorIndex("Color Index", Color) = (1,1,1,1)
        [HDR]_ColorMiddle("Color Middle", Color) = (1,1,1,1)
        [HDR]_ColorRing("Color Ring", Color) = (1,1,1,1)
        [HDR]_ColorPinky("Color Pinky", Color) = (1,1,1,1)
        [HDR]_ColorPalm("Color Palm", Color) = (1,1,1,1)

        _ColorThumbId("Id Thumb", Color) = (1,0,1,1)
        _ColorIndexId("Id Index", Color) = (1,1,0,1)
        _ColorMiddleId("Id Middle", Color) = (0,0,1,1)
        _ColorRingId("Id Ring", Color) = (0,1,0,1)
        _ColorPinkyId("Id Pinky", Color) = (1,0,0,1)
        _ColorPalmId("ID Palm", Color) = (0,1,1,1)
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    fixed4 _ColorThumb;
    fixed4 _ColorIndex;
    fixed4 _ColorMiddle;
    fixed4 _ColorRing;
    fixed4 _ColorPinky;
    fixed4 _ColorPalm;

    fixed4 _ColorThumbId;
    fixed4 _ColorIndexId;
    fixed4 _ColorMiddleId;
    fixed4 _ColorRingId;
    fixed4 _ColorPinkyId;
    fixed4 _ColorPalmId;

    bool equColor(half4 col1, half4 col2)
    {
        return col1.r == col2.r && col1.g == col2.g && col1.b == col2.b;
    }

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float2 uv_MainTex : TEXCOORD0;
        float2 uv_EmissiveTex : TEXCOORD1;
        float2 uv2_MapId : TEXCOORD2;
    };

    sampler2D _MainTex;
    sampler2D _EmissiveTex;
    sampler2D _MapId;

    float4 _MainTex_ST;
    float4 _EmissiveTex_ST;
    float4 _MapId_ST;

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);
        o.uv_EmissiveTex = TRANSFORM_TEX(v.uv, _EmissiveTex);
        o.uv2_MapId = TRANSFORM_TEX(v.uv, _MapId);
        
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        fixed4 c = tex2D(_MainTex, i.uv_MainTex);
        fixed4 e = tex2D(_EmissiveTex, i.uv_EmissiveTex);
        fixed4 id = tex2D(_MapId, i.uv2_MapId);

        fixed3 emiCol = fixed3(0, 0, 0);
        
        if (equColor(id, _ColorThumbId)) emiCol = _ColorThumb.xyz;
        else if (equColor(id, _ColorIndexId)) emiCol = _ColorIndex.xyz;
        else if (equColor(id, _ColorMiddleId)) emiCol = _ColorMiddle.xyz;
        else if (equColor(id, _ColorRingId)) emiCol = _ColorRing.xyz;
        else if (equColor(id, _ColorPinkyId)) emiCol = _ColorPinky.xyz;
        else if (equColor(id, _ColorPalmId)) emiCol = _ColorPalm.xyz;
        
        c.rgb += emiCol * e.xyz;

        return c;
    }
    ENDCG

    // URP
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "RenderType" = "Opaque"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }

    // Standard built-in
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry" "RenderType"="Opaque"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }

    Fallback "Diffuse"
}