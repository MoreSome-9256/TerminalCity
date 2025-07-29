//-----------------------------------------------【Shader说明】--------------------------------------------------------
//     Shader功能：   2D模糊 
//       核心思路：  在片源着色器里对单个图元累加周边的颜色然后再取平均，高斯模糊涉及到高斯公式(高斯正太分布公式)
//---------------------------------------------------------------------------------------------------------------------

Shader "2D模糊"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurRadius ("Blur Radius", Range(0, 15)) = 5
        _TextureSize ("Texture Size", Float) = 1024
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_horizontal
            
            #include "UnityCG.cginc"
            
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
            float _BlurRadius;
            float _TextureSize;
            static const float weights[9] = { 
    0.05, 0.09, 0.12, 
    0.15, 0.18, 0.15,
    0.12, 0.09, 0.05 
};

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag_horizontal (v2f i) : SV_Target
            {
                float2 texelSize = 1.0 / _TextureSize;
                fixed4 col = 0;
                float weightSum = 0;
                
                for(int x = -8; x <= 8; x++) // 固定17个采样点
                {
                    float weight = weights[abs(x)];
                    float2 offset = float2(x * texelSize.x * _BlurRadius/3, 0);
                    col += tex2D(_MainTex, i.uv + offset) * weight;
                    weightSum += weight;
                }
                return col / weightSum;
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_vertical
            #include "UnityCG.cginc"
            
            // 结构体和vert方法同上
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
            float _BlurRadius;
            float _TextureSize;
            static const float weights[9] = { 
    0.05, 0.09, 0.12, 
    0.15, 0.18, 0.15,
    0.12, 0.09, 0.05 
};

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag_vertical (v2f i) : SV_Target
            {
                float2 texelSize = 1.0 / _TextureSize;
                fixed4 col = 0;
                float weightSum = 0;
                
                for(int y = -8; y <= 8; y++)
                {
                    float weight = weights[abs(y)];
                    float2 offset = float2(0, y * texelSize.y * _BlurRadius/3);
                    col += tex2D(_MainTex, i.uv + offset) * weight;
                    weightSum += weight;
                }
                return col / weightSum;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}