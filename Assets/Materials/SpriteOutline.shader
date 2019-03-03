// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Outline" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off
        Blend One OneMinusSrcAlpha
       
        Pass {
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
 
            struct v2f {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
            };
 
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
 
            fixed4 _Color;
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);

                //float t = (sin(_Time * 100.0f) + 1.0f) * 0.2f;

                //c.rgb = lerp(c.rgb, _Color, t);
                c.rgb *= c.a;
                half4 outlineC = _Color;
                outlineC.a *= ceil(c.a);
                outlineC.rgb *= outlineC.a;
 				
				fixed2 texelSize = _MainTex_TexelSize * 5.0f;

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
    FallBack "Diffuse"
}