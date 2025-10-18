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

                // ����Χӳ��Ϊ��ɫ����ϵ����1 ��ʾԭֵ����50 ��ʾ���� 0~3 ����
                float3 factor = 1 + float3(_RedOffset, _GreenOffset, _BlueOffset) / 50.0;

                // �Գ˷���ʽƫ��ɫ��ǿ�ȣ�����˲�䱥��
                col.rgb *= factor;

                // ȫ��ɫ�����
                col.rgb *= _TintColor.rgb;

                // ���ְ�ȫ��Χ
                col.rgb = clamp(col.rgb, 0.0, 1.0);

                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
