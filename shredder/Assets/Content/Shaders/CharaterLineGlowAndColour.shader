Shader "Unlit/CharaterLineGlowAndColour"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _GlowColour ("Glow Colour", color) = (0, 0, 0, 1)
        _DeltaX ("Delta X", Float) = 0.01
		_DeltaY ("Delta Y", Float) = 0.01
        _EdgeThickness ("Edge", Float) = 0.01
        
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
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
                fixed4 colour : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 colour : TEXCOORD1;
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

            float _DeltaX;
            float _DeltaY;
            fixed4 _GlowColour;

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
                float sobelVal = sobel(_MainTex, i.uv);
                fixed4 sampleCol = tex2D(_MainTex, i.uv);
                fixed4 col = fixed4(((i.colour * sampleCol) + (_GlowColour * sobelVal)).xyz, sampleCol.a);
                return col;
            }
            ENDCG
        }
    }
}