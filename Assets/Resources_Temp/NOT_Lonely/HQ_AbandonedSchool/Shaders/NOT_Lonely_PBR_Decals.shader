Shader "NOT_Lonely/NOT_Lonely_PBRDecals" {
	Properties {
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_BumpScale("Normalmap strength", Range(0.0, 10.0)) = 1.0
		_Glossiness("Glossiness", 2D) = "white" {}
		_Gloss("Gloss", Range(0.0, 1.0)) = 1.0
		_Metallic("Metallic", Range(0.0, 1.0)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "IgnoreProjector" = "True" "Queue" = "AlphaTest" }
		LOD 200
		
		//Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows decal:blend//alpha:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Glossiness;
		float _Gloss, _Metallic, _BumpScale;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 color: Color;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color  * IN.color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
			half4 glossMap = tex2D(_Glossiness, IN.uv_MainTex);
			o.Metallic = _Metallic;
			o.Smoothness = glossMap.r * _Gloss;
		}
		ENDCG
	}
	FallBack "Decal/Transparent Diffuse"
}