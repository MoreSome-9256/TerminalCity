// 创建 LightOverlay.shader
Shader "UI/LightOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightTex ("Light Texture", 2D) = "white" {}
        _LightPos ("Light Position", Vector) = (0.5,0.5,0,0)
        _LightRange ("Light Range", Range(0,1)) = 0.5
        _Intensity ("Intensity", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend One One // Additive Blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _LightTex;
            float4 _LightPos;
            float _LightRange;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算光照衰减
                float2 lightUV = (i.uv - _LightPos.xy) * 2.0;
                float distance = length(lightUV);
                float attenuation = saturate(1.0 - distance / _LightRange);
                
                // 混合纹理
                fixed4 lightCol = tex2D(_LightTex, i.uv);
                fixed4 texCol = tex2D(_MainTex, i.uv);
                
                return lightCol * attenuation * _Intensity + texCol;
            }
            ENDCG
        }
    }
}