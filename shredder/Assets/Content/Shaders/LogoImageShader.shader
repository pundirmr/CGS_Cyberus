Shader "Unlit/LogoImageShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Hue ("Hue", color) = (1,1,1,1)
        [HDR]_Tint("Tint", color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100

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
                float4 colour : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 colour : COLOR;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Hue;
            float4    _Tint;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.colour = v.colour;
                return o;
            }

            float luma(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                return fixed4(_Hue.rgb * _Tint , _Hue.a * col.a);
            }
            ENDCG
        }
    }
}