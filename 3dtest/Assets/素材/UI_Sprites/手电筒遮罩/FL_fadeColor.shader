// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/FL_fadeColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color Tint",Color)=(1,1,1,1)
        _Gloss("Gloss",Range(8.0,256))=20
        _Specular("Specular Color",Color)=(1,1,1,1)

        _Playerpos("Player Position",Vector)=(0,0,0,0)
        _fadeRange("Fade Range",float)=1
        _fadeDist("Fade Distance",float)=10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Tags{"LightMode"="ForwardBase"}
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldpos:TEXCOORD1;
                SHADOW_COORDS(2)
                float3 worldNormal:TEXCOORD3;
            };

            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color; fixed4 _Playerpos; float _fadeRange;float _fadeDist; float _Gloss; fixed4 _Specular;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                //o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldpos = UnityObjectToWorldDir(v.vertex.xyz);
                o.worldNormal=normalize(UnityObjectToWorldNormal(v.normal));
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldpos=normalize(i.worldpos);
                float3 worldLightDir=normalize(UnityWorldSpaceLightDir(worldpos));
                float3 worldViewDir=normalize(UnityWorldSpaceViewDir(worldpos));
                fixed vertexDist=length((_Playerpos-i.worldpos.xyz));

                fixed3 ambient=UNITY_LIGHTMODEL_AMBIENT.xyz;
                fixed3 diffuse = _Color.rgb*tex2D(_MainTex, i.uv).rgb*max(0,dot(i.worldNormal,worldLightDir));
                fixed3 specular=_LightColor0.rgb*pow(max(0,dot(worldLightDir,i.worldNormal)),_Gloss)*_Specular.rgb;
                UNITY_LIGHT_ATTENUATION(atten,i,i.worldpos);

                // 计算衰减因子
                //顶点距离大于fadeDist+1，t=1，顶点距离小于fadeDist，t=0
                float t = saturate( (vertexDist - _fadeDist) / _fadeRange);

                // 混合本色与黑色半透明
                float3 black = float3(0,0,0);
                fixed3 color=ambient+specular+diffuse;
                float alpha = atten * t;
                //顶点距离大于fadeDist+1，物体为黑色，顶点距离小于fadeDist，物体为本身颜色
                float3 finalColor = lerp(black, color, 1-t);

                return fixed4(finalColor*atten,1);
            }
            ENDCG
        }
    }
}
