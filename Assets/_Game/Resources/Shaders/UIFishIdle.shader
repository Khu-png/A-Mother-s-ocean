// Menu (UI Image) version of the fish idle; the motion lives in FishIdleCommon.hlsl.
// UISpriteLocalMesh writes the sprite-local position + phase into TEXCOORD1 and the sprite's
// uv rect into TEXCOORD2, so every shark image can share one material.
// The canvas must enable the TexCoord1 and TexCoord2 additional shader channels.
Shader "A Mothers Ocean/UI Fish Idle"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _SwimSpeed("Swim Speed", Float) = 3.0
        _WaveLength("Wave Length", Float) = 5.5
        _TailStart("Tail Start (u)", Range(0.05, 1)) = 0.5
        _TailAmplitude("Tail Amplitude", Range(0, 0.1)) = 0.045
        _TailSquash("Tail Squash", Range(0, 0.1)) = 0.012
        _BodyAmplitude("Body Amplitude", Range(0, 0.03)) = 0.006
        _BreathAmount("Breath Amount", Range(0, 0.05)) = 0.012
        _BreathCenter("Breath Center (uv)", Vector) = (0.6, 0.5, 0, 0)

        [Header(Eyes)]
        _EyeCenter("Eye Center (uv)", Vector) = (0.775, 0.511, 0, 0)
        _EyeRadius("Eye Radius (uv)", Vector) = (0.049, 0.072, 0, 0)
        _Eye2Center("Second Eye Center (uv, z = enabled)", Vector) = (0, 0, 0, 0)
        _Eye2Radius("Second Eye Radius (uv)", Vector) = (0.03, 0.06, 0, 0)
        _LookAmount("Look Amount (eye radius)", Range(0, 0.5)) = 0.22
        _LookInterval("Look Interval (s)", Float) = 1.8

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
            float4 _BreathCenter;
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

            #include "FishIdleCommon.hlsl"

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

            fixed4 frag(v2f input) : SV_Target
            {
                float2 sampleLocal = FishIdleSample(input.localAndPhase.xy, input.localAndPhase.z);
                float4 rect = input.spriteRect;
                float2 inside = step(0.0, sampleLocal) * step(sampleLocal, 1.0);
                fixed4 color = input.color * tex2D(_MainTex, rect.xy + saturate(sampleLocal) * rect.zw);
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
