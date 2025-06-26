// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:Skybox/Procedural,iptp:2,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:True,qofs:0,qpre:0,rntp:1,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:3554,x:32480,y:32959,varname:node_3554,prsc:2|emission-7568-OUT;n:type:ShaderForge.SFN_Color,id:8306,x:31755,y:32224,ptovrint:False,ptlb:Sky Color,ptin:_SkyColor,varname:node_8306,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.03137255,c2:0.09019608,c3:0.1764706,c4:1;n:type:ShaderForge.SFN_ViewVector,id:2265,x:31161,y:32872,varname:node_2265,prsc:2;n:type:ShaderForge.SFN_Dot,id:7606,x:31418,y:32953,varname:node_7606,prsc:2,dt:1|A-2265-OUT,B-3211-OUT;n:type:ShaderForge.SFN_Vector3,id:3211,x:31161,y:32997,varname:node_3211,prsc:2,v1:0,v2:-1,v3:0;n:type:ShaderForge.SFN_Color,id:3839,x:31772,y:32848,ptovrint:False,ptlb:Horizon Color,ptin:_HorizonColor,varname:_GroundColor_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.06617647,c2:0.5468207,c3:1,c4:1;n:type:ShaderForge.SFN_Power,id:4050,x:31772,y:32995,varname:node_4050,prsc:2|VAL-6125-OUT,EXP-7609-OUT;n:type:ShaderForge.SFN_Vector1,id:7609,x:31587,y:33095,varname:node_7609,prsc:2,v1:8;n:type:ShaderForge.SFN_OneMinus,id:6125,x:31587,y:32953,varname:node_6125,prsc:2|IN-7606-OUT;n:type:ShaderForge.SFN_Lerp,id:2737,x:31999,y:32869,cmnt:Sky,varname:node_2737,prsc:2|A-7257-OUT,B-3839-RGB,T-4050-OUT;n:type:ShaderForge.SFN_LightVector,id:3559,x:29834,y:33890,cmnt:Auto-adapts to your directional light,varname:node_3559,prsc:2;n:type:ShaderForge.SFN_Add,id:7568,x:32262,y:33059,cmnt:Sky plus Sun,varname:node_7568,prsc:2|A-2737-OUT,B-5855-OUT;n:type:ShaderForge.SFN_Slider,id:2435,x:29945,y:34205,ptovrint:False,ptlb:Sun Radius B,ptin:_SunRadiusB,varname:node_2435,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:5;n:type:ShaderForge.SFN_Slider,id:3144,x:29748,y:33410,ptovrint:False,ptlb:Sun Size,ptin:_SunSize,varname:_SunOuterRadius_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:5;n:type:ShaderForge.SFN_Clamp01,id:7022,x:31370,y:33343,varname:node_7022,prsc:2|IN-2915-OUT;n:type:ShaderForge.SFN_Power,id:754,x:31772,y:33336,varname:node_754,prsc:2|VAL-7022-OUT,EXP-5929-OUT;n:type:ShaderForge.SFN_Vector1,id:5929,x:31556,y:33412,varname:node_5929,prsc:2,v1:5;n:type:ShaderForge.SFN_Multiply,id:5855,x:32004,y:33243,cmnt:Sun,varname:node_5855,prsc:2|A-2359-RGB,B-754-OUT,C-7055-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7055,x:31772,y:33505,ptovrint:False,ptlb:Sun Intensity,ptin:_SunIntensity,varname:node_7055,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_LightColor,id:2359,x:31139,y:33243,cmnt:Get color from directional light,varname:node_2359,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:2881,x:31012,y:32435,ptovrint:False,ptlb:Stars Texture,ptin:_StarsTexture,varname:node_2881,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:16678a465b57ace40a6a6ed243ec9366,ntxv:2,isnm:False|UVIN-6232-OUT;n:type:ShaderForge.SFN_Add,id:7257,x:32039,y:32562,varname:node_7257,prsc:2|A-8306-RGB,B-9374-OUT;n:type:ShaderForge.SFN_Slider,id:4866,x:30937,y:32776,ptovrint:False,ptlb:Stars Power,ptin:_StarsPower,varname:node_4866,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.001,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:9744,x:31242,y:32559,varname:node_9744,prsc:2|A-2881-G,B-5345-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:5039,x:28931,y:32929,varname:node_5039,prsc:2;n:type:ShaderForge.SFN_Normalize,id:4787,x:29127,y:32929,varname:node_4787,prsc:2|IN-5039-XYZ;n:type:ShaderForge.SFN_ComponentMask,id:3474,x:29616,y:32801,varname:node_3474,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-4787-OUT;n:type:ShaderForge.SFN_ComponentMask,id:2659,x:29725,y:32537,varname:node_2659,prsc:2,cc1:0,cc2:2,cc3:-1,cc4:-1|IN-4787-OUT;n:type:ShaderForge.SFN_Clamp01,id:8565,x:29822,y:32836,varname:node_8565,prsc:2|IN-3474-OUT;n:type:ShaderForge.SFN_OneMinus,id:3526,x:29986,y:32836,varname:node_3526,prsc:2|IN-8565-OUT;n:type:ShaderForge.SFN_Power,id:5345,x:30576,y:32837,varname:node_5345,prsc:2|VAL-8565-OUT,EXP-8924-OUT;n:type:ShaderForge.SFN_Multiply,id:9439,x:30178,y:32643,varname:node_9439,prsc:2|A-2659-OUT,B-3526-OUT;n:type:ShaderForge.SFN_Add,id:6232,x:30355,y:32617,varname:node_6232,prsc:2|A-2659-OUT,B-9439-OUT;n:type:ShaderForge.SFN_Slider,id:8924,x:30180,y:32928,ptovrint:False,ptlb:Stars Position,ptin:_StarsPosition,varname:node_8924,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.001,cur:2,max:10;n:type:ShaderForge.SFN_Multiply,id:9937,x:31404,y:32690,varname:node_9937,prsc:2|A-9744-OUT,B-4866-OUT;n:type:ShaderForge.SFN_Tex2d,id:6627,x:30796,y:31831,varname:node_6627,prsc:2,ntxv:2,isnm:False|UVIN-3970-UVOUT,TEX-1617-TEX;n:type:ShaderForge.SFN_Multiply,id:4871,x:31504,y:32051,varname:node_4871,prsc:2|A-8785-OUT,B-6181-OUT;n:type:ShaderForge.SFN_Power,id:6181,x:30822,y:32641,varname:node_6181,prsc:2|VAL-3526-OUT,EXP-5568-OUT;n:type:ShaderForge.SFN_Slider,id:5568,x:30344,y:33053,ptovrint:False,ptlb:Clouds Position,ptin:_CloudsPosition,varname:node_5568,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.001,cur:2,max:10;n:type:ShaderForge.SFN_Add,id:9374,x:31714,y:32633,varname:node_9374,prsc:2|A-9937-OUT,B-2138-OUT;n:type:ShaderForge.SFN_Multiply,id:2138,x:31657,y:32388,varname:node_2138,prsc:2|A-5951-OUT,B-5771-OUT;n:type:ShaderForge.SFN_Slider,id:5771,x:31264,y:32415,ptovrint:False,ptlb:Clouds Power,ptin:_CloudsPower,varname:node_5771,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_Multiply,id:5951,x:31522,y:32261,varname:node_5951,prsc:2|A-4871-OUT,B-8565-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:1617,x:30437,y:31935,ptovrint:False,ptlb:Clouds Texture,ptin:_CloudsTexture,varname:node_1617,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8785,x:31280,y:31932,varname:node_8785,prsc:2|A-6627-RGB,B-3379-OUT;n:type:ShaderForge.SFN_Tex2d,id:5148,x:30936,y:32202,varname:node_5148,prsc:2,ntxv:0,isnm:False|UVIN-306-OUT,TEX-1617-TEX;n:type:ShaderForge.SFN_Divide,id:306,x:30765,y:32349,varname:node_306,prsc:2|A-1470-UVOUT,B-4162-OUT;n:type:ShaderForge.SFN_Vector1,id:4162,x:30359,y:32325,varname:node_4162,prsc:2,v1:3;n:type:ShaderForge.SFN_Time,id:2759,x:29928,y:32278,varname:node_2759,prsc:2;n:type:ShaderForge.SFN_Panner,id:1470,x:30463,y:32409,varname:node_1470,prsc:2,spu:0,spv:-1|UVIN-6232-OUT,DIST-4279-OUT;n:type:ShaderForge.SFN_Slider,id:861,x:29824,y:32453,ptovrint:False,ptlb:Clouds Speed,ptin:_CloudsSpeed,varname:node_861,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:0.5,max:1;n:type:ShaderForge.SFN_Multiply,id:4279,x:30215,y:32392,varname:node_4279,prsc:2|A-2759-TSL,B-861-OUT;n:type:ShaderForge.SFN_Panner,id:3970,x:30668,y:32150,varname:node_3970,prsc:2,spu:0,spv:-1|UVIN-6232-OUT,DIST-5481-OUT;n:type:ShaderForge.SFN_Divide,id:5481,x:30372,y:32140,varname:node_5481,prsc:2|A-4279-OUT,B-9434-OUT;n:type:ShaderForge.SFN_Vector1,id:9434,x:30177,y:32212,varname:node_9434,prsc:2,v1:4;n:type:ShaderForge.SFN_Color,id:811,x:30996,y:32003,ptovrint:False,ptlb:Clouds Color,ptin:_CloudsColor,varname:node_811,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:3379,x:31213,y:32139,varname:node_3379,prsc:2|A-5148-R,B-811-RGB;n:type:ShaderForge.SFN_Tex2d,id:8113,x:31360,y:34031,ptovrint:False,ptlb:Moon Texture,ptin:_MoonTexture,varname:node_8113,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:110874655ca7baf418fcdddd0a75dae0,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Dot,id:7946,x:30082,y:33822,varname:node_7946,prsc:2,dt:0|A-4787-OUT,B-3559-OUT;n:type:ShaderForge.SFN_Clamp01,id:3089,x:30259,y:33822,varname:node_3089,prsc:2|IN-7946-OUT;n:type:ShaderForge.SFN_Power,id:606,x:30436,y:33822,varname:node_606,prsc:2|VAL-3089-OUT,EXP-3858-OUT;n:type:ShaderForge.SFN_Exp,id:644,x:30090,y:33464,varname:node_644,prsc:2,et:0|IN-3144-OUT;n:type:ShaderForge.SFN_Multiply,id:8026,x:30606,y:33822,varname:node_8026,prsc:2|A-606-OUT,B-2545-OUT;n:type:ShaderForge.SFN_Exp,id:2545,x:30436,y:34010,varname:node_2545,prsc:2,et:0|IN-2435-OUT;n:type:ShaderForge.SFN_Clamp01,id:2915,x:30777,y:33846,varname:node_2915,prsc:2|IN-8026-OUT;n:type:ShaderForge.SFN_Add,id:4813,x:30758,y:33671,varname:node_4813,prsc:2|A-728-OUT,B-7946-OUT;n:type:ShaderForge.SFN_Vector1,id:728,x:30558,y:33671,varname:node_728,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:2400,x:31075,y:33761,varname:node_2400,prsc:2|A-3476-OUT,B-4813-OUT;n:type:ShaderForge.SFN_Vector1,id:3476,x:30903,y:33671,varname:node_3476,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Power,id:1622,x:31257,y:33761,varname:node_1622,prsc:2|VAL-2400-OUT,EXP-6130-OUT;n:type:ShaderForge.SFN_Slider,id:6130,x:30980,y:33966,ptovrint:False,ptlb:MoonHorizon,ptin:_MoonHorizon,varname:node_6130,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:5;n:type:ShaderForge.SFN_Add,id:410,x:31467,y:33761,varname:node_410,prsc:2|A-1622-OUT,B-2400-OUT;n:type:ShaderForge.SFN_RemapRange,id:3858,x:30259,y:33597,varname:node_3858,prsc:2,frmn:1,frmx:5,tomn:5000,tomx:100|IN-3144-OUT;n:type:ShaderForge.SFN_Abs,id:4051,x:29881,y:33085,varname:node_4051,prsc:2|IN-3474-OUT;n:type:ShaderForge.SFN_OneMinus,id:5846,x:30067,y:33113,varname:node_5846,prsc:2|IN-4051-OUT;n:type:ShaderForge.SFN_Power,id:1377,x:30295,y:33158,varname:node_1377,prsc:2|VAL-5846-OUT,EXP-6040-OUT;n:type:ShaderForge.SFN_Slider,id:6040,x:29947,y:33270,ptovrint:False,ptlb:Horizon Sharpness,ptin:_HorizonSharpness,varname:node_6040,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:2920,x:31556,y:33545,varname:node_2920,prsc:2|A-1377-OUT,B-410-OUT;n:type:ShaderForge.SFN_OneMinus,id:2019,x:30692,y:33168,varname:node_2019,prsc:2|IN-1377-OUT;n:type:ShaderForge.SFN_Multiply,id:1550,x:30906,y:33168,varname:node_1550,prsc:2|A-2019-OUT,B-1057-OUT;n:type:ShaderForge.SFN_Power,id:1774,x:31469,y:33167,varname:node_1774,prsc:2|VAL-2359-RGB,EXP-1550-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1057,x:30812,y:33395,ptovrint:False,ptlb:Horizon Light,ptin:_HorizonLight,varname:node_1057,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:8306-3839-2435-3144-7055-2881-1617-811-4866-8924-5568-5771-861-8113-6040-1057;pass:END;sub:END;*/

Shader "NOT_Lonely/NOT_Lonely_Sky" {
    Properties {
        _SkyColor ("Sky Color", Color) = (0.03137255,0.09019608,0.1764706,1)
        _HorizonColor ("Horizon Color", Color) = (0.06617647,0.5468207,1,1)
        _SunRadiusB ("Sun Radius B", Range(0, 5)) = 0.1
        _SunSize ("Sun Size", Range(1, 5)) = 1
        _SunIntensity ("Sun Intensity", Float ) = 2
        _StarsTexture ("Stars Texture", 2D) = "black" {}
        _CloudsTexture ("Clouds Texture", 2D) = "black" {}
        _CloudsColor ("Clouds Color", Color) = (1,1,1,1)
        _StarsPower ("Stars Power", Range(0.001, 1)) = 1
        _StarsPosition ("Stars Position", Range(0.001, 10)) = 2
        _CloudsPosition ("Clouds Position", Range(0.001, 10)) = 2
        _CloudsPower ("Clouds Power", Range(0, 1)) = 0.5
        _CloudsSpeed ("Clouds Speed", Range(-1, 1)) = 0.5
        _MoonTexture ("Moon Texture", 2D) = "white" {}
        _HorizonSharpness ("Horizon Sharpness", Range(0, 1)) = 0
        _HorizonLight ("Horizon Light", Float ) = 1
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Background"
            "RenderType"="Opaque"
            "PreviewType"="Skybox"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _SkyColor;
            uniform float4 _HorizonColor;
            uniform float _SunRadiusB;
            uniform float _SunSize;
            uniform float _SunIntensity;
            uniform sampler2D _StarsTexture; uniform float4 _StarsTexture_ST;
            uniform float _StarsPower;
            uniform float _StarsPosition;
            uniform float _CloudsPosition;
            uniform float _CloudsPower;
            uniform sampler2D _CloudsTexture; uniform float4 _CloudsTexture_ST;
            uniform float _CloudsSpeed;
            uniform float4 _CloudsColor;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
////// Emissive:
                float3 node_4787 = normalize(i.posWorld.rgb);
                float2 node_2659 = node_4787.rb;
                float node_3474 = node_4787.g;
                float node_8565 = saturate(node_3474);
                float node_3526 = (1.0 - node_8565);
                float2 node_6232 = (node_2659+(node_2659*node_3526));
                float4 _StarsTexture_var = tex2D(_StarsTexture,TRANSFORM_TEX(node_6232, _StarsTexture));
                float4 node_2759 = _Time;
                float node_4279 = (node_2759.r*_CloudsSpeed);
                float2 node_3970 = (node_6232+(node_4279/4.0)*float2(0,-1));
                float4 node_6627 = tex2D(_CloudsTexture,TRANSFORM_TEX(node_3970, _CloudsTexture));
                float2 node_306 = ((node_6232+node_4279*float2(0,-1))/3.0);
                float4 node_5148 = tex2D(_CloudsTexture,TRANSFORM_TEX(node_306, _CloudsTexture));
                float node_7946 = dot(node_4787,lightDirection);
                float3 emissive = (lerp((_SkyColor.rgb+(((_StarsTexture_var.g*pow(node_8565,_StarsPosition))*_StarsPower)+((((node_6627.rgb*(node_5148.r*_CloudsColor.rgb))*pow(node_3526,_CloudsPosition))*node_8565)*_CloudsPower))),_HorizonColor.rgb,pow((1.0 - max(0,dot(viewDirection,float3(0,-1,0)))),8.0))+(_LightColor0.rgb*pow(saturate(saturate((pow(saturate(node_7946),(_SunSize*-1225.0+6225.0))*exp(_SunRadiusB)))),5.0)*_SunIntensity));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    //CustomEditor "ShaderForgeMaterialInspector"
}
