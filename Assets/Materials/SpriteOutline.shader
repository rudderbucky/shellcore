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
                fixed u = 1.0f;
                
                for (int a=-texelSize.x; a < texelSize.x; a++)
                {
                    for (int b=-texelSize.y; b < texelSize.y; b++)
                    {
                        fixed x = tex2D(_MainTex, i.uv + fixed2(a, b)).a;
				        if (i.uv.y + b >= 1.0f) x = 0.0f;
				        if (i.uv.y - b <= 0.0f) x = 0.0f;
				        if (i.uv.x + a >= 1.0f) x = 0.0f;
				        if (i.uv.x - a <= 0.0f) x = 0.0f;
                        u *= x;
                    }
                }

                if (c.a < 1.0f) u = 0.0f;
 		    
                return lerp(outlineC, c, ceil(u));
            }
            ENDCG
        }
    }
}