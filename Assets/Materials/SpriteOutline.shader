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
 				
				fixed4 texelSize = _MainTex_TexelSize ;
                float u = 1.0f;

                for (fixed l=-texelSize.x* 2.0f; l <= texelSize.x* 3.0f; l += texelSize.x)
                {
                    for (fixed m=-texelSize.y* 2.0f; m <= texelSize.y* 3.0f; m += texelSize.y)
                    {
                        fixed o = tex2D(_MainTex, i.uv + fixed2(l, m)).a;
                        if (c.a <= 0.2f) o = 1.0f;
                        else
                        {
                            fixed jj = 1.0f;
                            if (i.uv.y + m >= 1.0f - jj*texelSize.y) o = 0.0f;
                            if (i.uv.y - m <= jj*texelSize.y) o = 0.0f;
                            if (i.uv.x + l >= 1.0f - jj*texelSize.x) o = 0.0f;
                            if (i.uv.x - l <= jj*texelSize.x) o = 0.0f;
                        }
                        u *= o;
                        //o = floor(o);
                    }
                }

                return lerp(outlineC, c, ceil(u));
            }
            ENDCG
        }
    }
}