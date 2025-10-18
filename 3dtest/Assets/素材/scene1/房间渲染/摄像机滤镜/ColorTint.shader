Shader "Custom/ColorTint"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _RedOffset ("Red Offset", Range(-50,50)) = 0
        _GreenOffset ("Green Offset", Range(-50,50)) = 0
        _BlueOffset ("Blue Offset", Range(-50,50)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 将范围映射为颜色调整系数：1 表示原值，±50 表示倍数 0~3 区间
                float3 factor = 1 + float3(_RedOffset, _GreenOffset, _BlueOffset) / 50.0;

                // 以乘法方式偏移色彩强度，避免瞬间饱和
                col.rgb *= factor;

                // 全局色调混合
                col.rgb *= _TintColor.rgb;

                // 保持安全范围
                col.rgb = clamp(col.rgb, 0.0, 1.0);

                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
