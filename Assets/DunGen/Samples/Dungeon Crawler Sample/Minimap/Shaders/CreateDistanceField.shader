Shader "Hidden/DunGen/Dungeon Crawler Sample/Create Distance Field"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_TextureSize ("Texture Size", Float) = 512
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
			float _TextureSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float MaxRadius = 8;
				float2 UV = i.uv;

				float curdist = 0;
				float mindist = MaxRadius;
				float texelsize = 1 / _TextureSize;

				UV = (floor(UV * _TextureSize) / _TextureSize) + (0.5 / _TextureSize);

				float startsample = tex2D(_MainTex, UV).r > 0 ? 1 : 0;
				int startinside = startsample;

				for (int i = -MaxRadius; i < MaxRadius; i++)
				{
					for (int j = -MaxRadius; j < MaxRadius; j++)
					{
						float2 curoffset = float2((float)i, float(j));
						float2 curUV = UV + curoffset * texelsize;
						curUV = (floor(curUV * _TextureSize) / _TextureSize) + (0.5 / _TextureSize);

						float cursample = tex2D(_MainTex, curUV).r > 0 ? 1 : 0;

						if (cursample != startsample)
						{
							curdist = length(curoffset);
							mindist = min(mindist, curdist);
						}
					}
				}

				float output = (mindist - 0.5) / (MaxRadius - 0.5);

				output *= startinside == 0.0 ? -1 : 1;
				output = (output + 1) * 0.5;

				return float4(output.rrr, 1);
            }
            ENDCG
        }
    }
}
