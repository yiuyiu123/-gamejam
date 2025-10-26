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

        _DrawTexVisibility("DrawTex Visibility", Range(0,1)) = 0
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
        float _DrawTexVisibility;

        struct Input
        {
            float2 uv_BaseTex;
            float2 uv_DrawTex;
            INTERNAL_DATA
        };

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

            // 用C#传的显隐值控制
            float visibility = saturate(_DrawTexVisibility);

            o.Albedo = baseCol.rgb + drawCol.rgb * visibility;
            o.Metallic = 0.0;
            o.Smoothness = 0.3;

            o.Emission = drawCol.rgb * _EmissionStrength * visibility;
        }

        ENDCG
    }

    FallBack "Diffuse"
}