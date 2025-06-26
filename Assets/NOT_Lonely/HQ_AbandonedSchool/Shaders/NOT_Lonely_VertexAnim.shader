// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_VertexAnim"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.35
		_BaseColor("Base Color", 2D) = "white" {}
		_Spec("Spec", 2D) = "black" {}
		_Normal("Normal", 2D) = "bump" {}
		_Speed("Speed", Float) = 1
		_Amplitude("Amplitude", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _BaseColor;
		uniform float4 _BaseColor_ST;
		uniform sampler2D _Spec;
		uniform float4 _Spec_ST;
		uniform float _Amplitude;
		uniform float _Speed;
		uniform float _Cutoff = 0.35;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult28 = (float4(ase_worldPos.x , 0.0 , ase_worldPos.z , 0.0));
			float mulTime10 = _Time.y * _Speed;
			v.vertex.xyz += ( v.color.r * ( sin( appendResult28 ) * ( _Amplitude * cos( ( appendResult28 + mulTime10 ) ) ) ) ).xyz;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			float4 tex2DNode1 = tex2D( _BaseColor, uv_BaseColor );
			o.Albedo = ( tex2DNode1 * float4(1,1,1,0) ).rgb;
			float2 uv_Spec = i.uv_texcoord * _Spec_ST.xy + _Spec_ST.zw;
			float4 tex2DNode5 = tex2D( _Spec, uv_Spec );
			o.Specular = tex2DNode5.rgb;
			o.Smoothness = tex2DNode5.a;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13401
1927;29;1666;974;2150.194;154.404;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;13;-2342.11,353.5667;Float;False;Property;_Speed;Speed;4;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.WorldPosInputsNode;8;-2150.11,129.5668;Float;False;0;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleTimeNode;10;-2134.11,321.5667;Float;False;1;0;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;28;-1893.906,108.3671;Float;False;FLOAT4;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT4
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1718.11,305.5667;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.CosOpNode;19;-1494.11,481.5667;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;23;-1251.495,386.0001;Float;False;Property;_Amplitude;Amplitude;5;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1021.495,468.0001;Float;False;2;2;0;FLOAT;0,0,0;False;1;FLOAT4;0;False;1;FLOAT4
Node;AmplifyShaderEditor.SinOpNode;15;-1382.11,225.5667;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.VertexColorNode;26;-759.2943,255.0003;Float;False;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-809,-180;Float;True;Property;_BaseColor;Base Color;1;0;Assets/HQ_AbandonedSchool/Textures/BasketballBackboard/BasketballBackboard_d.tga;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-758.6965,474.4001;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.ColorNode;4;-731,37;Float;False;Constant;_Color0;Color 0;2;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;5;-473,181;Float;True;Property;_Spec;Spec;2;0;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-378,-179;Float;False;2;2;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;6;-491,-464;Float;True;Property;_Normal;Normal;3;0;Assets/HQ_AbandonedSchool/Textures/BasketballBackboard/BasketballBackboard_n.tga;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-510.0946,493.8;Float;False;2;2;0;FLOAT;0,0,0,0;False;1;FLOAT4;0.0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;NOT_Lonely/NOT_Lonely_VertexAnim;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Masked;0.35;True;True;0;False;TransparentCutout;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;13;0
WireConnection;28;0;8;1
WireConnection;28;2;8;3
WireConnection;11;0;28;0
WireConnection;11;1;10;0
WireConnection;19;0;11;0
WireConnection;24;0;23;0
WireConnection;24;1;19;0
WireConnection;15;0;28;0
WireConnection;25;0;15;0
WireConnection;25;1;24;0
WireConnection;2;0;1;0
WireConnection;2;1;4;0
WireConnection;27;0;26;1
WireConnection;27;1;25;0
WireConnection;0;0;2;0
WireConnection;0;1;6;0
WireConnection;0;3;5;0
WireConnection;0;4;5;4
WireConnection;0;10;1;4
WireConnection;0;11;27;0
ASEEND*/
//CHKSM=BD18B8454DC15194B34A2A59EDB7C0BE2308C191