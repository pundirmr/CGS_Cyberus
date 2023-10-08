Shader "Custom/Geometry/WireframeForDissolve"
{
    Properties
    {
        [PowerSlider(3.0)]
        _WireframeVal ("Wireframe width", Range(0., 0.5)) = 0.05
        [HDR]_Colour ("Colour", color) = (1., 1., 1., 1.)
        [HDR]_BaseColour ("Base Colour", color) = (1., 1., 1., 0.5)
        _ClipPoint ("Clip Point", Range(0., 1)) = 0.5
        [Toggle] _RemoveDiag("Remove diagonals?", Float) = 0.
        _Scale ("Scale", Vector) = (1,1,1)
    }
    SubShader
    {
        Tags
        {
            /*"Queue"="Geometry"*/ "Queue"="Transparent" "RenderType"="Transparent"
        }

        Pass
        {
            Cull back
            ZWrite off
            ZTest Gequal
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
                // float4 position : POSTION;
                float3 vertex : TEXCOORD3;
                float2 uv : TEXCOORD0;
            };

            struct g2f
            {
                float4 worldPos : SV_POSITION;
                // float4 position : POSTION;
                float3 bary : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 vertex : TEXCOORD4;
                float3 scale : TEXCOORD5;
            };

            v2g vert(appdata_base v)
            {
                v2g o;
                o.uv = v.texcoord;
                o.vertex = v.vertex;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                // o.position = v.vertex;
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

                float a = abs((IN[0].vertex.x - IN[2].vertex.x) * (IN[0].vertex.x - IN[2].vertex.x));
                float b = abs((IN[0].vertex.y - IN[1].vertex.y) * (IN[0].vertex.y - IN[1].vertex.y));
                // float z = sqrt(a + b);
                float z = abs((IN[0].vertex.z - IN[1].vertex.z) * (IN[0].vertex.z - IN[1].vertex.z));
                float3 scale = float3((a - 1),(b - 1), 0);
                
                g2f    o;
                o.uv = IN[0].uv;
                o.vertex = IN[0].vertex;
                o.worldPos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                o.scale = scale;
                triStream.Append(o);
                o.worldPos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                o.uv = IN[1].uv;
                o.vertex = IN[1].vertex;
                o.scale = scale;
                triStream.Append(o);
                o.worldPos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                o.uv = IN[2].uv;
                o.vertex = IN[2].vertex;
                o.scale = scale;
                triStream.Append(o);
            }

            float  _WireframeVal;
            float  _ClipPoint;
            fixed4 _Colour;
            fixed4 _BaseColour;
            float3 _Scale;

            fixed4 frag(g2f i) : SV_Target
            {
                // return fixed4(i.scale.xyz, 1);
                // return fixed4(i.vertex + 0.5, 1);
                return fixed4(i.bary.xyz, 1);
                if (!any(bool3(i.bary.x  < _WireframeVal * (1 / i.scale.x), i.bary.y < _WireframeVal,
                               i.bary.z < _WireframeVal)))
                    return (_BaseColour * (_WireframeVal >= 0)) * (i.uv.y >= (/*1 -*/ _ClipPoint));

                return _Colour * (i.uv.y > (/*1 -*/ _ClipPoint));
            }
            ENDCG
        }
    }
}