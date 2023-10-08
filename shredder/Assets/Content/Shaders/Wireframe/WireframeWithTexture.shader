Shader "Unlit/WireframeWithTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Colour ("Colour", color) = (0.,0.,0.,1)
        [HDR]_BaseColour ("Base Colour", color) = (0.,0.,0.,0.25)
    }
    
    SubShader
    {
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Colour;
            float4 _BaseColour;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // return fixed4(i.uv.xy, 0.0, 1.0);
                const float alpha = tex2D(_MainTex, i.uv).a;
                return ((_Colour * (alpha > 0.1f)) + (_BaseColour * (alpha <= 0.1f))) ;
            }
            ENDCG
        }
    }
}
