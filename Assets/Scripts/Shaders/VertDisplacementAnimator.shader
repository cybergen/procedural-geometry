﻿Shader "Custom/VertDisplacementAnimator" 
{
	Properties 
    {
        _Tess ("Tessellation", Range(1,64)) = 4
        _Scale ("Scale", Range(0,20)) = 1
		_ScaleTarget("ScaleTarget", Range(0,20)) = 0.4
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Displacement ("Displacement (RGB)", 2D) = "white" {}
		_Normal ("Normal (RGB)", 2D) = "white" {}		
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 300
        
        CGPROGRAM
        #pragma surface surf Standard addshadow fullforwardshadows vertex:disp tessellate:tessFixed
        #pragma target 5.0
        #include "Tessellation.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
        };

        float _Tess;
        float _Phong;

        float4 tessFixed()
        {
            return _Tess;
        }

        sampler2D _Displacement;
        sampler2D _Normal;
        float _Scale;
		float _ScaleTarget;

        void disp (inout appdata v)
        {
        	float4 displace = float4((tex2Dlod(_Displacement, float4(v.texcoord.xy,0,0)).xyz - 0.5) * 2, 0);
			_Scale = (sin(_Time * 20) / 2 + 0.5) * _ScaleTarget;
            v.vertex += displace * _Scale;

            float4 normal = float4((tex2Dlod(_Normal, float4(v.texcoord.xy,0,0)).xyz - 0.5) * 2, 0);
            normal.x *= -1;
            normal.z *= -1;
            v.normal = normal;
        }

        struct Input 
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        sampler2D _NormalMap;
        fixed4 _Color;
        float _Metallic;
        float _Smoothness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
