Shader "Custom/TopDownFogURP"
{
    Properties
    {
        _TorchMask("Torch Mask", 2D) = "black" {}
        _PlayerMask("Player Mask", 2D) = "black" {}
        _FogAlpha("Fog Alpha", Range(0, 1)) = 0.9
        _MapOrigin("Map Origin", Vector) = (0, 0, 0, 0)
        _MapSize("Map Size", Vector) = (80, 40, 0, 0)
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
                "RenderPipeline" = "UniversalPipeline"
            }

            Pass
            {
                Name "Forward"
                Tags { "LightMode" = "UniversalForward" }

                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off
                Cull Off

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                TEXTURE2D(_TorchMask);
                SAMPLER(sampler_TorchMask);

                TEXTURE2D(_PlayerMask);
                SAMPLER(sampler_PlayerMask);

                float4 _MapOrigin;
                float4 _MapSize;
                float _FogAlpha;

                struct Attributes
                {
                    float4 positionOS : POSITION;
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                    float3 worldPos : TEXCOORD0;
                };

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    VertexPositionInputs pos = GetVertexPositionInputs(
                        IN.positionOS.xyz);
                    OUT.positionHCS = pos.positionCS;
                    OUT.worldPos = pos.positionWS;
                    return OUT;
                }

                half4 frag(Varyings IN) : SV_Target
                {
                    float2 uv;
                    uv.x = (IN.worldPos.x - _MapOrigin.x) / _MapSize.x;
                    uv.y = (IN.worldPos.z - _MapOrigin.z) / _MapSize.y;

                    if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                        return half4(0, 0, 0, _FogAlpha);

                    float torchV = SAMPLE_TEXTURE2D(_TorchMask,
                        sampler_TorchMask, uv).r;
                    float playerV = SAMPLE_TEXTURE2D(_PlayerMask,
                        sampler_PlayerMask, uv).r;

                    float visible = max(torchV, playerV);
                    float alpha = (1.0 - visible) * _FogAlpha;

                    return half4(0, 0, 0, alpha);
                }
                ENDHLSL
            }
        }
}