// Idle for the upright mother shark holding the level board on the Home page (one sprite).
// Only the shark moves: the tail swings around its base, the body breathes and the eye looks
// around. The board area above the shark is left untouched.
// UISpriteLocalMesh supplies the sprite-local position + phase (TEXCOORD1) and uv rect (TEXCOORD2);
// the canvas must enable the TexCoord1 and TexCoord2 additional shader channels.
Shader "A Mothers Ocean/UI Home Shark"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)

        [Header(Tail)]
        _TailPivot("Tail Pivot (uv)", Vector) = (0.34, 0.16, 0, 0)
        _TailReach("Tail Region (u max, v max)", Vector) = (0.4, 0.28, 0, 0)
        _TailAngle("Tail Swing (radians)", Range(0, 0.4)) = 0.16
        _TailSpeed("Tail Speed", Float) = 2.6

        [Header(Body)]
        _BreathCenter("Breath Center (uv)", Vector) = (0.56, 0.24, 0, 0)
        _BreathTop("Breath Fades Out At (v)", Range(0, 1)) = 0.5
        _BreathAmount("Breath Amount", Range(0, 0.05)) = 0.014
        _BreathSpeed("Breath Speed", Float) = 1.6

        [Header(Eye)]
        _EyeCenter("Eye Center (uv)", Vector) = (0.563, 0.517, 0, 0)
        _EyeRadius("Eye Radius (uv)", Vector) = (0.062, 0.037, 0, 0)
        _LookAmount("Look Amount (eye radius)", Range(0, 0.5)) = 0.2
        _LookInterval("Look Interval (s)", Float) = 1.9

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 localAndPhase : TEXCOORD1;
                float4 spriteRect : TEXCOORD2;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 localAndPhase : TEXCOORD0;
                float4 spriteRect : TEXCOORD1;
                float4 worldPosition : TEXCOORD2;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float4 _TailPivot;
            float4 _TailReach;
            float _TailAngle;
            float _TailSpeed;
            float4 _BreathCenter;
            float _BreathTop;
            float _BreathAmount;
            float _BreathSpeed;
            float4 _EyeCenter;
            float4 _EyeRadius;
            float _LookAmount;
            float _LookInterval;

            v2f vert(appdata input)
            {
                v2f output;
                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.color = input.color * _Color;
                output.localAndPhase = input.localAndPhase;
                output.spriteRect = input.spriteRect;
                return output;
            }

            float Hash(float n)
            {
                return frac(sin(n * 12.9898) * 43758.5453);
            }

            // Rotates the tail region around its base; the swing grows toward the tail tip.
            float2 Tail(float2 local, float time)
            {
                float region = (1.0 - smoothstep(_TailReach.x - 0.1, _TailReach.x, local.x))
                    * (1.0 - smoothstep(_TailReach.y - 0.08, _TailReach.y, local.y));
                float2 offset = local - _TailPivot.xy;
                float reach = saturate(length(offset) / 0.3);
                float angle = sin(time * _TailSpeed) * _TailAngle * region * reach;
                float s = sin(-angle);
                float c = cos(-angle);
                return _TailPivot.xy + float2(offset.x * c - offset.y * s, offset.x * s + offset.y * c);
            }

            // Gentle swelling of the lower body, fading out before the board.
            float2 Breath(float2 local, float time)
            {
                float body = 1.0 - smoothstep(_BreathTop - 0.12, _BreathTop, local.y);
                float breath = sin(time * _BreathSpeed) * _BreathAmount * body;
                return _BreathCenter.xy + (local - _BreathCenter.xy) / (1.0 + breath);
            }

            // Shifts the iris toward a new random direction every _LookInterval seconds.
            float2 Eye(float2 local, float time)
            {
                float slot = floor(time / _LookInterval);
                float blend = smoothstep(0.85, 1.0, frac(time / _LookInterval));
                float2 current = float2(Hash(slot) * 2.0 - 1.0, (Hash(slot + 7.1) * 2.0 - 1.0) * 0.5);
                float2 next = float2(Hash(slot + 1.0) * 2.0 - 1.0, (Hash(slot + 8.1) * 2.0 - 1.0) * 0.5);
                float2 look = lerp(current, next, blend) * _LookAmount;
                float distanceToEye = length((local - _EyeCenter.xy) / _EyeRadius.xy);
                return local - look * _EyeRadius.xy * (1.0 - smoothstep(0.5, 0.95, distanceToEye));
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float time = _Time.y + input.localAndPhase.z;
                float2 local = input.localAndPhase.xy;
                local = Tail(local, time);
                local = Breath(local, time);
                local = Eye(local, time);
                float4 rect = input.spriteRect;
                float2 inside = step(0.0, local) * step(local, 1.0);
                fixed4 color = input.color * tex2D(_MainTex, rect.xy + saturate(local) * rect.zw);
                color.a *= inside.x * inside.y;
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
