Shader "Custom/Ultraviolet"
{
    Properties
    {
        _Color("Color Tint",Color)=(1,1,1,1)
        _BaseTex("Base Texture", 2D) = "white" {}
        _BaseTex_STCustom("Base Tex Tiling/Offset", Vector) = (1,1,0,0)

        _DrawTex("Drawing Texture", 2D) = "white" {}
        _DrawTex_STCustom("Draw Tex Tiling/Offset", Vector) = (1,1,0,0)
        _DrawRotation("Draw Rotation", Range(0,360)) = 0

        _NormalMap("Normal Map", 2D) = "bump" {}
        _EmissionStrength("Emission Strength", Range(0,5)) = 1
    }

    SubShader
    {
        //Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Tags { "Queue" = "Geometry-10"  "IgnoreProjector"="true" "RenderType"="Opaque" "ForceNoShadowCasting"="True"}
        
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _BaseTex;  fixed4 _Color;
        float4 _BaseTex_STCustom;

        sampler2D _DrawTex;
        float4 _DrawTex_STCustom;  // ✅ 自定义命名，彻底避免冲突
        float _DrawRotation;

        sampler2D _NormalMap;
        half _EmissionStrength;

        struct Input
        {
            float2 uv_BaseTex;
            float2 uv_DrawTex;
            float3 worldPos;
            float3 viewDir;
        };

        // 自定义旋转UV
        inline float2 RotateUV(float2 uv, float angle)
        {
            float rad = radians(angle);
            float s = sin(rad);
            float c = cos(rad);
            uv -= 0.5;
            float2x2 rot = float2x2(c, -s, s, c);
            uv = mul(rot, uv);
            uv += 0.5;
            return uv;
        }

        // surf 函数
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // BaseTex UV
            float2 baseUV = IN.uv_BaseTex * _BaseTex_STCustom.xy + _BaseTex_STCustom.zw;
            fixed4 baseCol = _Color*tex2D(_BaseTex, baseUV);

            // DrawTex UV + 旋转 + ST
            float2 drawUV = RotateUV(IN.uv_DrawTex, _DrawRotation);
            drawUV = drawUV * _DrawTex_STCustom.xy + _DrawTex_STCustom.zw;
            fixed4 drawCol = tex2D(_DrawTex, drawUV);

            // ---- 颜色叠加到 Albedo ----
            o.Albedo = baseCol.rgb + drawCol.rgb * drawCol.a;  // ✅ DrawTex 叠加到 BaseTex
            o.Metallic = 0.0;
            o.Smoothness = 0.3;

            // ---- 法线 ----
            o.Normal = UnpackNormal(tex2D(_NormalMap, baseUV));

            // ---- Emission（可选：聚光灯下自发光）----
            float visibility = step(0.1, saturate(dot(o.Normal, normalize(_WorldSpaceLightPos0.xyz))));
            o.Emission = drawCol.rgb * drawCol.a * _EmissionStrength * visibility;
        }

        ENDCG
    }

    FallBack "Diffuse"
}
