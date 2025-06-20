Shader "DunGen/Dungeon Crawler Sample/Draw Minimap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_WallSize ("Wall Size", Float) = 0.03
		_FillColour ("Fill Colour", Color) = (0.5, 0.5, 0.5)
		_WallColour ("Wall Colour", Color) = (1.0, 1.0, 1.0)
		_ShadowColour ("Shadow Colour", Color) = (0.0, 0.0, 0.0)
		_EdgeFadeDistance ("Edge Fade Distance", Float) = 0.1
		_EdgeFadeColour ("Edge Fade Colour", Color) = (0.0, 0.0, 0.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _EdgeDistance;
			float _WallSize;
			float3 _FillColour;
			float3 _WallColour;
			float3 _ShadowColour;
			float _EdgeFadeDistance;
			float3 _EdgeFadeColour;
			

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distance = tex2D(_MainTex, i.uv).r * 2 - 1;
				float f = _WallSize;

				float outlineMask = step(distance, f) * step(-f, distance);
				float fillMask = step(0, distance);

				float3 col = fillMask * _FillColour;

				float shadowMask = step(0, distance) * smoothstep(1, 0, distance);
				col = lerp(col, _ShadowColour, shadowMask);
				col = lerp(col, _WallColour, outlineMask);

				float2 uv = i.uv;
				float edgeFadeMask = smoothstep(_EdgeFadeDistance, 0.0, uv.x);
				edgeFadeMask = max(edgeFadeMask, smoothstep(1 - _EdgeFadeDistance, 1.0, uv.x));
				edgeFadeMask = max(edgeFadeMask, smoothstep(_EdgeFadeDistance, 0.0, uv.y));
				edgeFadeMask = max(edgeFadeMask, smoothstep(1 - _EdgeFadeDistance, 1.0, uv.y));

				col = lerp(col, _EdgeFadeColour, edgeFadeMask);

				return half4(col.rgb, 1);
            }
            ENDCG
        }
    }
}
