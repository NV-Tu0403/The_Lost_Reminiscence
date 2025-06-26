// Upgrade NOTE: upgraded instancing buffer 'NOT_LonelyNOT_Lonely_LightRays' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_LightRays"
{
	Properties
	{
		_Mask("Mask", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_NoiseContrast("Noise Contrast", Range( 0 , 10)) = 2
		_AnimSpeed("Anim Speed", Float) = 1
		_DepthFade("Depth Fade", Range( 0 , 20)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 2.0
		#pragma multi_compile_instancing
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float eyeDepth;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform sampler2D _Noise;
		uniform float _AnimSpeed;
		uniform float4 _Noise_ST;
		uniform float _NoiseContrast;
		uniform sampler2D _CameraDepthTexture;
		uniform float _DepthFade;

		UNITY_INSTANCING_BUFFER_START(NOT_LonelyNOT_Lonely_LightRays)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
#define _Color_arr NOT_LonelyNOT_Lonely_LightRays
		UNITY_INSTANCING_BUFFER_END(NOT_LonelyNOT_Lonely_LightRays)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float mulTime56 = _Time.y * -0.02;
			float2 appendResult26 = (float2(_AnimSpeed , 0.0));
			float2 uv_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 panner24 = ( uv_Noise + mulTime56 * appendResult26);
			float temp_output_6_0 = ( tex2D( _Mask, uv_Mask ).r * pow( tex2D( _Noise, panner24 ).r , _NoiseContrast ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth34 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth34 = abs( ( screenDepth34 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) );
			float clampResult42 = clamp( distanceDepth34 , 0.0 , 1.0 );
			float cameraDepthFade43 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / 1.0);
			float clampResult45 = clamp( cameraDepthFade43 , 0.0 , 1.0 );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNDotV47 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode47 = ( 0.0 + 1.5 * pow( 1.0 - fresnelNDotV47, 5.0 ) );
			float clampResult49 = clamp( fresnelNode47 , 0.0 , 1.0 );
			float temp_output_48_0 = ( 1.0 - clampResult49 );
			o.Emission = ( _Color_Instance * temp_output_6_0 * _Color_Instance.a * clampResult42 * clampResult45 * temp_output_48_0 ).rgb;
			o.Alpha = ( temp_output_6_0 * _Color_Instance.a * clampResult42 * clampResult45 * temp_output_48_0 );
		}

		ENDCG
	}
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13801
1927;29;1666;974;1039.204;563.4127;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;27;-1234.108,684.7516;Float;False;Constant;_Float0;Float 0;4;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;25;-1243.579,592.6126;Float;False;Property;_AnimSpeed;Anim Speed;5;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.TexturePropertyNode;8;-1451.359,190.2869;Float;True;Property;_Noise;Noise;2;0;None;False;white;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-1167.27,328.5937;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;1,1;False;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleTimeNode;56;-1053.361,764.588;Float;False;1;0;FLOAT;-0.02;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;26;-1023.108,619.7516;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.PannerNode;24;-829.4327,502.1707;Float;False;3;0;FLOAT2;1,1;False;2;FLOAT2;0,0;False;1;FLOAT;-0.1;False;1;FLOAT2
Node;AmplifyShaderEditor.FresnelNode;47;-202.9587,570.5718;Float;False;Tangent;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.5;False;3;FLOAT;5.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;94;-499.5009,361.413;Float;False;Property;_NoiseContrast;Noise Contrast;4;0;2;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;7;-501,146;Float;True;Property;_Noise01;Noise01;0;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;92;-406.6682,464.9855;Float;False;Property;_DepthFade;Depth Fade;6;0;1;0;20;0;1;FLOAT
Node;AmplifyShaderEditor.CameraDepthFade;43;-40.3996,419.9783;Float;False;2;0;FLOAT;1.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.DepthFade;34;-45.07642,304.3715;Float;False;1;0;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;49;17.00624,570.7932;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;5;-506,-75;Float;True;Property;_Mask;Mask;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.PowerNode;93;-147.5009,167.413;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;45;197.6915,391.379;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;20;-422.7517,-269.4338;Float;False;InstancedProperty;_Color;Color;3;0;1,1,1,1;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;32.96663,37.6697;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;48;179.1516,566.7312;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;42;199.1005,259.4784;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;509.9264,192.3654;Float;False;5;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;510.2262,-19.5347;Float;False;6;6;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;2;FLOAT;0,0,0,0;False;3;FLOAT;0,0,0,0;False;4;FLOAT;0,0,0,0;False;5;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;4;931.4662,-25.4303;Float;False;True;0;Float;ASEMaterialInspector;0;0;Unlit;NOT_Lonely/NOT_Lonely_LightRays;False;False;False;False;True;True;True;True;True;False;False;False;False;False;True;True;True;Back;0;0;False;0;0;Custom;0.5;True;False;0;True;Transparent;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;False;2;SrcAlpha;OneMinusSrcAlpha;0;SrcAlpha;OneMinusSrcAlpha;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;14;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;2;8;0
WireConnection;26;0;25;0
WireConnection;26;1;27;0
WireConnection;24;0;29;0
WireConnection;24;2;26;0
WireConnection;24;1;56;0
WireConnection;7;0;8;0
WireConnection;7;1;24;0
WireConnection;34;0;92;0
WireConnection;49;0;47;0
WireConnection;93;0;7;1
WireConnection;93;1;94;0
WireConnection;45;0;43;0
WireConnection;6;0;5;1
WireConnection;6;1;93;0
WireConnection;48;0;49;0
WireConnection;42;0;34;0
WireConnection;23;0;6;0
WireConnection;23;1;20;4
WireConnection;23;2;42;0
WireConnection;23;3;45;0
WireConnection;23;4;48;0
WireConnection;22;0;20;0
WireConnection;22;1;6;0
WireConnection;22;2;20;4
WireConnection;22;3;42;0
WireConnection;22;4;45;0
WireConnection;22;5;48;0
WireConnection;4;2;22;0
WireConnection;4;9;23;0
ASEEND*/
//CHKSM=24314614FB31356FBAF19AAC75573FAEAEC440A2