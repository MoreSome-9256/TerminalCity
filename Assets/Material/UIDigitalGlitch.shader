Shader "Custom/UIDigitalGlitch"
{
    Properties
    {
        _MainTex  ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _TrashTex ("Trash Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _TrashTex;
            float _Intensity;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 glitch = tex2D(_NoiseTex, i.uv);

                float thresh = 1.001 - _Intensity * 1.001;
                float w_d = step(thresh, pow(glitch.z, 2.5)); // displacement glitch
                float w_f = step(thresh, pow(glitch.w, 2.5)); // frame glitch
                float w_c = step(thresh, pow(glitch.z, 3.5)); // color glitch

                // Displacement
                float2 uv = i.uv + glitch.xy * w_d;
                uv = saturate(uv); // È·±£ UV ÔÚ [0,1]
                float4 source = tex2D(_MainTex, uv);

                // Mix with trash frame
                float3 color = lerp(source.rgb, tex2D(_TrashTex, uv).rgb, w_f);

                // Shuffle color components
                float3 neg = saturate(color.grb + (1 - dot(color, 1)) * 0.5);
                color = lerp(color, neg, w_c);

                return float4(color, source.a);
            }
            ENDCG
        }
    }
}