Shader "Custom/Outline" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader {

        Cull Off
        Blend One OneMinusSrcAlpha
       
        Pass {
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;

            struct appdata {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                half2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
 
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }
 
            fixed4 _Color;
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);

                c *= i.color;
                c.rgb *= c.a;
                half4 outlineC = _Color;
                outlineC.a *= ceil(c.a);
                outlineC.rgb *= outlineC.a;
 				
				fixed2 texelSize = _MainTex_TexelSize * 9.0f;

                fixed alpha_up = tex2D(_MainTex, i.uv + fixed2(0, texelSize.y)).a;
                fixed alpha_down = tex2D(_MainTex, i.uv - fixed2(0, texelSize.y)).a;
                fixed alpha_right = tex2D(_MainTex, i.uv + fixed2(texelSize.x, 0)).a;
                fixed alpha_left = tex2D(_MainTex, i.uv - fixed2(texelSize.x, 0)).a;

				if (alpha_up > 0.9f && i.uv.y + texelSize.y > 1.0f) alpha_up = 0.0f;
				if (alpha_down > 0.9f && i.uv.y - texelSize.y < 0.0f) alpha_down = 0.0f;
				if (alpha_right > 0.9f && i.uv.x + texelSize.x > 1.0f) alpha_right = 0.0f;
				if (alpha_left > 0.9f && i.uv.x - texelSize.x < 0.0f) alpha_left = 0.0f;
 		    
                return lerp(outlineC, c, ceil(alpha_up * alpha_down * alpha_right * alpha_left));
            }
            ENDCG
        }
    }
}