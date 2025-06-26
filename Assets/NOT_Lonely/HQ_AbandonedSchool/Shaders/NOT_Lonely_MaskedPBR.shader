// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_MaskedPBR"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_BaseColor("BaseColor", 2D) = "white" {}
		_ColorMask("Color Mask", 2D) = "white" {}
		_Color01("Color01", Color) = (1,1,1,0)
		_Color02("Color02", Color) = (1,1,1,0)
		_Color03("Color03", Color) = (1,1,1,0)
		_Color04("Color04", Color) = (1,1,1,0)
		_NormalMap("Normal Map", 2D) = "white" {}
		_MetallicRAOGGlossA("Metallic (R) AO (G) Gloss (A)", 2D) = "black" {}
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
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _BaseColor;
		uniform float4 _BaseColor_ST;
		uniform float4 _Color04;
		uniform float4 _Color01;
		uniform sampler2D _ColorMask;
		uniform float4 _ColorMask_ST;
		uniform float4 _Color02;
		uniform float4 _Color03;
		uniform sampler2D _MetallicRAOGGlossA;
		uniform float4 _MetallicRAOGGlossA_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			float4 tex2DNode1 = tex2D( _BaseColor, uv_BaseColor );
			float2 uv_ColorMask = i.uv_texcoord * _ColorMask_ST.xy + _ColorMask_ST.zw;
			float4 tex2DNode12 = tex2D( _ColorMask, uv_ColorMask );
			o.Albedo = lerp( lerp( lerp( ( tex2DNode1 * _Color04 ) , ( tex2DNode1 * _Color01 ) , tex2DNode12.r ) , ( tex2DNode1 * _Color02 ) , tex2DNode12.g ) , ( tex2DNode1 * _Color03 ) , tex2DNode12.b ).rgb;
			float2 uv_MetallicRAOGGlossA = i.uv_texcoord * _MetallicRAOGGlossA_ST.xy + _MetallicRAOGGlossA_ST.zw;
			float4 tex2DNode15 = tex2D( _MetallicRAOGGlossA, uv_MetallicRAOGGlossA );
			o.Metallic = tex2DNode15.r;
			o.Smoothness = tex2DNode15.a;
			o.Occlusion = tex2DNode15.g;
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
7;29;1906;1124;1640.196;467.6998;1.1;True;True
Node;AmplifyShaderEditor.ColorNode;6;-1203.199,92.40023;Float;False;Property;_Color01;Color01;2;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;10;-1190.2,638.4007;Float;False;Property;_Color04;Color04;5;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-1256.801,-182.4999;Float;True;Property;_BaseColor;BaseColor;0;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-732.6006,265.4001;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;12;-885.696,-485.5997;Float;True;Property;_ColorMask;Color Mask;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-734.6006,-83.59991;Float;False;2;0;COLOR;0.0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.ColorNode;8;-1199.299,273.1006;Float;False;Property;_Color02;Color02;3;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-733.6006,32.40009;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;11;-430.5979,-76.79963;Float;False;3;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.ColorNode;9;-1191.499,457.7005;Float;False;Property;_Color03;Color03;4;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-732.6006,144.4001;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;13;-232.9965,52.40039;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;15;-225.3946,594.9004;Float;True;Property;_MetallicRAOGGlossA;Metallic (R) AO (G) Gloss (A);7;0;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;16;-230.3952,369.4009;Float;True;Property;_NormalMap;Normal Map;6;0;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;14;-32.7967,154.6003;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;357.3999,175;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;NOT_Lonely/NOT_Lonely_MaskedPBR;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;Standard;-1;-1;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;1;0
WireConnection;5;1;10;0
WireConnection;2;0;1;0
WireConnection;2;1;6;0
WireConnection;3;0;1;0
WireConnection;3;1;8;0
WireConnection;11;0;5;0
WireConnection;11;1;2;0
WireConnection;11;2;12;1
WireConnection;4;0;1;0
WireConnection;4;1;9;0
WireConnection;13;0;11;0
WireConnection;13;1;3;0
WireConnection;13;2;12;2
WireConnection;14;0;13;0
WireConnection;14;1;4;0
WireConnection;14;2;12;3
WireConnection;0;0;14;0
WireConnection;0;1;16;0
WireConnection;0;3;15;1
WireConnection;0;4;15;4
WireConnection;0;5;15;2
ASEEND*/
//CHKSM=0FDF75F673AD55ECB2A80354E1219F0DB5CD1194