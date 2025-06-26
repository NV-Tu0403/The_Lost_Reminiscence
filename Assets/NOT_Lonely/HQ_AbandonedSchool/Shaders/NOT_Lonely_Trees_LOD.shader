// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_Trees_LOD"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.35
		_BaseColor("Base Color", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_MetallicRAOGGlossA("Metallic (R) AO (G) Gloss (A)", 2D) = "black" {}
		_Color01("Color01", Color) = (1,1,1,0)
		_Color02("Color02", Color) = (1,1,1,0)
		_MaskContrast("MaskContrast", Range( 0.001 , 10)) = 1
		_RandomTintIntensity("Random Tint Intensity", Range( 0 , 1)) = 0.5
		_RandomTintTiling("Random Tint Tiling", Range( 0.01 , 5)) = 0.2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 worldPos;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _BaseColor;
		uniform float4 _BaseColor_ST;
		uniform float4 _Color01;
		uniform float4 _Color02;
		uniform sampler2D _MetallicRAOGGlossA;
		uniform float4 _MetallicRAOGGlossA_ST;
		uniform float _MaskContrast;
		uniform float _RandomTintTiling;
		uniform float _RandomTintIntensity;
		uniform float _Cutoff = 0.35;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			float4 tex2DNode1 = tex2D( _BaseColor, uv_BaseColor );
			float4 temp_output_2_0 = ( tex2DNode1 * _Color01 );
			float2 uv_MetallicRAOGGlossA = i.uv_texcoord * _MetallicRAOGGlossA_ST.xy + _MetallicRAOGGlossA_ST.zw;
			float4 tex2DNode5 = tex2D( _MetallicRAOGGlossA, uv_MetallicRAOGGlossA );
			float4 lerpResult30 = lerp( temp_output_2_0 , ( temp_output_2_0 * _Color02 ) , ( i.vertexColor.g * pow( ( 1.0 - tex2DNode5.g ) , _MaskContrast ) ));
			float3 ase_worldPos = i.worldPos;
			float4 clampResult61 = clamp( ( lerpResult30 * ( ( sin( ( ( ase_worldPos.x + ase_worldPos.y + ase_worldPos.z ) * _RandomTintTiling ) ) * 0.5 ) + ( 1.0 - _RandomTintIntensity ) ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Albedo = clampResult61.rgb;
			o.Metallic = tex2DNode5.r;
			o.Smoothness = tex2DNode5.a;
			o.Occlusion = tex2DNode5.g;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13801
1927;29;1666;974;1624.285;741.5463;1.9;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;39;-867.4012,617.9558;Float;False;0;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;50;-812.5088,807.9552;Float;False;Property;_RandomTintTiling;Random Tint Tiling;8;0;0.2;0.01;5;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;5;-620.3995,378.8064;Float;True;Property;_MetallicRAOGGlossA;Metallic (R) AO (G) Gloss (A);3;0;Assets/HQ_AbandonedSchool/Textures/Tree02/Tree02_m_ao_g.tga;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-646.4812,639.8977;Float;False;3;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-492.5088,680.9552;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-946,-239;Float;True;Property;_BaseColor;Base Color;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;36;-416.3867,196.1078;Float;False;Property;_MaskContrast;MaskContrast;6;0;1;0.001;10;0;1;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;38;-273.3867,299.1078;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;4;-877,-24;Float;False;Property;_Color01;Color01;4;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;44;-406.3873,826.9346;Float;False;Constant;_Float0;Float 0;9;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SinOpNode;42;-272.0756,663.2647;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;31;-638.8215,49.81;Float;False;Property;_Color02;Color02;5;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.VertexColorNode;26;-847.5922,275.8488;Float;False;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.PowerNode;34;-82.38672,171.1078;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;59;-151.4611,905.966;Float;False;Property;_RandomTintIntensity;Random Tint Intensity;7;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-458,-181;Float;False;2;2;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-299.9641,-77.28193;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;83.61328,66.10779;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-88.38708,642.9346;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;62;183.0064,720.1416;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;30;260.9715,-35.50258;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;58;400.3389,476.1657;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;572.2208,87.8606;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.ClampOpNode;61;756.2107,108.4576;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;6;-491,-464;Float;True;Property;_Normal;Normal;2;0;Assets/HQ_AbandonedSchool/Textures/BasketballBackboard/BasketballBackboard_n.tga;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;953.2033,59.39899;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;NOT_Lonely/NOT_Lonely_Trees_LOD;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;Back;0;0;False;0;0;Masked;0.35;True;True;0;False;TransparentCutout;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;40;0;39;1
WireConnection;40;1;39;2
WireConnection;40;2;39;3
WireConnection;49;0;40;0
WireConnection;49;1;50;0
WireConnection;38;0;5;2
WireConnection;42;0;49;0
WireConnection;34;0;38;0
WireConnection;34;1;36;0
WireConnection;2;0;1;0
WireConnection;2;1;4;0
WireConnection;32;0;2;0
WireConnection;32;1;31;0
WireConnection;37;0;26;2
WireConnection;37;1;34;0
WireConnection;43;0;42;0
WireConnection;43;1;44;0
WireConnection;62;0;59;0
WireConnection;30;0;2;0
WireConnection;30;1;32;0
WireConnection;30;2;37;0
WireConnection;58;0;43;0
WireConnection;58;1;62;0
WireConnection;47;0;30;0
WireConnection;47;1;58;0
WireConnection;61;0;47;0
WireConnection;0;0;61;0
WireConnection;0;1;6;0
WireConnection;0;3;5;1
WireConnection;0;4;5;4
WireConnection;0;5;5;2
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=A91EE8E529AAAE483BA0B2BA220BA3EEC7F43350