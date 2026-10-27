Shader "Custom/RoomArtifactGlitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Glitch Intensity", Range(0, 1)) = 0.0
        _ChromaticDisp ("Color Drift (RGB Split)", Range(0, 0.08)) = 0.02
        _ArtifactBlockSize ("Artifact Block Scale", Float) = 15.0
        _NoiseSpeed ("Noise Speed", Float) = 8.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Intensity;
            float _ChromaticDisp;
            float _ArtifactBlockSize;
            float _NoiseSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            // 伪随机生成器
            float hash21(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                if (_Intensity <= 0.001)
                {
                    return tex2D(_MainTex, uv) * i.color;
                }

                // 1. 生成基于行的高频离散块（Block / Artifact Glitch）
                float timeStep = floor(_Time.y * _NoiseSpeed);
                float row = floor(uv.y * _ArtifactBlockSize);
                float noise = hash21(float2(row, timeStep));

                // 只有当随机值满足阈值时产生横向切片漂移
                float artifactOffset = 0.0;
                if (noise < _Intensity * 0.45)
                {
                    artifactOffset = (hash21(float2(row * 1.5, timeStep)) - 0.5) * _Intensity * 0.08;
                }

                float2 sampleUV = float2(frac(uv.x + artifactOffset), uv.y);

                // 2. RGB 色散分离（零晃动，仅通道水平分离）
                float split = _ChromaticDisp * _Intensity;
                
                // 红色通道向左偏移采样，蓝色通道向右偏移采样，绿色通道居中
                float r = tex2D(_MainTex, float2(sampleUV.x - split, sampleUV.y)).r;
                float g = tex2D(_MainTex, sampleUV).g;
                float b = tex2D(_MainTex, float2(sampleUV.x + split, sampleUV.y)).b;
                float a = tex2D(_MainTex, sampleUV).a;

                // 3. 伪影雪花条带噪点（微弱叠加在亮部）
                float scanlineGlitch = hash21(float2(uv.y * 120.0, _Time.y * 30.0));
                if (scanlineGlitch < _Intensity * 0.15)
                {
                    float noiseBar = (hash21(float2(uv.x * 20.0, _Time.y)) - 0.5) * 0.15;
                    r += noiseBar;
                    g += noiseBar;
                    b += noiseBar;
                }

                return fixed4(r, g, b, a) * i.color;
            }
            ENDCG
        }
    }
}