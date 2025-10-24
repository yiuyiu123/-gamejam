
Shader "Unlit/backBlack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color Tint",Color)=(1,1,1,1)
        _Specular("Specular",Color)=(1,1,1,1)
        _AlphaScale("Alpha Scale",Range(0,1))=1
        _Gloss("Gloss",Range(8.0,256))=20
        //法线
        _NormalMap("Normal Map",2D)="bump"{}
        _BumpScale("Bump Scale",float)=1.0
    }
    SubShader
    {
        Tags { "Queue" = "Geometry-10"  "IgnoreProjector"="true" "RenderType"="Opaque" "ForceNoShadowCasting"="True"}
        
        /*Pass//背面半透明黑
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
                o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal=UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(0,0,0,_AlphaScale);
            }
            ENDCG
        }*/

        Pass // 正面Phong
        {
            ZWrite On
            Cull Back
            Blend Off
            Tags{"LightMode"="ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
                float3 worldpos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 tangentWorld : TEXCOORD4;
                float3 bitangentWorld : TEXCOORD5;
            };

            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color; fixed4 _Specular; float _AlphaScale;
            fixed _Gloss; sampler2D _NormalMap; float _BumpScale;

            v2f vert (appdata_tan v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o, o.pos);

                o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 worldBitangent = cross(o.worldNormal, worldTangent) * v.tangent.w;

                o.tangentWorld = worldTangent;
                o.bitangentWorld = worldBitangent;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ✅ 法线贴图采样只能在 fragment 中做
                float3 normalTex = UnpackNormal(tex2D(_NormalMap, i.uv)) * _BumpScale;

                // 转换切线空间法线到世界空间
                float3x3 TBN = float3x3(normalize(i.tangentWorld), normalize(i.bitangentWorld), normalize(i.worldNormal));
                float3 worldNormal = normalize(mul(normalTex, TBN));

                float3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldpos));
                fixed3 albedo = _LightColor0.rgb * _Color.rgb * tex2D(_MainTex, i.uv).rgb * max(0, dot(worldNormal, worldLightDir));
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT;
                fixed3 specular = _Specular.rgb * _LightColor0.rgb * pow(max(0, dot(worldLightDir, worldNormal)), _Gloss);

                return fixed4(ambient + albedo + specular, 1);
            }
            ENDCG
        }

        //点光源
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            ZWrite On
            Cull Back
            Blend Off
            //Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
                float3 worldpos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 tangentWorld : TEXCOORD4;
                float3 bitangentWorld : TEXCOORD5;
            };

            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color; fixed4 _Specular; float _AlphaScale;
            fixed _Gloss; sampler2D _NormalMap; float _BumpScale;

            v2f vert (appdata_tan v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o, o.pos);

                o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 worldBitangent = cross(o.worldNormal, worldTangent) * v.tangent.w;

                o.tangentWorld = worldTangent;
                o.bitangentWorld = worldBitangent;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ✅ 法线贴图采样只能在 fragment 中做
                float3 normalTex = UnpackNormal(tex2D(_NormalMap, i.uv)) * _BumpScale;

                // 转换切线空间法线到世界空间
                float3x3 TBN = float3x3(normalize(i.tangentWorld), normalize(i.bitangentWorld), normalize(i.worldNormal));
                float3 worldNormal = normalize(mul(normalTex, TBN));

                float3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldpos));
                fixed3 albedo = _LightColor0.rgb * _Color.rgb * tex2D(_MainTex, i.uv).rgb * max(0, dot(worldNormal, worldLightDir));
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT;
                fixed3 specular = _Specular.rgb * _LightColor0.rgb * pow(max(0, dot(worldLightDir, worldNormal)), _Gloss);

                return fixed4(ambient + albedo + specular, 1);
            }
            ENDCG
        }
    }
}
