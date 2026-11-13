// Idle motion for special board tiles, so the board feels underwater instead of static.
// _Mode picks the tile-specific effect:
//   0 = none, 1 = whirlpool spin (Rotate), 2 = current flowing along the pipe,
//   3 = seaweed sway (Block), 4 = shimmering light beam with rising sparkles (Finish).
// Sprite colour (used by the win/lose fades) is respected through the vertex colour.
Shader "A Mothers Ocean/Tile Idle"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        [Enum(None,0,Whirlpool,1,Pipe Flow,2,Seaweed,3,Light Beam,4)] _Mode("Effect", Float) = 0

        [Header(Whirlpool)]
        _SwirlCenter("Swirl Center (uv)", Vector) = (0.5, 0.51, 0, 0)
        _SwirlRadius("Swirl Radius (uv)", Range(0.05, 0.5)) = 0.36
        _SwirlSpeed("Swirl Speed (deg per second)", Float) = 70

        [Header(Pipe Flow)]
        _FlowDirection("Flow Direction (uv)", Vector) = (1, 0, 0, 0)
        _FlowColor("Flow Color", Color) = (0.85, 1, 1, 1)
        _FlowStrength("Flow Strength", Range(0, 2)) = 0.9
        _FlowWidth("Flow Pulse Width", Range(0.02, 0.5)) = 0.14
        _FlowSpeed("Flow Speed", Float) = 0.7
        _FlowMaskStart("Pipe Brightness Threshold", Range(0, 1)) = 0.62

        [Header(Seaweed)]
        _SwayAmount("Sway Amount (uv)", Range(0, 0.05)) = 0.018
        _SwaySpeed("Sway Speed", Float) = 2.2
        _SwayBottom("Sway Bottom (v)", Range(0, 1)) = 0.18
        _SwayTop("Sway Top (v)", Range(0, 1)) = 0.46

        [Header(Light Beam)]
        _BeamCenter("Beam Center (u)", Range(0, 1)) = 0.5
        _BeamWidth("Beam Half Width (u)", Range(0.01, 0.4)) = 0.12
        _BeamStrength("Beam Shimmer Strength", Range(0, 2)) = 0.6
        _BeamSpeed("Beam Speed", Float) = 1.2

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
                float _Mode;
                float4 _SwirlCenter;
                float _SwirlRadius;
                float _SwirlSpeed;
                float4 _FlowDirection;
                half4 _FlowColor;
                float _FlowStrength;
                float _FlowWidth;
                float _FlowSpeed;
                float _FlowMaskStart;
                float _SwayAmount;
                float _SwaySpeed;
                float _SwayBottom;
                float _SwayTop;
                float _BeamCenter;
                float _BeamWidth;
                float _BeamStrength;
                float _BeamSpeed;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            float4 Sample(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }

            float Luminance01(float3 rgb)
            {
                return dot(rgb, float3(0.299, 0.587, 0.114));
            }

            // Spins the whirlpool disk as a whole; the ring near the edge blends back to the
            // still frame so nothing tears.
            float4 Whirlpool(float2 uv)
            {
                float2 offset = uv - _SwirlCenter.xy;
                float distanceFromCenter = length(offset);
                float angle = radians(_Time.y * _SwirlSpeed);
                float s = sin(angle);
                float c = cos(angle);
                float2 rotated = _SwirlCenter.xy + float2(offset.x * c - offset.y * s, offset.x * s + offset.y * c);
                float inside = 1.0 - smoothstep(_SwirlRadius * 0.82, _SwirlRadius, distanceFromCenter);
                return lerp(Sample(uv), Sample(rotated), inside);
            }

            // A soft pulse of light travelling along the bright pipe pixels.
            float3 PipeFlow(float2 uv, float4 color)
            {
                float2 direction = normalize(_FlowDirection.xy);
                float along = dot(uv - 0.5, direction) + 0.5;
                float phase = frac(_Time.y * _FlowSpeed);
                float travel = lerp(-_FlowWidth, 1.0 + _FlowWidth, phase);
                float pulse = 1.0 - smoothstep(0.0, _FlowWidth, abs(along - travel));
                float pipe = smoothstep(_FlowMaskStart, _FlowMaskStart + 0.2, Luminance01(color.rgb));
                // Keep the light off the tile's own neon frame, which is just as bright.
                float2 edge = min(uv, 1.0 - uv);
                pipe *= smoothstep(0.1, 0.14, min(edge.x, edge.y));
                float breathe = 0.12 + 0.08 * sin(_Time.y * 2.0);
                return _FlowColor.rgb * pipe * (pulse * _FlowStrength + breathe);
            }

            bool IsSeaweed(float4 color)
            {
                return color.a > 0.5 && color.g > color.r + 0.08 && color.g > color.b + 0.04;
            }

            // Sways the lower band where seaweed grows; only seaweed pixels move.
            float4 Seaweed(float2 uv)
            {
                float4 still = Sample(uv);
                float height = saturate((uv.y - _SwayBottom) / max(0.001, _SwayTop - _SwayBottom));
                float band = step(_SwayBottom, uv.y) * step(uv.y, _SwayTop + 0.04);
                float sway = sin(_Time.y * _SwaySpeed + uv.x * 9.0) * _SwayAmount * height * band;
                float4 moved = Sample(uv - float2(sway, 0.0));
                return (IsSeaweed(still) || IsSeaweed(moved)) ? moved : still;
            }

            float Hash(float n)
            {
                return frac(sin(n * 12.9898) * 43758.5453);
            }

            // Streaks rising through the beam plus a few sparkles floating up.
            float3 LightBeam(float2 uv, float4 color)
            {
                float across = abs(uv.x - _BeamCenter) / _BeamWidth;
                float column = saturate(1.0 - across);
                float t = _Time.y * _BeamSpeed;
                float streaks = 0.5 + 0.5 * sin(uv.y * 18.0 - t * 3.0 + uv.x * 25.0);
                float glow = column * streaks * Luminance01(color.rgb);
                float sparkles = 0.0;
                for (int i = 0; i < 6; i++)
                {
                    float seed = i + 1.0;
                    float2 spot = float2(_BeamCenter + (Hash(seed) - 0.5) * _BeamWidth * 2.4,
                        frac(t * (0.25 + Hash(seed * 3.1) * 0.25) + Hash(seed * 7.7)));
                    float size = 0.012 + Hash(seed * 5.3) * 0.012;
                    sparkles += (1.0 - smoothstep(0.0, size, length(uv - spot))) * (1.0 - spot.y);
                }
                return float3(1.0, 0.98, 0.85) * (glow + sparkles) * _BeamStrength;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float4 color;
                if (_Mode > 0.5 && _Mode < 1.5) color = Whirlpool(uv);
                else if (_Mode > 2.5 && _Mode < 3.5) color = Seaweed(uv);
                else color = Sample(uv);

                if (_Mode > 1.5 && _Mode < 2.5) color.rgb += PipeFlow(uv, color);
                if (_Mode > 3.5) color.rgb += LightBeam(uv, color);
                return color * input.color;
            }
            ENDHLSL
        }
    }
}
