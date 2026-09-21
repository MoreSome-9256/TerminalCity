Shader "UI/HoloScanlineFill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)

        [Header(Scanline Settings)]
        _LineFrequency ("Line Density", Float) = 150.0
        _ScrollSpeed ("Scroll Speed", Float) = 2.0
        _MinLineAlpha ("Min Scanline Alpha", Range(0, 1)) = 0.35

        [Header(Flicker Settings)]
        _FlickerSpeed ("Flicker Speed", Float) = 30.0
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.5)) = 0.08
        _GlitchChance ("Glitch Threshold", Range(0.8, 1.0)) = 0.92
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _LineFrequency;
            float _ScrollSpeed;
            float _MinLineAlpha;
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _GlitchChance;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 采样填充图的 Alpha 与颜色
                fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 如果是透明区域直接剔除
                if (col.a <= 0.001) return fixed4(0, 0, 0, 0);

                // 2. 自下向上的滚动扫描线
                // uv.y 加上时间，产生从下往上滚动的正弦波条纹
                float scanline = sin((IN.texcoord.y + _Time.y * _ScrollSpeed) * _LineFrequency);
                scanline = lerp(_MinLineAlpha, 1.0, scanline * 0.5 + 0.5);

                // 3. 常态高频电流微颤
                float microFlicker = 1.0 + sin(_Time.y * _FlickerSpeed) * _FlickerIntensity;

                // 4. 阶梯状偶发故障跌落 (低频阶梯随机值)
                float timeStep = floor(_Time.y * 8.0); // 每秒判断 8 次
                float rand = frac(sin(timeStep * 12.9898) * 43758.5453);
                float glitch = (rand > _GlitchChance) ? 0.65 : 1.0;

                // 5. 组合亮度变化
                col.rgb *= scanline * microFlicker * glitch;
                
                return col;
            }
            ENDCG
        }
    }
}