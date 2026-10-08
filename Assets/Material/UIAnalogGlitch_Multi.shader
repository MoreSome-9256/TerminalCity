Shader "Custom/UIAnalogGlitch_Multi"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ScanLineJitter ("ScanLineJitter", Vector) = (0.02,0.1,0,0)
        _VerticalJump ("VerticalJump", Vector) = (0.05,0,0,0)
        _HorizontalShake ("HorizontalShake", Float) = 0.02
        _ColorDrift ("ColorDrift", Vector) = (0.03,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
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
            float2 _ScanLineJitter;
            float2 _VerticalJump;
            float _HorizontalShake;
            float2 _ColorDrift;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float nrand(float x, float y)
            {
                return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float u = i.uv.x;
                float v = i.uv.y;

                float jitter = nrand(v, _Time.y) * 2 - 1;
                jitter *= step(_ScanLineJitter.y, abs(jitter)) * _ScanLineJitter.x;

                float jump = lerp(v, frac(v + _VerticalJump.y), _VerticalJump.x);
                float shake = (nrand(_Time.y, 2) - 0.5) * _HorizontalShake;
                float drift = sin(jump + _ColorDrift.y) * _ColorDrift.x;

                fixed4 src1 = tex2D(_MainTex, float2(u + jitter + shake, jump)) * i.color;
                fixed4 src2 = tex2D(_MainTex, float2(u + jitter + shake + drift, jump)) * i.color;

                return fixed4(src1.r, src2.g, src1.b, src1.a);
            }
            ENDCG
        }
    }
}