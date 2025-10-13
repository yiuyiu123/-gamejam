// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/backBlack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color Tint",Color)=(1,1,1,1)
        _Specular("Specular",Color)=(1,1,1,1)
        _AlphaScale("Alpha Scale",Range(0,1))=1
        _Gloss("Gloss",Range(8.0,256))=20
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent" "ForceNoShadowCasting"="True"}
        
        Pass//背面半透明黑
        {
            ZWrite On
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            Tags{"LightMode"="ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
                float3 worldpos:TEXCOORD2;
                float3 worldNormal:TEXCOORD3;
            };

            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color; fixed4 _Specular; float _AlphaScale;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldpos=(UnityObjectToClipPos(v.vertex)).xyz;
                o.worldNormal=UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldNormal=normalize(i.worldNormal);
                float3 worldLightDir=normalize(UnityWorldSpaceLightDir(i.worldpos));
                fixed3 albedo=_LightColor0.rgb*_Color.rgb*tex2D(_MainTex,i.uv).rgb*max(0,dot(worldNormal,worldLightDir));
                fixed3 ambient=UNITY_LIGHTMODEL_AMBIENT;

                return fixed4(0,0,0,_AlphaScale);
            }
            ENDCG
        }

        Pass//正面Phong
        {
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            Tags{"LightMode"="ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
                float3 worldpos:TEXCOORD2;
                float3 worldNormal:TEXCOORD3;
            };

            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color; fixed4 _Specular; float _AlphaScale;
            fixed _Gloss;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldpos=(UnityObjectToClipPos(v.vertex)).xyz;
                o.worldNormal=UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldNormal=normalize(i.worldNormal);
                float3 worldLightDir=normalize(UnityWorldSpaceLightDir(i.worldpos));
                fixed3 albedo=_LightColor0.rgb*_Color.rgb*tex2D(_MainTex,i.uv).rgb*max(0,dot(worldNormal,worldLightDir));
                fixed3 ambient=UNITY_LIGHTMODEL_AMBIENT;
                fixed3 specular=_Specular.rgb*_LightColor0.rgb*pow(max(0,dot(worldLightDir,worldNormal)),_Gloss);

                return fixed4(ambient+albedo+specular,1);
            }
            ENDCG
        }
    }
}
