// Gameplay (SpriteRenderer) version of the fish idle; the motion lives in FishIdleCommon.hlsl.
// _SpriteRect (uv x, y, width, height) and _Phase are set per renderer by SpriteIdleWave.
Shader "A Mothers Ocean/Sprite Fish Idle"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _SwimSpeed("Swim Speed", Float) = 3.2
        _WaveLength("Wave Length", Float) = 5.5
        _TailStart("Tail Start (u)", Range(0.05, 1)) = 0.5
        _TailAmplitude("Tail Amplitude", Range(0, 0.1)) = 0.035
        _TailSquash("Tail Squash", Range(0, 0.1)) = 0.012
        _BodyAmplitude("Body Amplitude", Range(0, 0.03)) = 0.006
        _BreathAmount("Breath Amount", Range(0, 0.05)) = 0.012
        _BreathCenter("Breath Center (uv)", Vector) = (0.6, 0.5, 0, 0)

        [Header(Eyes)]
        _EyeCenter("Eye Center (uv)", Vector) = (0.775, 0.511, 0, 0)
        _EyeRadius("Eye Radius (uv)", Vector) = (0.043, 0.063, 0, 0)
        _Eye2Center("Second Eye Center (uv, z = enabled)", Vector) = (0, 0, 0, 0)
        _Eye2Radius("Second Eye Radius (uv)", Vector) = (0.03, 0.06, 0, 0)
        _LookAmount("Look Amount (eye radius)", Range(0, 0.5)) = 0.22
        _LookInterval("Look Interval (s)", Float) = 1.8

        [HideInInspector] _SpriteRect("Sprite Rect", Vector) = (0, 0, 1, 1)
        [HideInInspector] _Phase("Phase", Float) = 0
        [HideInInspector] _Color("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _SpriteRect;
                float4 _BreathCenter;
                float _Phase;
                float _SwimSpeed;
                float _WaveLength;
                float _TailStart;
                float _TailAmplitude;
                float _TailSquash;
                float _BodyAmplitude;
                float _BreathAmount;
                float4 _EyeCenter;
                float4 _EyeRadius;
                float4 _Eye2Center;
                float4 _Eye2Radius;
                float _LookAmount;
                float _LookInterval;
            CBUFFER_END

            #include "FishIdleCommon.hlsl"

            Varyings Vert(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 local = (input.uv - _SpriteRect.xy) / _SpriteRect.zw;
                float2 sampleLocal = FishIdleSample(local, _Phase);
                float2 inside = step(0.0, sampleLocal) * step(sampleLocal, 1.0);
                float2 uv = _SpriteRect.xy + saturate(sampleLocal) * _SpriteRect.zw;
                half4 color = input.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                color.a *= inside.x * inside.y;
                return color;
            }
            ENDHLSL
        }
    }
}
