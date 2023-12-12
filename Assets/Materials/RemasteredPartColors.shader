// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/RemasteredPartColors"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			float4 _MainTex_TexelSize;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord);
				c.a *= IN.color.a;
				c.rgb *= c.a;
				fixed thresh = 0.125 * c.a;
				if (c.r > thresh && c.g > thresh && c.b > thresh)
				{
					c *= IN.color;
				}
				fixed4 d = SampleSpriteTexture (IN.texcoord);
				d.a *= IN.color.a;
				d.rgb *= c.a;
				d *= IN.color;


                half4 outlineD = _Color;
                outlineD.a *= ceil(c.a);
                outlineD.rgb *= outlineD.a;
 				
				fixed4 texelSize = _MainTex_TexelSize;
                float u = 1.0f;

                for (fixed l=-texelSize.x* 2.0f; l <= texelSize.x* 3.0f; l += texelSize.x)
                {
                    for (fixed m=-texelSize.y* 2.0f; m <= texelSize.y* 3.0f; m += texelSize.y)
                    {
                        fixed o = tex2D(_MainTex, IN.texcoord + fixed2(l, m)).a;
                        if (d.a <= 0.2f) o = 1.0f;
                        else
                        {
                            fixed jj = 1.0f;
                            if (IN.texcoord.y + m >= 1.0f - jj*texelSize.y) o = 0.0f;
                            if (IN.texcoord.y - m <= jj*texelSize.y) o = 0.0f;
                            if (IN.texcoord.x + l >= 1.0f - jj*texelSize.x) o = 0.0f;
                            if (IN.texcoord.x - l <= jj*texelSize.x) o = 0.0f;
                        }
                        u *= o;
                    }
                }
            	const float pi = 3.141592653589793238462;
                float x = (IN.texcoord.x * _MainTex_TexelSize.x) - _MainTex_TexelSize.x / 2;
                float y = (IN.texcoord.y * _MainTex_TexelSize.y) - _MainTex_TexelSize.y / 2;



				//float xv =  0* pi;
				float xv = _Time.x * 16;
				float eq1 = fmod(xv, 2 * pi);
				float eq2 = fmod(xv + 0.5 * pi,  2 * pi);
				float a = abs(y / sqrt(x * x + y * y));
				float as = asin(a);

				if (x < 0) as = pi - as;
				if (x > 0 && y < 0) as = 2 * pi - as;
				if (x < 0 && y < 0) as = 2 * pi - as;

				if (eq1 > eq2)
				{
					if (y > 0)
						eq1 = 0;
					if (y < 0) eq2 = 2 * pi;
				}
                if (
					(as < eq2 && as > eq1)
				)
                {
                	d = lerp(outlineD, d, ceil(u));
                }

				

				//if (sin(_Time.x * 64) * y >= sqrt(y * y + x * x))
                //{
                //	d = lerp(outlineD, d, ceil(u));
                //}


				return lerp(c, d, sin (_Time.x * 64) * 0.5 + 0.5);
			}
		ENDCG
		}
	}
}
