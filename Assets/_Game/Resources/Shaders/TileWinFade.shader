Shader "A Mothers Ocean/Tile Win Fade"
{
    Properties
    {
        _Opacity("Opacity", Range(0, 1)) = 1
        _BaseColor("Base Color", Color) = (0.1, 0.65, 0.85, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Opacity;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 lightDirection = normalize(half3(-0.35, 0.7, -0.6));
                half lightAmount = 0.62 + 0.38 * saturate(dot(normalize(input.normalWS), lightDirection));
                return half4(_BaseColor.rgb * lightAmount, _BaseColor.a * _Opacity);
            }
            ENDHLSL
        }
    }
}
