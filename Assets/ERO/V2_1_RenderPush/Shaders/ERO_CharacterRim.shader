Shader "ERO/V21/CharacterRim"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.65,0.72,0.85,1)
        _RimColor ("Rim Color", Color) = (0.25,0.55,1,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
        _RimStrength ("Rim Strength", Range(0,3)) = 0.8
        _Metallic ("Metallic", Range(0,1)) = 0.15
        _Smoothness ("Smoothness", Range(0,1)) = 0.65
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float _RimPower;
                float _RimStrength;
                float _Metallic;
                float _Smoothness;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs pos = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(v.normalOS);
                o.positionHCS = pos.positionCS;
                o.normalWS = NormalizeNormalPerVertex(n.normalWS);
                o.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 v = normalize(i.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(n, v)), _RimPower);

                Light mainLight = GetMainLight();
                float ndl = saturate(dot(n, mainLight.direction));
                float3 lit = _BaseColor.rgb * (0.22 + ndl * mainLight.color.rgb);
                lit += _RimColor.rgb * fresnel * _RimStrength;

                return half4(lit, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
