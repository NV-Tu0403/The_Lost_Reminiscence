// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NOT_Lonely_WorldSpaceMapping"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Contrast("Contrast", Range( 1 , 10)) = 1
		_Tiling("Tiling", Float) = 0.25
		_OffsetX("Offset X", Range( 0 , 1)) = 0
		_OffsetY("Offset Y", Range( 0 , 1)) = 0.5
		_Angle("Angle", Range( 0 , 360)) = 0
		_Glossiness("Glossiness", Range( 0 , 1)) = 0
		_Normal("Normal", 2D) = "bump" {}
		_NormalIntensity("Normal Intensity", Float) = 1
		_OpacityContrast("Opacity Contrast", Range( 0.01 , 6)) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True"  }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard fullforwardshadows decal:blend 
		struct Input
		{
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		uniform float _NormalIntensity;
		uniform sampler2D _Normal;
		uniform float _Tiling;
		uniform float _OffsetX;
		uniform float _OffsetY;
		uniform float _Angle;
		uniform sampler2D _MainTex;
		uniform float _Contrast;
		uniform float4 _Color;
		uniform float _Glossiness;
		uniform float _OpacityContrast;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 appendResult38 = (float2(( ( ( 1.0 - _OffsetX ) - _OffsetY ) / 2.0 ) , ( ( ( 1.0 - _OffsetY ) + _OffsetX ) / 2.0 )));
			float cos18 = cos( ( ( _Angle / 180 ) * UNITY_PI ) );
			float sin18 = sin( ( ( _Angle / 180 ) * UNITY_PI ) );
			float2 rotator18 = mul( ( (ase_worldPos).xz * _Tiling ) - appendResult38 , float2x2( cos18 , -sin18 , sin18 , cos18 )) + appendResult38;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, rotator18 ) ,_NormalIntensity );
			float4 tex2DNode13 = tex2D( _MainTex, rotator18 );
			float temp_output_76_0 = pow( tex2DNode13.r , _Contrast );
			float clampResult78 = clamp( temp_output_76_0 , 0.0 , 1.0 );
			o.Albedo = ( clampResult78 * _Color ).rgb;
			o.Smoothness = ( temp_output_76_0 * _Glossiness );
			float smoothstepResult74 = smoothstep( 0.0 , ( 1.0 - tex2DNode13.a ) , i.vertexColor.r);
			float lerpResult69 = lerp( 0.0 , pow( ( tex2DNode13.a * _Color.a ) , _OpacityContrast ) , smoothstepResult74);
			o.Alpha = lerpResult69;
		}

		ENDCG
	}
	Fallback "Legacy Shaders/Transparent/Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13401
1927;29;1666;974;1247.877;392.9684;1.3;True;True
Node;AmplifyShaderEditor.RangedFloatNode;36;-2415.031,-267.1967;Float;False;Property;_OffsetY;Offset Y;6;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;39;-2412.031,-144.1967;Float;False;Property;_OffsetX;Offset X;5;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;43;-2069.525,-164.4967;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.IntNode;32;-1636.352,99.42838;Float;False;Constant;_Int0;Int 0;5;0;180;0;1;INT
Node;AmplifyShaderEditor.OneMinusNode;40;-2054.525,-308.4966;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;25;-1747.835,19.59564;Float;False;Property;_Angle;Angle;7;0;0;0;360;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-1894.529,-148.4967;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.WorldPosInputsNode;19;-1738.799,-548.6245;Float;False;0;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;47;-1701.878,-182.8826;Float;False;Constant;_Float0;Float 0;8;0;2;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleDivideOpNode;31;-1431.659,17.8725;Float;False;2;0;FLOAT;0.0;False;1;INT;0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;42;-1835.529,-254.4967;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-1550.178,-262.6826;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleDivideOpNode;48;-1539.963,-117.4025;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;23;-1509.799,-444.6243;Float;False;Property;_Tiling;Tiling;4;0;0.25;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.PiNode;28;-1266.434,18.99564;Float;False;1;0;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.ComponentMaskNode;20;-1514.799,-547.6245;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1262.798,-511.6244;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.WireNode;29;-1060.65,3.549774;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;38;-1395.629,-149.5967;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.RotatorNode;18;-1009.337,-159.6346;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;77;-875.1868,-441.2426;Float;False;Property;_Contrast;Contrast;3;0;1;1;10;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;13;-768.5696,-206.195;Float;True;Property;_MainTex;MainTex;2;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;2;-733.8424,45.50723;Float;False;Property;_Color;Color;1;0;1,1,1,1;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;75;-102.2316,619.6559;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.PowerNode;76;-466.1868,-259.2426;Float;True;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-249.8081,-28.06372;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;55;-369.8777,290.5016;Float;False;Property;_OpacityContrast;Opacity Contrast;11;0;1;0.01;6;0;1;FLOAT
Node;AmplifyShaderEditor.VertexColorNode;57;-115.3691,420.3633;Float;False;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;78;-233.1868,-253.2426;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;33;114.7625,180.0586;Float;False;Property;_Glossiness;Glossiness;8;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.PowerNode;54;-56.87769,208.5016;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SmoothstepOpNode;74;115.7634,491.7509;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;73;123.404,368.9614;Float;False;Constant;_Float1;Float 1;11;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;51;-1067.737,411.8396;Float;False;Property;_NormalIntensity;Normal Intensity;10;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;69;327.0964,314.3051;Float;False;3;0;FLOAT;0,0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;433.0237,142.6316;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-75.80811,-208.0637;Float;False;2;2;0;FLOAT;0,0,0,0;False;1;COLOR;0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;50;-812.6852,265.3042;Float;True;Property;_Normal;Normal;9;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;592,12;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;NOT_Lonely/NOT_Lonely_WorldSpaceMapping;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Back;0;0;False;0;0;Custom;0.5;True;False;0;True;Transparent;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;Legacy Shaders/Transparent/Diffuse;0;-1;-1;-1;0;1;=;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;43;0;39;0
WireConnection;40;0;36;0
WireConnection;41;0;40;0
WireConnection;41;1;39;0
WireConnection;31;0;25;0
WireConnection;31;1;32;0
WireConnection;42;0;43;0
WireConnection;42;1;36;0
WireConnection;46;0;42;0
WireConnection;46;1;47;0
WireConnection;48;0;41;0
WireConnection;48;1;47;0
WireConnection;28;0;31;0
WireConnection;20;0;19;0
WireConnection;21;0;20;0
WireConnection;21;1;23;0
WireConnection;29;0;28;0
WireConnection;38;0;46;0
WireConnection;38;1;48;0
WireConnection;18;0;21;0
WireConnection;18;1;38;0
WireConnection;18;2;29;0
WireConnection;13;1;18;0
WireConnection;75;0;13;4
WireConnection;76;0;13;1
WireConnection;76;1;77;0
WireConnection;5;0;13;4
WireConnection;5;1;2;4
WireConnection;78;0;76;0
WireConnection;54;0;5;0
WireConnection;54;1;55;0
WireConnection;74;0;57;0
WireConnection;74;2;75;0
WireConnection;69;0;73;0
WireConnection;69;1;54;0
WireConnection;69;2;74;0
WireConnection;79;0;76;0
WireConnection;79;1;33;0
WireConnection;6;0;78;0
WireConnection;6;1;2;0
WireConnection;50;1;18;0
WireConnection;50;5;51;0
WireConnection;0;0;6;0
WireConnection;0;1;50;0
WireConnection;0;4;79;0
WireConnection;0;9;69;0
ASEEND*/
//CHKSM=925B9A986B1AB0905BDAEFD520BBD1500858C5B4