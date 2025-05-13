Shader "Custom/LavaWall"
{
    Properties
    {
        [Header(Textures)]
        _MainTex ("Lava Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        
        [Header(Colors)]
        _BaseColor ("Base Color", Color) = (0.5, 0.0, 0.0, 1.0)
        _LavaColor ("Lava Color", Color) = (1.0, 0.3, 0.0, 1.0)
        _HotLavaColor ("Hot Spots Color", Color) = (1.0, 0.8, 0.0, 1.0)
        
        [Header(Flow Settings)]
        _FlowDirection ("Flow Direction", Vector) = (0.0, -1.0, 0.0, 0.0)
        _FlowSpeed ("Flow Speed", Range(0.1, 5.0)) = 0.8
        _FlowIntensity ("Flow Intensity", Range(0.1, 2.0)) = 0.5
        
        [Header(Lava Effect)]
        _DisplacementAmount ("Displacement Amount", Range(0.0, 0.5)) = 0.05
        _BumpPower ("Surface Bumpiness", Range(0.0, 1.0)) = 0.4
        _LavaBrightness ("Lava Brightness", Range(0.0, 5.0)) = 2.0
        _PulsateSpeed ("Pulsate Speed", Range(0.1, 10.0)) = 1.5
        _PulsateAmount ("Pulsate Amount", Range(0.0, 1.0)) = 0.4
        
        [Header(Hot Spots)]
        _HotspotFrequency ("Hotspot Frequency", Range(1.0, 20.0)) = 10.0
        _HotspotSpeed ("Hotspot Speed", Range(0.1, 5.0)) = 0.7
        _HotspotIntensity ("Hotspot Intensity", Range(0.0, 2.0)) = 1.0
        
        [Space(10)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.7
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "LavaForward"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex LitVertex
            #pragma fragment LitFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS    : SV_POSITION;
                float3 positionWS    : TEXCOORD0;
                float3 normalWS      : TEXCOORD1;
                float3 viewDirWS     : TEXCOORD2;
                float3 positionOS    : TEXCOORD3;
                float2 uv            : TEXCOORD4;
                float3 tangentWS     : TEXCOORD5;
                float3 bitangentWS   : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                half4 _BaseColor;
                half4 _LavaColor;
                half4 _HotLavaColor;
                float3 _FlowDirection;
                half _FlowSpeed;
                half _FlowIntensity;
                half _DisplacementAmount;
                half _BumpPower;
                half _LavaBrightness;
                half _PulsateSpeed;
                half _PulsateAmount;
                half _HotspotFrequency;
                half _HotspotSpeed;
                half _HotspotIntensity;
                half _Smoothness;
                half _Metallic;
            CBUFFER_END

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(443.897, 441.423, 437.195));
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float voronoi(float2 x, float u, float v)
            {
                float2 n = floor(x);
                float2 f = frac(x);
                
                float r = 1.0;
                
                for(int j = -1; j <= 1; j++)
                {
                    for(int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 o = hash22(n + g);
                        o = 0.5 + 0.3 * sin(_Time.y * u + 6.2831 * o);
                        
                        float2 d = g - f + o;
                        float t = dot(d, d);
                        t = pow(t, v * 0.8 + 0.2);
                        r = min(r, t);
                    }
                }
                
                return saturate(sqrt(r));
            }

            float generateLavaDisplacement(float2 uv, float3 positionOS, float time)
            {
                float2 noiseUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float noise = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, noiseUV + float2(time * 0.1, time * 0.05), 0).r;
                
                float2 voronoiUV = uv * _HotspotFrequency;
                float voronoiNoise = voronoi(voronoiUV, _HotspotSpeed, 0.7);
                
                float displacement = noise * 0.6 + (1.0 - voronoiNoise) * 0.4;
                displacement = smoothstep(0.0, 1.0, displacement);
                
                return displacement * _DisplacementAmount * 0.5;
            }

            float3 generateLavaColor(float2 uv, float3 positionWS, float time)
            {
                float2 flowVector = normalize(_FlowDirection.xy) * _FlowIntensity;
                float2 flowUV = uv - flowVector * time * _FlowSpeed;
                float2 baseUV = flowUV * _MainTex_ST.xy + _MainTex_ST.zw;
                float3 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, baseUV).rgb;
                
                float2 noiseUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float noise1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV + float2(time * 0.2, time * 0.1)).r;
                float noise2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV * 2.0 - float2(time * 0.15, time * 0.25)).r;
                
                float2 voronoiUV = uv * _HotspotFrequency;
                float voronoiNoise = voronoi(voronoiUV, _HotspotSpeed, 0.7);
                
                float pulse = 0.5 + 0.5 * sin(time * _PulsateSpeed);
                float pulseFactor = 1.0 + pulse * _PulsateAmount;
                
                float2 crackUV = uv - flowVector * time * (_FlowSpeed * 0.5);
                float crackNoise = voronoi(crackUV * _HotspotFrequency * 0.5, _HotspotSpeed * 0.7, 0.6);
                
                float hotIntensity = pow(1.0 - voronoiNoise, 2.0) * _HotspotIntensity * pulseFactor;
                float crackIntensity = pow(1.0 - crackNoise, 3.0) * 0.7 * pulseFactor;
                
                float3 lavaColor = lerp(_BaseColor.rgb, _LavaColor.rgb, noise1 * 0.7 + 0.3);
                lavaColor = lerp(lavaColor, _HotLavaColor.rgb, hotIntensity);
                lavaColor = lerp(lavaColor, _HotLavaColor.rgb, crackIntensity);
                lavaColor *= (1.0 + pulse * 0.2);
                
                return lavaColor;
            }

            float3 generateLavaEmission(float2 uv, float3 positionWS, float time)
            {
                float2 voronoiUV = uv * _HotspotFrequency;
                float voronoiNoise = voronoi(voronoiUV, _HotspotSpeed, 0.7);
                
                float2 flowVector = normalize(_FlowDirection.xy) * _FlowIntensity;
                float2 crackUV = uv - flowVector * time * (_FlowSpeed * 0.5);
                float crackNoise = voronoi(crackUV * _HotspotFrequency * 0.5, _HotspotSpeed * 0.7, 0.6);
                
                float pulse = 0.5 + 0.5 * sin(time * _PulsateSpeed);
                float pulseFactor = 1.0 + pulse * _PulsateAmount;
                
                float hotIntensity = pow(1.0 - voronoiNoise, 3.0) * _HotspotIntensity * pulseFactor;
                float crackIntensity = pow(1.0 - crackNoise, 4.0) * 0.8 * pulseFactor;
                
                float3 emission = _HotLavaColor.rgb * (hotIntensity + crackIntensity) * _LavaBrightness;
                
                return emission;
            }

            Varyings LitVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionOS = input.positionOS.xyz;
                output.uv = input.uv;
                
                float time = _Time.y;
                float displacement = generateLavaDisplacement(input.uv, input.positionOS.xyz, time);
                
                float edgeFactor = min(min(input.uv.x, 1.0 - input.uv.x), min(input.uv.y, 1.0 - input.uv.y));
                edgeFactor = smoothstep(0.0, 0.2, edgeFactor);
                displacement *= edgeFactor;
                
                float3 positionOS = input.positionOS.xyz + input.normalOS * displacement;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.viewDirWS = GetWorldSpaceViewDir(output.positionWS);
                
                return output;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                float time = _Time.y;
                float3 normalTS = float3(0, 0, 1);
                
                float2 noiseUV = input.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float bumpX = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV + float2(time * 0.1, 0)).r;
                float bumpY = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV + float2(0, time * 0.1)).r;
                float2 bump = (float2(bumpX, bumpY) * 2.0 - 1.0) * _BumpPower;
                
                normalTS.xy += bump;
                normalTS.z = sqrt(1.0 - saturate(dot(normalTS.xy, normalTS.xy)));
                
                float3x3 tangentToWorld = float3x3(
                    normalize(input.tangentWS),
                    normalize(input.bitangentWS),
                    normalWS
                );
                float3 bumpedNormalWS = mul(normalTS, tangentToWorld);
                
                // Generar color y emisión ignorando las sombras
                float3 lavaColor = generateLavaColor(input.uv, input.positionWS, time);
                float3 emission = generateLavaEmission(input.uv, input.positionWS, time);
                
                // Crear una estructura de SurfaceData básica
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = lavaColor;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = emission;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1.0;
                
                // Preparar InputData pero sin sombras
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = bumpedNormalWS;
                inputData.viewDirectionWS = viewDirWS;
                
                // Crear un shadowCoord con valores que eviten sombras
                inputData.shadowCoord = float4(1.0, 1.0, 1.0, 1.0);
                
                // Obtener la dirección e intensidad de la luz principal
                #ifdef _MAIN_LIGHT_SHADOWS
                    Light mainLight = GetMainLight();
                #else
                    Light mainLight = GetMainLight(inputData.shadowCoord);
                #endif
                
                // Aplica la iluminación pero con un factor adicional para hacerla más brillante
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // Añadir más brillo para contrarrestar las sombras
                color.rgb += emission * 0.5;
                
                return color;
            }
            ENDHLSL
        }
        
        // Shadow casting pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                half _DisplacementAmount;
                half _HotspotFrequency;
                half _HotspotSpeed;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(443.897, 441.423, 437.195));
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float voronoi(float2 x, float u, float v)
            {
                float2 n = floor(x);
                float2 f = frac(x);
                
                float r = 1.0;
                for(int j = -1; j <= 1; j++)
                {
                    for(int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 o = hash22(n + g);
                        o = 0.5 + 0.3 * sin(_Time.y * u + 6.2831 * o);
                        
                        float2 d = g - f + o;
                        float t = dot(d, d);
                        t = pow(t, v * 0.8 + 0.2);
                        r = min(r, t);
                    }
                }
                return saturate(sqrt(r));
            }

            float generateLavaDisplacement(float2 uv, float time)
            {
                float2 noiseUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float noise = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, noiseUV + float2(time * 0.1, time * 0.05), 0).r;
                
                float2 voronoiUV = uv * _HotspotFrequency;
                float voronoiNoise = voronoi(voronoiUV, _HotspotSpeed, 0.7);
                
                float displacement = noise * 0.6 + (1.0 - voronoiNoise) * 0.4;
                
                float edgeFactor = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                edgeFactor = smoothstep(0.0, 0.2, edgeFactor);
                
                displacement = smoothstep(0.0, 1.0, displacement) * _DisplacementAmount * 0.5 * edgeFactor;
                
                return displacement;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float time = _Time.y;
                float displacement = generateLavaDisplacement(input.uv, time);
                
                float3 positionOS = input.positionOS.xyz + input.normalOS * displacement;
                output.positionCS = TransformObjectToHClip(positionOS);
                
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            
            ENDHLSL
        }
        
        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                half _DisplacementAmount;
                half _HotspotFrequency;
                half _HotspotSpeed;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(443.897, 441.423, 437.195));
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float voronoi(float2 x, float u, float v)
            {
                float2 n = floor(x);
                float2 f = frac(x);
                
                float r = 1.0;
                for(int j = -1; j <= 1; j++)
                {
                    for(int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 o = hash22(n + g);
                        o = 0.5 + 0.3 * sin(_Time.y * u + 6.2831 * o);
                        
                        float2 d = g - f + o;
                        float t = dot(d, d);
                        t = pow(t, v * 0.8 + 0.2);
                        r = min(r, t);
                    }
                }
                return saturate(sqrt(r));
            }

            float generateLavaDisplacement(float2 uv, float time)
            {
                float2 noiseUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float noise = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, noiseUV + float2(time * 0.1, time * 0.05), 0).r;
                
                float2 voronoiUV = uv * _HotspotFrequency;
                float voronoiNoise = voronoi(voronoiUV, _HotspotSpeed, 0.7);
                
                float displacement = noise * 0.6 + (1.0 - voronoiNoise) * 0.4;
                
                float edgeFactor = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                edgeFactor = smoothstep(0.0, 0.2, edgeFactor);
                
                displacement = smoothstep(0.0, 1.0, displacement) * _DisplacementAmount * 0.5 * edgeFactor;
                
                return displacement;
            }

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float time = _Time.y;
                float displacement = generateLavaDisplacement(input.uv, time);
                
                float3 positionOS = input.positionOS.xyz + input.normalOS * displacement;
                output.positionCS = TransformObjectToHClip(positionOS);
                
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            
            ENDHLSL
        }
    }
}