Shader "Custom/Geometry/Wireframe2D"
{
    Properties
    {
        [PowerSlider(3.0)]
        _WireframeVal ("Wireframe width", Range(0., 0.5)) = 0.05
        [HDR]_Colour ("Colour", color) = (1., 1., 1., 1.)
        [HDR]_BaseColour ("Base Colour", color) = (1., 1., 1., 0.5)
        _ClipPoint ("Clip Point", Range(0., 1)) = 0.5
        [Toggle] _RemoveDiag("Remove diagonals?", Float) = 0.
    }
    SubShader
    {
        Tags
        {
            /*"Queue"="Geometry"*/ "Queue"="Transparent" "RenderType"="Transparent"
        }

        Pass
        {
            Cull off
//            ZWrite on
//            ZTest LEqual  
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            struct v2g
            {
                float4 worldPos : SV_POSITION;
                float3 vertex : TEXCOORD3;
                float2 uv : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 vertex : TEXCOORD4;
            };

            v2g vert(appdata_base v)
            {
                v2g o;
                o.uv = v.texcoord;
                o.vertex = v.vertex;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float3 param = float3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;
                o.uv = IN[0].uv;
                o.vertex = IN[0].vertex;
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                o.uv = IN[1].uv;
                o.vertex = IN[1].vertex;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                o.uv = IN[2].uv;
                o.vertex = IN[2].vertex;
                triStream.Append(o);
            }

            float  _WireframeVal;
            float _ClipPoint;
            fixed4 _Colour;
            fixed4 _BaseColour; 

            fixed4 frag(g2f i) : SV_Target
            {
                if (!any(bool3(i.bary.x < _WireframeVal, i.bary.y < _WireframeVal, i.bary.z < _WireframeVal)))
                    return _BaseColour * (i.uv.y > (/*1 -*/ _ClipPoint));
                
                return _Colour * (i.uv.y > (/*1 -*/ _ClipPoint));
            }
            
            ENDCG
        }
    }
}