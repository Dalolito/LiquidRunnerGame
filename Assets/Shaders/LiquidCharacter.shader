Shader "Custom/LiquidCharacter"
{
    Properties
    {
        _BaseColor("Color", Color) = (0, 0.3, 1, 0.8)
        _RimColor("Rim Color", Color) = (0, 0.5, 1, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
        _WobbleAmount("Wobble Amount", Range(0, 0.1)) = 0.03
        _WobbleSpeed("Wobble Speed", Range(0, 10)) = 1
        _Smoothness("Smoothness", Range(0, 1)) = 0.9
        _Metallic("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        // Para renderizar correctamente objetos transparentes en URP
        // Primera pasada - escribir en el buffer de profundidad
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 position    : POSITION;
                float3 normal      : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _WobbleAmount;
            float _WobbleSpeed;

            // Función para generar ondulación
            float Wobble(float3 pos, float time)
            {
                return sin(pos.x * 8.0 + time) * cos(pos.z * 8.0 + time) * sin(pos.y * 6.0 + time) * _WobbleAmount;
            }

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Aplicar efecto de ondulación
                float time = _Time.y * _WobbleSpeed;
                float wobbleOffset = Wobble(input.position.xyz, time);
                float3 worldPos = input.position.xyz + input.normal * wobbleOffset;
                
                output.positionCS = TransformObjectToHClip(worldPos);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

        // Segunda pasada - renderizado principal
        Pass
        {
            Name "LitForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS    : SV_POSITION;
                float3 positionWS    : TEXCOORD0;
                float3 normalWS      : TEXCOORD1;
                float3 viewDirWS     : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RimColor;
                half _RimPower;
                half _WobbleAmount;
                half _WobbleSpeed;
                half _Smoothness;
                half _Metallic;
            CBUFFER_END

            // Función para generar ondulación
            float Wobble(float3 pos, float time)
            {
                return sin(pos.x * 8.0 + time) * cos(pos.z * 8.0 + time) * sin(pos.y * 6.0 + time) * _WobbleAmount;
            }

            Varyings LitVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Aplicar efecto de ondulación al vértice
                float time = _Time.y * _WobbleSpeed;
                float wobbleOffset = Wobble(input.positionOS.xyz, time);
                float3 positionOS = input.positionOS.xyz + input.normalOS * wobbleOffset;
                
                // Transformar posición a espacio de clip
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                
                // Calcular dirección de vista en espacio world
                output.viewDirWS = GetWorldSpaceViewDir(output.positionWS);
                
                return output;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Normalizar vectores
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Preparar la estructura de SurfaceData
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = _BaseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0, 0, 1);
                surfaceData.emission = float3(0, 0, 0);
                surfaceData.occlusion = 1;
                surfaceData.alpha = _BaseColor.a;
                
                // Preparar la estructura de InputData
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                // Calcular efecto de borde (rim)
                half rim = 1.0 - saturate(dot(viewDirWS, normalWS));
                half3 rimColor = _RimColor.rgb * pow(rim, _RimPower);
                
                // Añadir ondulación adicional en la superficie
                float time = _Time.y * _WobbleSpeed * 0.5;
                float waveX = sin(input.positionWS.x * 3.0 + time) * 0.03;
                float waveZ = cos(input.positionWS.z * 3.0 + time) * 0.03;
                
                // Añadir variación de color basada en la ondulación
                surfaceData.albedo += float3(waveX, waveX + waveZ, waveZ) * 0.3;
                
                // Añadir emisión para el efecto de borde
                surfaceData.emission = rimColor;
                
                // Aumentar la opacidad en los bordes
                surfaceData.alpha += pow(rim, _RimPower) * 0.3;
                surfaceData.alpha = saturate(surfaceData.alpha); // Asegurar que esté en el rango [0, 1]
                
                // Calcular iluminación usando el PBR de URP
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                return color;
            }
            ENDHLSL
        }
        
        // Shadow casting pass
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}