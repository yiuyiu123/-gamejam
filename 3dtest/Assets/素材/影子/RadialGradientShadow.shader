Shader "Custom/RadialGradientShadow"
{
    Properties
    {
        _CenterColor ("Center Color", Color) = (0,0,0,0.8)
        _EdgeColor ("Edge Color", Color) = (0,0,0,0)
        _Softness ("Softness", Float) = 2.0
        _Radius ("Radius", Range(0.1, 2.0)) = 1.0
        _AspectRatio ("Aspect Ratio", Range(0.1, 2.0)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            fixed4 _CenterColor;
            fixed4 _EdgeColor;
            float _Softness;
            float _Radius;
            float _AspectRatio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 调整UV坐标以适应宽高比
                float2 centeredUV = (i.uv - 0.5) * float2(_AspectRatio, 1.0);
                float distance = length(centeredUV);
                
                // 应用半径控制
                distance = distance / _Radius;
                
                // 创建平滑渐变
                float gradient = 1.0 - saturate(distance);
                gradient = pow(gradient, _Softness);
                
                // 混合颜色
                fixed4 color = lerp(_EdgeColor, _CenterColor, gradient);
                
                return color;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
}