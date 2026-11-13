// A diagonal light band that sweeps across a UI sprite, then rests before the next pass.
// It is masked by the sprite's own alpha, so the shine never leaves the button's shape.
// UISpriteLocalMesh supplies the sprite-local position (TEXCOORD1) and uv rect (TEXCOORD2);
// the canvas must enable the TexCoord1 and TexCoord2 additional shader channels.
Shader "A Mothers Ocean/UI Shine Sweep"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _ShineColor("Shine Color", Color) = (1, 1, 0.9, 0.55)
        _Width("Band Width", Range(0.02, 0.5)) = 0.12
        _Slant("Band Slant", Range(0, 1)) = 0.35
        _SweepTime("Sweep Time (s)", Float) = 0.7
        _Period("Repeat Every (s)", Float) = 2.6

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
        Blend SrcAlpha One
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 localAndPhase : TEXCOORD1;
                float4 worldPosition : TEXCOORD2;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ShineColor;
            float4 _ClipRect;
            float _Width;
            float _Slant;
            float _SweepTime;
            float _Period;

            v2f vert(appdata input)
            {
                v2f output;
                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.color = input.color * _Color;
                output.texcoord = input.texcoord;
                output.localAndPhase = input.localAndPhase;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 local = input.localAndPhase.xy;
                float since = fmod(_Time.y + input.localAndPhase.z, _Period);
                float sweep = since / _SweepTime;
                float position = lerp(-_Width - _Slant, 1.0 + _Width, saturate(sweep));
                float distanceToBand = abs(local.x + local.y * _Slant - position - _Slant * 0.5);
                float band = (1.0 - smoothstep(0.0, _Width, distanceToBand)) * step(sweep, 1.0);
                float spriteAlpha = tex2D(_MainTex, input.texcoord).a * input.color.a;
                fixed4 color = fixed4(_ShineColor.rgb, _ShineColor.a * band * spriteAlpha);
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
