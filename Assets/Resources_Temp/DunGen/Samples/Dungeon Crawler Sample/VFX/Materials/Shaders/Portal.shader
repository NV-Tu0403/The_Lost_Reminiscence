Shader "DunGen/Dungeon Crawler Sample/Portal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_GlowSize ("Glow Size", Float) = 0.05
		_GlowColour ("Glow Colour", Color) = (1.0, 1.0, 1.0)
		_GlowIntensity ("Glow Intensity", Float) = 1.0
		_NoiseIntensityAndSpeed ("Noise Intensity & Speed", Vector) = (0.1, 0.1, 1.0, 1.0)
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			sampler2D _NoiseTex;
            float4 _MainTex_ST;
			float _GlowSize;
			float3 _GlowColour;
			float _GlowIntensity;
			float4 _NoiseIntensityAndSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				float t = _Time.y;
				float2 noiseUV = i.uv + _NoiseIntensityAndSpeed.zw * t;

				float noiseX = tex2D(_NoiseTex, noiseUV).g * 2.0 - 1.0;
				float noiseY = tex2D(_NoiseTex, noiseUV * 1.3).g * 2.0 - 1.0;

				float2 uv = i.uv + float2(noiseX, noiseY) * _NoiseIntensityAndSpeed.xy;
                fixed4 col = tex2D(_MainTex, uv);

				float size = 0.5 - _GlowSize;
				float distanceToCenter = distance(uv, 0.5);
				col.a = smoothstep(size + _GlowSize, size, distanceToCenter);

				float glowAmount = smoothstep(size, size + _GlowSize, distanceToCenter);
				glowAmount *= smoothstep(size + _GlowSize, size, distanceToCenter);

				float3 glow = glowAmount * _GlowColour * _GlowIntensity;
				col.rgb += glow;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
