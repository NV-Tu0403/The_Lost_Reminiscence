// Upgrade NOTE: upgraded instancing buffer 'NOT_LonelyNOT_Lonely_Grass' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_Grass"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Normals("Normals", 2D) = "bump" {}
		_NormalsIntensity("Normals Intensity", Float) = 1
		_TintMask("Tint Mask", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_MaskTiling("Mask Tiling", Range( 0.001 , 5)) = 1
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_Speed("Speed", Float) = 1
		_Amplitude("Amplitude", Float) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float _NormalsIntensity;
		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform sampler2D _TintMask;
		uniform float _MaskTiling;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Gloss;
		uniform float _Amplitude;
		uniform float _Speed;
		uniform float _Cutoff = 0.5;

		UNITY_INSTANCING_BUFFER_START(NOT_LonelyNOT_Lonely_Grass)
			UNITY_DEFINE_INSTANCED_PROP(float4, _TintColor)
#define _TintColor_arr NOT_LonelyNOT_Lonely_Grass
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
#define _Color_arr NOT_LonelyNOT_Lonely_Grass
		UNITY_INSTANCING_BUFFER_END(NOT_LonelyNOT_Lonely_Grass)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult16 = (float4(ase_worldPos.x , 0.0 , ase_worldPos.z , 0.0));
			float mulTime6 = _Time.y * _Speed;
			v.vertex.xyz += ( v.color.r * ( sin( appendResult16 ) * ( _Amplitude * cos( ( appendResult16 + mulTime6 ) ) ) ) ).xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _Normals, uv_Normals ) ,_NormalsIntensity );
			float4 _TintColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_TintColor_arr, _TintColor);
			float4 _Color_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			float3 ase_worldPos = i.worldPos;
			float4 lerpResult26 = lerp( _TintColor_Instance , _Color_Instance , tex2D( _TintMask, ( (ase_worldPos).xz * _MaskTiling ) ).r);
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex );
			o.Albedo = ( lerpResult26 * tex2DNode1 ).rgb;
			o.Smoothness = _Gloss;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Standard"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13801
1927;29;1666;974;837.0129;289.6235;1;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;15;-2501.852,-1.594587;Float;False;0;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;5;-2485.447,313.1224;Float;False;Property;_Speed;Speed;9;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleTimeNode;6;-2277.446,281.1224;Float;False;1;0;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;16;-2245.647,-22.79429;Float;False;FLOAT4;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT4
Node;AmplifyShaderEditor.WorldPosInputsNode;21;-1502.821,-306.2238;Float;False;0;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-1861.448,265.1224;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;20;-1375.79,-154.83;Float;False;Property;_MaskTiling;Mask Tiling;7;0;1;0.001;5;0;1;FLOAT
Node;AmplifyShaderEditor.ComponentMaskNode;23;-1278.821,-305.2238;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1026.819,-269.2239;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.CosOpNode;8;-1637.448,441.1225;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;9;-1394.833,345.5559;Float;False;Property;_Amplitude;Amplitude;10;0;0.1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-1164.832,427.5559;Float;False;2;2;0;FLOAT;0,0,0;False;1;FLOAT4;0;False;1;FLOAT4
Node;AmplifyShaderEditor.ColorNode;27;-760.0616,-606.5272;Float;False;InstancedProperty;_TintColor;Tint Color;6;0;1,1,1,1;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SinOpNode;11;-1525.448,185.1223;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.ColorNode;4;-762.9177,-413.0966;Float;False;InstancedProperty;_Color;Color;1;0;1,1,1,1;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;25;-807.3577,-227.2264;Float;True;Property;_TintMask;Tint Mask;5;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.VertexColorNode;12;-902.6318,214.556;Float;False;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-632,-6;Float;True;Property;_MainTex;MainTex;2;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;26;-415.2294,-174.7495;Float;False;3;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-902.0341,433.956;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;30;-500.0132,257.3765;Float;False;Property;_NormalsIntensity;Normals Intensity;4;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;28;-239.0131,94.3765;Float;False;Property;_Gloss;Gloss;8;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-653.4326,453.3557;Float;False;2;2;0;FLOAT;0,0,0,0;False;1;FLOAT4;0;False;1;FLOAT4
Node;AmplifyShaderEditor.SamplerNode;29;-250.0132,202.3765;Float;True;Property;_Normals;Normals;3;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-206,-72;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;153.6,12.6;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;NOT_Lonely/NOT_Lonely_Grass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;Off;0;0;False;0;0;Masked;0.5;True;True;0;False;TransparentCutout;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;Standard;0;0;-1;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;5;0
WireConnection;16;0;15;1
WireConnection;16;2;15;3
WireConnection;7;0;16;0
WireConnection;7;1;6;0
WireConnection;23;0;21;0
WireConnection;24;0;23;0
WireConnection;24;1;20;0
WireConnection;8;0;7;0
WireConnection;10;0;9;0
WireConnection;10;1;8;0
WireConnection;11;0;16;0
WireConnection;25;1;24;0
WireConnection;26;0;27;0
WireConnection;26;1;4;0
WireConnection;26;2;25;1
WireConnection;13;0;11;0
WireConnection;13;1;10;0
WireConnection;14;0;12;1
WireConnection;14;1;13;0
WireConnection;29;5;30;0
WireConnection;3;0;26;0
WireConnection;3;1;1;0
WireConnection;0;0;3;0
WireConnection;0;1;29;0
WireConnection;0;4;28;0
WireConnection;0;10;1;4
WireConnection;0;11;14;0
ASEEND*/
//CHKSM=F88810E680A46DEB1AAFC57EB3D83F9E2CF44BAA