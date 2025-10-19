Shader "Custom/WindowWall_Lit_ShadowCaster"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.3
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200
        Cull Off     // 双面显示窗户

        CGPROGRAM
        // ✅ 支持多光源、阴影、Alpha裁剪
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff addshadow

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // 基础颜色
            o.Albedo = c.rgb;
            // 使用透明度控制裁剪
            o.Alpha = c.a;
            // 金属/光滑参数
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG

        // ✅ 自定义阴影投射 pass（让窗户透明部分不挡光）
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutoff;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                // 🔸 透明部分不投影
                clip(tex.a - _Cutoff);
                return 0;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
