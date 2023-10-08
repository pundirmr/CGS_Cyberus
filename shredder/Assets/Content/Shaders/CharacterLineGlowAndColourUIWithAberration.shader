Shader "Unlit/CharaterLineGlowAndColour"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _FadeMask ("Alpha Texture", 2D) = "white" {}
        [HDR] _GlowColour ("Glow Colour", color) = (0, 0, 0, 1)
        _DeltaX ("Delta X", Float) = 0.01
        _DeltaY ("Delta Y", Float) = 0.01
        _EdgeThickness ("Edge", Float) = 0.01

        _AmountX ("AmountX", Vector) = (0,0,0)
        _AmountY ("AmountY", Vector) = (0,0,0)


        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

    }
    SubShader
    {

        LOD 100

        Stencil
        {
            Ref [_Stencil]
            Pass[_StencilOp]
            Comp[_StencilComp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }


        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Cull off
            ZWrite on
            ZTest Lequal
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 colour : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 colour : COLOR;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.colour = v.colour;
                return o;
            }


            sampler2D _AlphaTex;
            float4    _AlphaTex_ST;

            float  _DeltaX;
            float  _DeltaY;
            fixed4 _GlowColour;

            float3 _AmountX;
            float3 _AmountY;
            // fixed4 _ColorR;
            // fixed4 _ColorG;
            // fixed4 _ColorB;
            // int    _UseTint;

            float sobel(sampler2D tex, float2 uv)
            {
                float2 delta = float2(_DeltaX, _DeltaY);

                float4 hr = float4(0, 0, 0, 0);
                float4 vt = float4(0, 0, 0, 0);

                hr += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) * 1.0;
                hr += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) * 0.0;
                hr += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) * -1.0;
                hr += tex2D(tex, (uv + float2(-1.0, 0.0) * delta)) * 2.0;
                hr += tex2D(tex, (uv + float2(0.0, 0.0) * delta)) * 0.0;
                hr += tex2D(tex, (uv + float2(1.0, 0.0) * delta)) * -2.0;
                hr += tex2D(tex, (uv + float2(-1.0, 1.0) * delta)) * 1.0;
                hr += tex2D(tex, (uv + float2(0.0, 1.0) * delta)) * 0.0;
                hr += tex2D(tex, (uv + float2(1.0, 1.0) * delta)) * -1.0;

                vt += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) * 1.0;
                vt += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) * 2.0;
                vt += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) * 1.0;
                vt += tex2D(tex, (uv + float2(-1.0, 0.0) * delta)) * 0.0;
                vt += tex2D(tex, (uv + float2(0.0, 0.0) * delta)) * 0.0;
                vt += tex2D(tex, (uv + float2(1.0, 0.0) * delta)) * 0.0;
                vt += tex2D(tex, (uv + float2(-1.0, 1.0) * delta)) * -1.0;
                vt += tex2D(tex, (uv + float2(0.0, 1.0) * delta)) * -2.0;
                vt += tex2D(tex, (uv + float2(1.0, 1.0) * delta)) * -1.0;

                return sqrt(hr * hr + vt * vt);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float sobelValR = clamp(sobel(_MainTex, float2(i.uv.x + _AmountX.x, i.uv.y + _AmountY.x)), 0.0, 1.0);
                float sobelValB = clamp(sobel(_MainTex, float2(i.uv.x + _AmountX.y, i.uv.y + _AmountY.y)), 0.0, 1.0);
                float sobelValG = clamp(sobel(_MainTex, float2(i.uv.x + _AmountX.z, i.uv.y + _AmountY.z)), 0.0, 1.0);

                fixed4 sampleColR = tex2D(_MainTex, float2(i.uv.x + _AmountX.x, i.uv.y + _AmountY.x));
                fixed4 sampleColG = tex2D(_MainTex, float2(i.uv.x + _AmountX.y, i.uv.y + _AmountY.y));
                fixed4 sampleColB = tex2D(_MainTex, float2(i.uv.x + _AmountX.z, i.uv.y + _AmountY.z));

                float maskAlpha = tex2D(_AlphaTex, i.uv).a;

                fixed4 sobelColour =
                fixed4((i.colour.r * sampleColR.r) + (_GlowColour.r * sobelValR),
                       (i.colour.g * sampleColG.g) + (_GlowColour.g * sobelValG),
                       (i.colour.b * sampleColB.b) + (_GlowColour.b * sobelValB),
                       clamp(((sampleColR.a + sampleColG.a + sampleColB.a) * i.colour.a) * maskAlpha, 0, 1));

                return sobelColour;
            }
            ENDCG
        }
    }
}