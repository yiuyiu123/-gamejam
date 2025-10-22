Shader "Custom/Ultraviolet"
{
    Properties
    {
        _BaseTex("Base Texture", 2D) = "white" {}
        _DrawTex("Drawing Texture", 2D) = "white" {}
        _DrawRotation("Draw Rotation", Range(0,360)) = 0

        _NormalMap("Normal Map", 2D) = "bump" {}
        _EmissionStrength("Emission Strength", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _BaseTex;
        float4 _BaseTex_ST;

        sampler2D _DrawTex;
        float4 _DrawTex_ST;
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

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 基础颜色
            float2 baseUV = TRANSFORM_TEX(IN.uv_BaseTex, _BaseTex);
            fixed4 baseCol = tex2D(_BaseTex, baseUV);

            // 法线贴图
            o.Normal = UnpackNormal(tex2D(_NormalMap, baseUV));

            // 绘制贴图UV + 旋转 + ST
            float2 drawUV = RotateUV(IN.uv_DrawTex, _DrawRotation);
            drawUV = drawUV * _DrawTex_ST.xy + _DrawTex_ST.zw;
            fixed4 drawCol = tex2D(_DrawTex, drawUV);

            // 基础颜色输出
            o.Albedo = baseCol.rgb;
            o.Metallic = 0.0;
            o.Smoothness = 0.3;

            // ---- 计算聚光灯影响 ----
            float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
            float3 toLight = lightDir;
            float3 normal = normalize(o.Normal);

            // 光照衰减（简单计算）
            half nl = saturate(dot(normal, toLight));

            // 假设聚光灯在主光源方向上：光强作为是否可见依据
            float visibility = step(0.1, nl);

            // ---- 涂鸦仅在聚光灯范围内显示 ----
            fixed3 graffitiColor = drawCol.rgb * drawCol.a * visibility;

            // ---- 聚光灯颜色控制自发光 ----
            fixed3 lightColor = unity_LightColor0.rgb;
            fixed3 emissive = graffitiColor * lightColor * _EmissionStrength;

            o.Emission = emissive;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
