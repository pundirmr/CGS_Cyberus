Shader "Unlit/ImageAbbertation"
{
    Properties
    {
        /*[PerRendererData]*/ _MainTex ("Texture", 2D) = "white" {}
        _ColorR ("ColorR", Color) = (0,0,0,1)
        _ColorG ("ColorG", Color) = (0,0,0,1)
        _ColorB ("ColorB", Color) = (0,0,0,1)
        _AmountX ("AmountX", Vector) = (0,0,0)
        _AmountY ("AmountY", Vector) = (0,0,0)
        _UseTint ("Use Tint", int) = 1

        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

    }
    SubShader
    {

        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 100

        Stencil
        {
            Ref [_Stencil]
            Pass[_StencilOp]
            Comp[_StencilComp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Pass
        {
            Cull Off ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float3    _AmountX;
            float3    _AmountY;
            fixed4    _ColorR;
            fixed4    _ColorG;
            fixed4    _ColorB;
            int       _UseTint;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 r = tex2D(_MainTex, float2(i.uv.x + _AmountX.x, i.uv.y + _AmountY.x));
                fixed4 g = tex2D(_MainTex, float2(i.uv.x + _AmountX.y, i.uv.y + _AmountY.y));
                fixed4 b = tex2D(_MainTex, float2(i.uv.x + _AmountX.z, i.uv.y + _AmountY.z));

                fixed4 col = fixed4(0, 0, 0, 0);

                fixed4 red = _ColorR;
                fixed4 green = _ColorG;
                fixed4 blue = _ColorB;

                if (_UseTint == 0)
                {
                    red = fixed4(i.color.r, 0, 0, r.a);
                    green = fixed4(0, i.color.g, 0, g.a);
                    blue = fixed4(0, 0, i.color.b, b.a);
                }

                if (r.a >= 0.001)
                {
                    col += red;
                }

                if (g.a >= 0.001)
                {
                    col += green;
                }

                if (b.a >= 0.001)
                {
                    col += blue;
                }
                
                col.a *= i.color.a;

                return col;
            }
            ENDCG
        }
    }
}