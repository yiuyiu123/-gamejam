Shader "Custom/ColorTint"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _RedOffset ("Red Offset", Float) = 0
        _GreenOffset ("Green Offset", Float) = 0
        _BlueOffset ("Blue Offset", Float) = 0
        _Brightness ("Brightness", Float) = 0
        _Contrast ("Contrast", Float) = 0
        _Saturation ("Saturation", Float) = 0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;
            float _RedOffset;
            float _GreenOffset;
            float _BlueOffset;
            float _Brightness;
            float _Contrast;
            float _Saturation;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // --- 通道偏移 ---
                col.r += _RedOffset / 255.0;
                col.g += _GreenOffset / 255.0;
                col.b += _BlueOffset / 255.0;

                // --- 颜色乘因子 ---
                col.rgb *= _TintColor.rgb;

                // --- 亮度 ---
                col.rgb += _Brightness / 50.0; // 映射到 -1 ~ +1

                // --- 对比度 ---
                float contrastFactor = 1.0 + (_Contrast / 50.0); // -50~50 → 0~2
                col.rgb = (col.rgb - 0.5) * contrastFactor + 0.5;

                // --- 饱和度 ---
                float3 gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                float satFactor = 1.0 + (_Saturation / 50.0); // -50~50 → 0~2
                col.rgb = lerp(gray.xxx, col.rgb, satFactor);

                col.a = 1.0;
                return col;
            }
            ENDCG
        }
    }
}
