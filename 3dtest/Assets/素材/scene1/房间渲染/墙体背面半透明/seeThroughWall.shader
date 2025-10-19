Shader "Unlit/seeThroughWall"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "ForceNoShadowCasting"="True" }
        LOD 200

        Cull Back            // ֻ��Ⱦ���棨����˫�棬�ĳ� Cull Off��
        ZWrite Off            // ��д����Ȼ��棨�����⡢�������ߣ�
        Blend SrcAlpha OneMinusSrcAlpha  // ��ͨ͸�����

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;

                // �򵥵� Alpha Cutoff����ͼ͸����̫��������
                if (tex.a < _Cutoff) discard;

                return tex;
            }
            ENDCG
        }
    }
    FallBack Off
}
