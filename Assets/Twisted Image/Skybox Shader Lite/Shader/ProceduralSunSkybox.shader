Shader "Custom/ProceduralSunSkybox"
// Autor: Seta
{
    Properties
    {
        _SkyboxTex ("Skybox Texture", Cube) = "" {}
        _SkyboxColor ("Skybox Color", Color) = (1,1,1,1)
        _SkyboxSaturation ("Skybox Saturation", Range(0, 2)) = 1.0
        _SkyboxIntensity ("Skybox Intensity", Range(0, 3)) = 1.0
        _SkyboxRotation ("Skybox Rotation", Range(0, 360)) = 0
        _SunSize ("Sun Size", Range(0, 0.2)) = 0.05
        _SunIntensity ("Sun Intensity", Range(0, 100)) = 2.0

    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 texcoord : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            samplerCUBE _SkyboxTex;
            float4 _SkyboxColor;
            float _SkyboxIntensity;
            float _SkyboxRotation;
            float _SunSize;
            float _SunIntensity;
            float4 _SunDirection;
            float _SkyboxSaturation;

            float3 RotateAroundY(float3 direction, float angle)
            {
                float cosAngle = cos(angle * 0.0174533);
                float sinAngle = sin(angle * 0.0174533);
                return float3(
                    direction.x * cosAngle - direction.z * sinAngle,
                    direction.y,
                    direction.x * sinAngle + direction.z * cosAngle
                );
            }

            float3 AdjustSaturation(float3 color, float saturation)
            {
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(float3(luminance, luminance, luminance), color, saturation);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = normalize(v.vertex.xyz);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(i.texcoord);
                float3 rotatedViewDir = RotateAroundY(viewDir, _SkyboxRotation);
                float3 skyColor = texCUBE(_SkyboxTex, rotatedViewDir).rgb * _SkyboxIntensity * _SkyboxColor.rgb;
                skyColor = AdjustSaturation(skyColor, _SkyboxSaturation);

                float3 sunDirection = normalize(_SunDirection.xyz);
                float sunFactor = smoothstep(_SunSize / 10, 0.0, 1.0 - dot(viewDir, sunDirection));
                float3 finalSunColor = float3(1.0, 0.9, 0.7) * sunFactor * _SunIntensity;

                return float4(skyColor + finalSunColor, 1.0);
            }
            ENDCG
        }
    }
}
