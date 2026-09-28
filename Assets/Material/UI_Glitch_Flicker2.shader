Shader "UI/Custom/GlitchFlicker2"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Flicker Settings)]
        _FlickerSpeed ("Flicker Speed", Float) = 25.0
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.08

        [Header(Glitch Settings)]
        _GlitchInterval ("Glitch Interval (Sec)", Float) = 3.0
        _GlitchDuration ("Glitch Duration (Sec)", Float) = 0.15
        _BlockSize ("Glitch Block Size", Float) = 20.0
        _DisplaceAmount ("Displace Amount", Range(0, 0.1)) = 0.03
        _RGBShift ("RGB Split Intensity", Range(0, 0.05)) = 0.015

        [Header(Scanline Settings)]
        _ScanlineDensity ("Scanline Density", Float) = 150.0
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        _ScanlineAlpha ("Scanline Strength", Range(0, 0.5)) = 0.05

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _FlickerSpeed;
            float _FlickerIntensity;
            float _GlitchInterval;
            float _GlitchDuration;
            float _BlockSize;
            float _DisplaceAmount;
            float _RGBShift;
            float _ScanlineDensity;
            float _ScanlineSpeed;
            float _ScanlineAlpha;

            float Random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // 核心改动：基于物体在屏幕空间的位置，生成一个独一无二的时间偏移量
                // 取世界坐标的整数部分做 Hash，保证同一个窗口内部像素偏移一致，但不同窗口间完全不同
                float2 windowSeed = floor(IN.worldPosition.xy * 0.05);
                float timeOffset = Random(windowSeed) * 100.0;

                // 各物体独享自己的时间轴
                float t = _Time.y + timeOffset;

                // 1. 周期性故障判断（错开发生）
                float cycle = fmod(t, _GlitchInterval);
                float isGlitching = step(cycle, _GlitchDuration);

                // 2. 块状位移
                if (isGlitching > 0.5)
                {
                    float blockY = floor(uv.y * _BlockSize);
                    float seed = blockY + floor(t * 30.0) + windowSeed.x;
                    float rnd = Random(float2(seed, seed));

                    if (rnd > 0.65)
                    {
                        float offset = (rnd - 0.8) * _DisplaceAmount;
                        uv.x += offset;
                    }
                }

                // 3. 色相分离 (RGB Split)
                float split = isGlitching * _RGBShift;
                fixed4 col;
                col.r = tex2D(_MainTex, uv + float2(split, 0)).r + _TextureSampleAdd.r;
                col.g = tex2D(_MainTex, uv).g + _TextureSampleAdd.g;
                col.b = tex2D(_MainTex, uv - float2(split, 0)).b + _TextureSampleAdd.b;
                col.a = tex2D(_MainTex, uv).a + _TextureSampleAdd.a;

                col *= IN.color;

                // 4. 错开节奏的微闪烁 (Flicker)
                float flickerSeed = floor(t * _FlickerSpeed) + windowSeed.y;
                float flicker = 1.0 - (Random(float2(flickerSeed, 0.0)) * _FlickerIntensity);
                col.rgb *= flicker;

                // 5. 扫描线
                float scanline = sin((uv.y + t * _ScanlineSpeed) * _ScanlineDensity);
                col.rgb -= scanline * _ScanlineAlpha;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}