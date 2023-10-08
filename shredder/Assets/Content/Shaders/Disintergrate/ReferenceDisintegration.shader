Shader "Manifold/ReferenceDisintegration"{

    Properties{
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpStr("Normal Map Strength", float) = 1
 
        _FlowMap("Flow (RG)", 2D) = "black" {}
        _DissolveTexture("Dissolve Texture", 2D) = "white" {}
        _DissolveColor("Dissolve Color Border", Color) = (1, 1, 1, 1) 
        _DissolveBorder("Dissolve Border", float) =  0.05


        _Expand("Expand", float) = 1
        _Weight("Weight", Range(0,1)) = 0
        _Direction("Direction", Vector) = (0, 0, 0, 0)
        _DisintegrationColor("Disintegration Color", Color) = (1, 1, 1, 1)
        _Glow("Glow", float) = 1

        _Shape("Shape Texture", 2D) = "white" {} 
        _R("Radius", float) = .1

    }

    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);

            
        CBUFFER_START(UnityPerMatrial)
            sampler2D _DissolveTexture;
            sampler2D _Shape;
            
            float4 _MainTex_ST;
            float4 _BumpMap_ST;
            float4 _FlowMap_ST;

            float _DissolveBorder;
            float _Weight;
            float _Glow;
            float _Expand;
            float _BumpStr;
            float _R;

            float4 _Direction;
            float4 _AmbientColor;
            float4 _DissolveColor;
            float4 _DisintegrationColor;
            float4 _Color;
            float _Metallic;
        CBUFFER_END;
        

        // because I can't find an identity mat for unity...
        #define IDENTITY_MAT float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1)

        // uniform float3 _WorldLightPos;
        // uniform half4 _LightColour;
        #define _WorldLightPos float3(0, 0, 0)
        #define _LightColour half4(1, 1, 1, 1)
    
        struct appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct v2g {
            float4 objPos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 normal : NORMAL;
            float3 worldPos : TEXCOORD1;
        };

        struct g2f {
            float4 worldPos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
            float3 normal : NORMAL;
        };


        v2g vert (appdata v) {
            v2g o;
            o.objPos = v.vertex;
            o.uv = v.uv;
            o.normal = TransformObjectToWorldNormal(v.normal);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            return o;
        }

        float random (float2 uv) {
            return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
        }

        float remap (float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        float randomMapped(float2 uv, float from, float to) {
            return remap(random(uv), 0, 1, from, to);
        }

        float4 remapFlowTexture(float4 tex) {
            return float4(
                remap(tex.x, 0, 1, -1, 1),
                remap(tex.y, 0, 1, -1, 1),
                0,
                remap(tex.w, 0, 1, -1, 1)
            );
        }


        [maxvertexcount(7)]
        void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {            
            float2 avgUV = (IN[0].uv + IN[1].uv + IN[2].uv) / 3;
            float3 avgPos = (IN[0].objPos + IN[1].objPos + IN[2].objPos) / 3;
            float3 avgNormal = (IN[0].normal + IN[1].normal + IN[2].normal) / 3;

            float dissolve_value = tex2Dlod(_DissolveTexture, float4(avgUV, 0, 0)).r;
            float t = clamp(_Weight * 2 - dissolve_value, 0 , 1);

            float2 flowUV = TRANSFORM_TEX(mul(unity_ObjectToWorld, avgPos).xz, _FlowMap);
            float4 flowVector = remapFlowTexture(_FlowMap.SampleLevel(sampler_FlowMap, float4(flowUV, 0, 0), 0));

            float3 pseudoRandomPos = (avgPos) + _Direction;
            pseudoRandomPos += (flowVector.xyz * _Expand);

            float3 p =  lerp(avgPos, pseudoRandomPos, t);
            float radius = lerp(_R, 0, t);
            

            if(t > 0) {
                float3 look = _WorldSpaceCameraPos - p;
                look = normalize(look);

                float3 right = UNITY_MATRIX_IT_MV[0].xyz;
                float3 up = UNITY_MATRIX_IT_MV[1].xyz;

                float halfS = 0.5f * radius;

                float4 v[4];
                v[0] = float4(p + halfS * right - halfS * up, 1.0f);
                v[1] = float4(p + halfS * right + halfS * up, 1.0f);
                v[2] = float4(p - halfS * right - halfS * up, 1.0f);
                v[3] = float4(p - halfS * right + halfS * up, 1.0f);
                
                
                g2f vert;
                vert.worldPos = TransformObjectToHClip(v[0]);
                vert.uv = (float2)mul(IDENTITY_MAT, float2(1.0f, 0.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);
                
                vert.worldPos = TransformObjectToHClip(v[1]);
                vert.uv = (float2)mul(IDENTITY_MAT, float2(1.0f, 1.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);

                vert.worldPos = TransformObjectToHClip(v[2]);
                vert.uv = (float2)mul(IDENTITY_MAT, float2(0.0f, 0.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);

                vert.worldPos = TransformObjectToHClip(v[3]);
                vert.uv = (float2)mul(IDENTITY_MAT, float2(0.0f, 1.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);

                triStream.RestartStrip();
            }

            for(int j = 0; j < 3; j++){
                g2f o;
                o.worldPos = TransformObjectToHClip(IN[j].objPos);
                o.uv = TRANSFORM_TEX(IN[j].uv, _MainTex);
                o.color = float4(0, 0, 0, 0);
                o.normal = IN[j].normal;
                triStream.Append(o); 
            }
            
            triStream.RestartStrip();
        }

    ENDHLSL
        
    SubShader{

        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Cull Off

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma multi_compile_fwdbase
           

            float4 frag (g2f i) : SV_Target {
                float4 col = _MainTex.Sample(sampler_MainTex, i.uv) * _Color;
                
                float3 normal = normalize(i.normal);
                half3 tnormal = UnpackNormal(_BumpMap.Sample(sampler_BumpMap, i.uv));
                tnormal.xy *= _BumpStr;
                tnormal = normalize(tnormal);
                
                float NdotL = dot(_WorldLightPos, normal * tnormal);
                float4 light = NdotL * _LightColour;
                col *= (_AmbientColor + light);
               
                float brightness = i.color.w  * _Glow;
                col = lerp(col, _DisintegrationColor,  i.color.x);

                if(brightness > 0){
                    col *= brightness + _Weight;
                }


                float dissolve = tex2D(_DissolveTexture, i.uv).r;
                
                if(i.color.w == 0){
                    clip(dissolve - 2*_Weight);
                    if(_Weight > 0){
                        col +=  _DissolveColor * _Glow * step(dissolve - 2 * _Weight, _DissolveBorder);
                    }
                }else{
                    float s = tex2D(_Shape, i.uv).r;
                    if(s < .5) {
                        discard;
                    }

                }

                return col;
            }
            ENDHLSL
        }

//        Pass{
//            Tags{
//                "LightMode" = "ShadowCaster"
//            }
//
//            HLSLPROGRAM
//            #pragma vertex vert
//            #pragma geometry geom
//            #pragma fragment frag
//            #pragma target 4.6
//            #pragma multi_compile_shadowcaster
//
//            float4 frag(g2f i) : SV_Target{
//                float dissolve = tex2D(_DissolveTexture, i.uv).r;
//
//                if(i.color.w == 0){
//                    clip(dissolve - 2 * _Weight);
//                }else{
//                    float s = tex2D(_Shape, i.uv).r;
//                    if(s < .5) {
//                        discard;
//                    }
//                }
//
//                #include "UnityCG.cginc"
//                SHADOW_CASTER_FRAGMENT(i)
//            }
//
//            ENDHLSL
//        }
    }
}
