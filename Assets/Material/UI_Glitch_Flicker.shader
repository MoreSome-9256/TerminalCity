Shader "UI/Custom/GlitchFlicker"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Unique Instance Seed)]
        _Seed ("Unique Seed", Float) = 0.0 // 由脚本自动注入或手动设置，错开各窗口时间

        [Header(Flicker Settings)]
        _FlickerSpeed ("Flicker Speed", Float) = 25.0
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.08

        [Header(Glitch Settings)]
        _GlitchInterval ("Glitch Interval (Sec)", Float) = 3.0
        _GlitchDuration ("Glitch Duration (Sec)", Float) = 0.15
        _BlockSize ("Glitch Slice Count", Float) = 25.0
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

            float _Seed;
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

                // 整个窗口共用一个连续时间轴，加上该窗口专属的种子偏移
                float t = _Time.y + _Seed;

                // 1. 周期性爆发判定（错峰触发）
                float cycle = fmod(t, _GlitchInterval);
                float isGlitching = step(cycle, _GlitchDuration);

                // 2. 纯横向条纹故障（整行水平位移，绝无方块网格马赛克）
                if (isGlitching > 0.5)
                {
                    // 仅按 Y 轴切出横行切片
                    float sliceIndex = floor(uv.y * _BlockSize);
                    float rnd = Random(float2(sliceIndex, floor(t * 24.0)));

                    // 随机让某些横切条产生水平错位
                    if (rnd > 0.7)
                    {
                        float offset = (rnd - 0.85) * _DisplaceAmount;
                        uv.x += offset;
                    }
                }

                // 3. 色相横向分离 (RGB Split)
                float split = isGlitching * _RGBShift;
                fixed4 col;
                col.r = tex2D(_MainTex, uv + float2(split, 0)).r + _TextureSampleAdd.r;
                col.g = tex2D(_MainTex, uv).g + _TextureSampleAdd.g;
                col.b = tex2D(_MainTex, uv - float2(split, 0)).b + _TextureSampleAdd.b;
                col.a = tex2D(_MainTex, uv).a + _TextureSampleAdd.a;

                col *= IN.color;

                // 4. 高频轻微闪烁 (Flicker)
                float flicker = 1.0 - (Random(float2(floor(t * _FlickerSpeed), _Seed)) * _FlickerIntensity);
                col.rgb *= flicker;

                // 5. 纯横向 CRT 扫描线 (平整完整覆盖)
                float scanline = sin((uv.y + t * _ScanlineSpeed) * _ScanlineDensity);
                col.rgb -= scanline * _ScanlineAlpha;

                // UGUI 裁剪与 Alpha 处理
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