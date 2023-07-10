Shader "Custom/Outline" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        // these six unused properties are required when a shader
        // is used in the UI system, or you get a warning.
        // look to UI-Default.shader to see these.
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        // see for example
        // http://answers.unity3d.com/questions/980924/ui-mask-with-shader.html
    }
    SubShader {

        // required for UI.Mask
        Stencil
        {
             Ref [_Stencil]
             Comp [_StencilComp]
             Pass [_StencilOp] 
             ReadMask [_StencilReadMask]
             WriteMask [_StencilWriteMask]
        }
          ColorMask [_ColorMask]
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
 				
				fixed2 texelSize = _MainTex_TexelSize * 4.0f;

                fixed alpha_up = tex2D(_MainTex, i.uv + fixed2(0, texelSize.y)).a;
                fixed alpha_down = tex2D(_MainTex, i.uv - fixed2(0, texelSize.y)).a;
                fixed alpha_right = tex2D(_MainTex, i.uv + fixed2(texelSize.x, 0)).a;
                fixed alpha_left = tex2D(_MainTex, i.uv - fixed2(texelSize.x, 0)).a;
                fixed alpha_leftdown = tex2D(_MainTex, i.uv - fixed2(texelSize.x, texelSize.y)).a;
                fixed alpha_leftup = tex2D(_MainTex, i.uv - fixed2(texelSize.x, -texelSize.y)).a;
                fixed alpha_rightdown = tex2D(_MainTex, i.uv + fixed2(texelSize.x, -texelSize.y)).a;
                fixed alpha_rightup = tex2D(_MainTex, i.uv + fixed2(texelSize.x, texelSize.y)).a;


				if (alpha_up > 0.0f && i.uv.y + texelSize.y > 1.0f) alpha_up = 0.0f;
				if (alpha_down > 0.0f && i.uv.y - texelSize.y < 0.0f) alpha_down = 0.0f;
				if (alpha_right > 0.0f && i.uv.x + texelSize.x > 1.0f) alpha_right = 0.0f;
				if (alpha_left > 0.0f && i.uv.x - texelSize.x < 0.0f) alpha_left = 0.0f;
 		    
                return lerp(outlineC, c, ceil(alpha_up * alpha_down * alpha_right * alpha_left * alpha_leftdown * alpha_leftup * alpha_rightdown * alpha_rightup));
            }
            ENDCG
        }
    }
}