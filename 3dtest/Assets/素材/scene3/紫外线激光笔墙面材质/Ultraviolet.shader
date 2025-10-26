Shader "Custom/Ultraviolet"
{
    Properties
    {
        _Color("Color Tint", Color) = (1,1,1,1)
        _BaseTex("Base Texture", 2D) = "white" {}
        _BaseTex_STCustom("Base Tex Tiling/Offset", Vector) = (1,1,0,0)

        _DrawTex("Drawing Texture", 2D) = "white" {}
        _DrawTex_STCustom("Draw Tex Tiling/Offset", Vector) = (1,1,0,0)
        _DrawRotation("Draw Rotation", Range(0,360)) = 0

        _NormalMap("Normal Map", 2D) = "bump" {}
        _EmissionStrength("Emission Strength", Range(0,5)) = 1

        // 手电筒参数
        _FlashPos("Flash Position", Vector) = (0,0,0,0)
        _FlashDir("Flash Direction", Vector) = (0,0,1,0)
        _FlashAngle("Flash Spot Angle", Float) = 30
        _FlashRange("Flash Range", Float) = 5
        _MinVisibility("Min Visibility", Float) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry-10"
            "IgnoreProjector" = "true"
            "RenderType" = "Opaque"
            "ForceNoShadowCasting" = "True"
        }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _BaseTex;  fixed4 _Color;
        float4 _BaseTex_STCustom;

        sampler2D _DrawTex; float4 _DrawTex_STCustom;
        float _DrawRotation;

        sampler2D _NormalMap; half _EmissionStrength;

        float3 _FlashPos;
        float3 _FlashDir;
        float _FlashAngle;
        float _FlashRange;
        float _MinVisibility;

        struct Input
        {
            float2 uv_BaseTex;
            float2 uv_DrawTex;
            float3 worldPos;
            float3 viewDir;
            INTERNAL_DATA
        };

        // UV旋转
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

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // BaseTex
            float2 baseUV = IN.uv_BaseTex * _BaseTex_STCustom.xy + _BaseTex_STCustom.zw;
            fixed4 baseCol = tex2D(_BaseTex, baseUV) * _Color;

            // DrawTex
            float2 drawUV = RotateUV(IN.uv_DrawTex, _DrawRotation);
            drawUV = drawUV * _DrawTex_STCustom.xy + _DrawTex_STCustom.zw;
            fixed4 drawCol = tex2D(_DrawTex, drawUV);

            // 法线
            o.Normal = UnpackNormal(tex2D(_NormalMap, baseUV));

            // 手电筒方向和距离衰减
            float3 toFlash = _FlashPos - IN.worldPos;
            float dist = length(toFlash);
            float3 dir = normalize(toFlash);

            // 光照角度衰减
            float spotDot = dot(dir, normalize(-_FlashDir));
            float cosHalfAngle = cos(radians(_FlashAngle * 0.5));
            float angleAtten = saturate((spotDot - cosHalfAngle) / (1.0 - cosHalfAngle));

            // 光照距离衰减
            float rangeAtten = saturate(1.0 - dist / _FlashRange);

            // 总可见度
            float visibility = angleAtten * rangeAtten;
            visibility = visibility >= _MinVisibility ? visibility : 0.0;

            // 输出颜色
            o.Albedo = baseCol.rgb + drawCol.rgb * drawCol.a * visibility;
            o.Metallic = 0.0;
            o.Smoothness = 0.3;

            // 发光
            o.Emission = drawCol.rgb * drawCol.a * _EmissionStrength * visibility;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
