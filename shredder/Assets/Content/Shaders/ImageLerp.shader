Shader "Unlit/ImageLerp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TextA ("TextA", 2D) = "white" {}
        _TextB ("TextB", 2D) = "white" {}
        _LerpVal ("Lerp Value", range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 colour: COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 colour : TEXCOORD1;
            };

            sampler2D _TextA;
            float4 _TextA_ST;

            sampler2D _TextB;
            float4 _TextB_ST;

            float _LerpVal;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _TextA);
                o.colour = v.colour;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = lerp(tex2D(_TextA, i.uv), tex2D(_TextB, i.uv), _LerpVal);
                col *=  i.colour;
                return col;
            }
            ENDCG
        }
    }
}
