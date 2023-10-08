Shader "Manifold/Disintegrate" {
    Properties {
        _MainTex("Texture", 2D) = "white" {}
        [HDR]_TopColour("Top Colour", Color) = (0, 0, 0, 1)
        _BottomColour("Bottom Colour", Color) = (1, 1, 1, 1)
        _Threshold("Threshold", Float) = 1
        _Direction("Direction", Float) = 0
        _RateOfDisintegration("Rate Of Disintegration", Float) = 1
        _NumOfLayers("Num Of Layers", Integer) = 2
    }

    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct V_IN {
                float4 vert_pos : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct V_OUT_G_IN {
                float4 vert_pos : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct G_OUT_F_IN {
                float4 vert_pos : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 colour   : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _BottomColour;
            float4 _TopColour;
            float _Threshold;
            float _Direction;
            float _RateOfDisintegration;
            int _NumOfLayers;
            CBUFFER_END;
            
            V_OUT_G_IN vert(V_IN input) {
                V_OUT_G_IN output;
                output.vert_pos = float4(TransformObjectToWorld(input.vert_pos), input.vert_pos.w);
                output.uv       = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            #define MAX_NUMBER_OF_LAYERS 20
            [maxvertexcount(3 * MAX_NUMBER_OF_LAYERS)]
            void geom(triangle V_OUT_G_IN input[3], inout TriangleStream<G_OUT_F_IN> tri_stream) {
                float4 center = (input[0].vert_pos + input[1].vert_pos + input[2].vert_pos) / 3;
                
                float mod = 1;
                float add = 20;
				G_OUT_F_IN output = (G_OUT_F_IN)0;

                for (int n = 0; n < _NumOfLayers && n < MAX_NUMBER_OF_LAYERS; ++n) {
                    for (int i = 0; i < 3; ++i) {
                        if (center.y >= _Threshold) {
                            float t    = center.y - _Threshold;
                            float v    = _Direction * (t + 0.5 * t * t);
                            float4 dir = input[i].vert_pos - center;

                            output.vert_pos = center;
                            if (t * _RateOfDisintegration <= 1) {
                                output.vert_pos += dir;
                            } else {
                                output.vert_pos += dir / (t * _RateOfDisintegration);
                            }

                            output.vert_pos.y += v * mod;
                            output.vert_pos    = TransformObjectToHClip((float3)output.vert_pos);
                            output.uv          = input[i].uv;
                            output.colour      = lerp(_BottomColour, _TopColour, t * _RateOfDisintegration);
                            tri_stream.Append(output);
                        } else {
                            output.vert_pos = input[i].vert_pos;
                            output.vert_pos = TransformObjectToHClip((float3)output.vert_pos);
                            output.uv       = input[i].uv;
                            output.colour   = _BottomColour;
                            tri_stream.Append(output);
                        }
                    }

                    mod += add;
                    tri_stream.RestartStrip();
                }
            }

            float4 frag(G_OUT_F_IN input) : SV_Target {
                float4 colour = _MainTex.Sample(sampler_MainTex, input.uv) * input.colour;
                return colour;
            }
            ENDHLSL
        }
    }
}
