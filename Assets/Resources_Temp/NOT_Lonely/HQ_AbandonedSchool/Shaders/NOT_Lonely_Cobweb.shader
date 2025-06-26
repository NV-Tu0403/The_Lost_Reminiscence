Shader "NOT_Lonely/NOT_Lonely_Cobweb"
{
	Properties
	{
		_BaseColor("BaseColor", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_EdgeFade("Edge Fade", Range( 0 , 1)) = 0.5
		_CameraFadeDistance("Camera Fade Distance", Range( 0 , 20)) = 3
		_AnimSpeed("Anim Speed", Float) = 1
		_Amplitude("Amplitude", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector" = "True" "Queue" = "AlphaTest" }
		Cull Back

		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.5
		#pragma surface surf Standard fullforwardshadows decal:blend vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float eyeDepth;
			float2 texcoord_0;
		};

		uniform float4 _Color;
		uniform sampler2D _BaseColor;
		uniform float4 _BaseColor_ST;
		uniform sampler2D _CameraDepthTexture;
		uniform float _EdgeFade;
		uniform float _CameraFadeDistance;
		uniform float _Amplitude;
		uniform float _AnimSpeed;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult25 = (float4(ase_worldPos.x , 0.0 , ase_worldPos.z , 0.0));
			o.texcoord_0.xy = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
			float mulTime24 = _Time.y * _AnimSpeed;
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );;
			v.vertex.xyz += ( v.color * ( sin( appendResult25 ) * ( _Amplitude * ( ( ( 1.0 - o.texcoord_0.y ) * cos( ( appendResult25 + mulTime24 ) ) ) * float4( ase_objectScale , 0.0 ) ) ) ) ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			float4 tex2DNode1 = tex2D( _BaseColor, uv_BaseColor );
			o.Albedo = ( _Color * tex2DNode1 ).rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth7 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth7 = abs( ( screenDepth7 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeFade ) );
			float clampResult16 = clamp( distanceDepth7 , 0.0 , 1.0 );
			float cameraDepthFade18 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / _CameraFadeDistance);
			float clampResult20 = clamp( cameraDepthFade18 , 0.0 , 1.0 );
			o.Alpha = ( ( ( _Color.a * tex2DNode1.a ) * clampResult16 ) * clampResult20 );
		}

		ENDCG
	}
	FallBack "Legacy Shaders/Transparent/Diffuse"
}