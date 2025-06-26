// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_Tesselation"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_MetallicRAOGGlossA("Metallic(R) AO(G) Gloss (A)", 2D) = "white" {}
		_DisplacementTex("DisplacementTex", 2D) = "white" {}
		_Displacement("Displacement", Range( 0 , 1)) = 0
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 10
		_TessMaxDisp( "Max Displacement", Float ) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float2 uv_texcoord;
		};

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			fixed4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform sampler2D _MetallicRAOGGlossA;
		uniform float4 _MetallicRAOGGlossA_ST;
		uniform sampler2D _DisplacementTex;
		uniform float4 _DisplacementTex_ST;
		uniform float _Displacement;
		uniform float _EdgeLength;
		uniform float _TessMaxDisp;

		float4 tessFunction( appdata v0, appdata v1, appdata v2 )
		{
			return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, _EdgeLength , _TessMaxDisp );
		}

		void vertexDataFunc( inout appdata v )
		{
			float2 uv_DisplacementTex = v.texcoord * _DisplacementTex_ST.xy + _DisplacementTex_ST.zw;
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( tex2Dlod( _DisplacementTex, float4( uv_DisplacementTex, 0, 1.0) ) * float4( ase_vertexNormal , 0.0 ) ) * _Displacement ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = tex2D( _Albedo, uv_Albedo ).rgb;
			float2 uv_MetallicRAOGGlossA = i.uv_texcoord * _MetallicRAOGGlossA_ST.xy + _MetallicRAOGGlossA_ST.zw;
			float4 tex2DNode8 = tex2D( _MetallicRAOGGlossA, uv_MetallicRAOGGlossA );
			o.Metallic = tex2DNode8.r;
			o.Smoothness = tex2DNode8.a;
			o.Occlusion = tex2DNode8.g;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Standard"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13401
1927;29;1666;974;1370.701;558.2998;1.3;True;True
Node;AmplifyShaderEditor.SamplerNode;1;-775.9,188.1;Float;True;Property;_DisplacementTex;DisplacementTex;3;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.NormalVertexDataNode;3;-767.9999,428.2;Float;False;0;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;4;-522,483.5;Float;False;Property;_Displacement;Displacement;4;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-426,290.5;Float;False;2;2;0;COLOR;0.0,0,0,0;False;1;FLOAT3;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;2;-518.9001,-413.7001;Float;True;Property;_Albedo;Albedo;0;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;7;-517.9,-193.5999;Float;True;Property;_Normal;Normal;1;0;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;8;-513.9996,-8.399884;Float;True;Property;_MetallicRAOGGlossA;Metallic(R) AO(G) Gloss (A);2;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-221,310.5;Float;False;2;2;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;NOT_Lonely/NOT_Lonely_Tesselation;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;True;3;10;1;2;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;Standard;-1;-1;-1;5;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;1;0
WireConnection;5;1;3;0
WireConnection;6;0;5;0
WireConnection;6;1;4;0
WireConnection;0;0;2;0
WireConnection;0;1;7;0
WireConnection;0;3;8;1
WireConnection;0;4;8;4
WireConnection;0;5;8;2
WireConnection;0;11;6;0
ASEEND*/
//CHKSM=920CB18725FF2B1AE3451108D613AF6C5E7279D6