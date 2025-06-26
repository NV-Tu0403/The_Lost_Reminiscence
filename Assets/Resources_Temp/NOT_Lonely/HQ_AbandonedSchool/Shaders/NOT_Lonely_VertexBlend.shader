// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_VertexBlend"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_BaseColor("Base Color", 2D) = "white" {}
		_ColorR("Color R", Color) = (1,1,1,1)
		_Normal("Normal", 2D) = "bump" {}
		_MetallicRAOGGlossA("Metallic (R) AO (G) Gloss (A)", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _BaseColor;
		uniform float4 _BaseColor_ST;
		uniform float4 _ColorR;
		uniform sampler2D _MetallicRAOGGlossA;
		uniform float4 _MetallicRAOGGlossA_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			float4 tex2DNode7 = tex2D( _BaseColor, uv_BaseColor );
			o.Albedo = lerp( tex2DNode7 , ( tex2DNode7 * _ColorR ) , i.vertexColor.r ).rgb;
			float2 uv_MetallicRAOGGlossA = i.uv_texcoord * _MetallicRAOGGlossA_ST.xy + _MetallicRAOGGlossA_ST.zw;
			float4 tex2DNode10 = tex2D( _MetallicRAOGGlossA, uv_MetallicRAOGGlossA );
			o.Metallic = tex2DNode10.r;
			o.Smoothness = tex2DNode10.a;
			o.Occlusion = tex2DNode10.g;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Standard"
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=10001
1950;52;1666;974;1246.998;621.8;1.3;True;True
Node;AmplifyShaderEditor.ColorNode;11;-624.2971,-387.1004;Float;False;Property;_ColorR;Color R;1;0;1,1,1,1;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;7;-688,-144;Float;True;Property;_BaseColor;Base Color;0;0;Assets/HQ_AbandonedSchool/Textures/PennantsA/PennantA_bc.tga;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.VertexColorNode;9;-177.7989,-460.2008;Float;False;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-214.7981,-182.2006;Float;False;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;13;71.00183,-94.50041;Float;False;3;0;FLOAT4;0,0,0,0;False;1;COLOR;0.0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;10;-674.7983,281.9994;Float;True;Property;_MetallicRAOGGlossA;Metallic (R) AO (G) Gloss (A);3;0;Assets/HQ_AbandonedSchool/Textures/PennantsA/PennantA_m_ao_g.tga;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;8;-688,64;Float;True;Property;_Normal;Normal;2;0;Assets/HQ_AbandonedSchool/Textures/PennantsA/PennantA_n.tga;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;280,78;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;NOT_Lonely/NOT_Lonely_VertexBlend;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;Standard;-1;-1;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;7;0
WireConnection;12;1;11;0
WireConnection;13;0;7;0
WireConnection;13;1;12;0
WireConnection;13;2;9;1
WireConnection;0;0;13;0
WireConnection;0;1;8;0
WireConnection;0;3;10;1
WireConnection;0;4;10;4
WireConnection;0;5;10;2
ASEEND*/
//CHKSM=E93F23A3F3B2DDA07DBBA27D24632B0F0C6DB601