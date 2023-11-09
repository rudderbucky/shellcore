Shader "Sprites/PartyWheelCursor" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Offset ("Offset", Float) = 0

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
            float _Offset;

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
                _Offset *= 0.5F * 3.1415;
                float rad = _MainTex_TexelSize.y / 2;
                float rad2 = _MainTex_TexelSize.y / 2 * 3 / 5;
                float x = (i.uv.x * _MainTex_TexelSize.x) - _MainTex_TexelSize.x / 2;
                float y = (i.uv.y * _MainTex_TexelSize.y) - _MainTex_TexelSize.y / 2;
                float test = x * x + y * y;
                
                if (sqrt(test) > rad)
                {
                    c.a = 0;
                    c.rgb *= 0;
                }

                if (sqrt(test) < rad2)
                {
                    c.a = 0;
                    c.rgb *= 0;
                }

                if (tan(0.6283) * x < y)
                {
                    c.a = 0;
                    c.rgb *= 0;
                }

                
                if (tan(-0.6283) * x > y)
                {
                    c.a = 0;
                    c.rgb *= 0;
                }

                c.a *= (sin(_Time*64+ _Offset * 3.14) / 4) + 0.75F;
                c.a *= 0.25F;

                c.rgb *= c.a;

                return c;
            }
            ENDCG
        }
    }
}