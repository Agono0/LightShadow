// LIGHT SHADOW - The Shadow's smoke silhouette.
// A hand-written URP shader (code, not a hand-edited asset): the humanoid
// under-mesh reads as a body of dark smoke with a soft fresnel rim and a
// gentle vertex wobble, so the outline breathes instead of sitting still.
// Deliberately unlit apart from the rim: in this game the Shadow is mostly
// seen as a blacker-than-dark shape between the player and a lit doorway.
Shader "LightShadow/ShadowSmoke"
{
    Properties
    {
        // Body colour: near-black with a hint of blue so it never clips
        // against the darkest wall.
        _BaseColor("Smoke Color", Color) = (0.012, 0.010, 0.022, 0.88)
        _EdgeColor("Rim Smoke Color", Color) = (0.16, 0.18, 0.24, 0.95)

        // Fresnel rim: power higher = tighter edge line.
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.2
        _RimStrength("Rim Strength", Range(0.0, 2.0)) = 0.9

        // Vertex wobble (amplitude and speed in metres / Hz-ish units).
        _WobbleAmplitude("Wobble Amplitude", Range(0.0, 0.3)) = 0.05
        _WobbleFrequency("Wobble Frequency", Range(0.0, 12.0)) = 2.6
        _WobbleSpeed("Wobble Speed", Range(0.0, 6.0)) = 1.4

        // Slow alpha breathing so the whole silhouette swells subtly.
        _AlphaPulse("Alpha Pulse", Range(0.0, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Fog support so the smoke fades with the scene's exponential fog.
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EdgeColor;
                half _RimPower;
                half _RimStrength;
                half _WobbleAmplitude;
                half _WobbleFrequency;
                half _WobbleSpeed;
                half _AlphaPulse;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                // xy = fog factor, z = wobble phase seed from object height.
                float3 misc : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float3 positionOS = input.positionOS.xyz;

                // Soft vertex wobble: two out-of-phase sine waves driven by the
                // object-space position. Keeps the silhouette from feeling like
                // a rigid statue while staying cheap (no noise textures).
                float phase = _Time.y * _WobbleSpeed;
                float wobbleX = sin(positionOS.y * _WobbleFrequency + phase) * _WobbleAmplitude;
                float wobbleZ = sin(positionOS.x * _WobbleFrequency * 0.8 + phase * 1.7 + positionOS.y * 2.0)
                    * _WobbleAmplitude * 0.7;
                positionOS.xz += float2(wobbleX, wobbleZ);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                float fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.misc.x = fogFactor;
                // Per-vertex phase offset from height makes the pulse travel
                // upward instead of flashing all at once.
                output.misc.z = positionOS.y;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 normalWS = normalize(input.normalWS);

                // Rim/fresnel: facing-away edges light up with the smoke grey.
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirectionWS)), _RimPower);

                // Alpha breathing tied to world time plus height phase.
                float pulse = 0.5 + 0.5 * sin(_Time.y * 0.9 + input.misc.z * 1.8);
                half alpha = _BaseColor.a * (1.0 - _AlphaPulse * pulse * 0.5);

                half3 color = lerp(_BaseColor.rgb, _EdgeColor.rgb, saturate(fresnel * _RimStrength));
                alpha = lerp(alpha, _EdgeColor.a, saturate(fresnel * _RimStrength * 0.8));

                // Match the scene fog so the silhouette sinks into the dark.
                float fogFactor = input.misc.x;
                color = MixFog(color, fogFactor);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Unlit"
}
