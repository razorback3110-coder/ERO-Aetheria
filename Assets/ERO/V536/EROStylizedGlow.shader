Shader "ERO/StylizedGlow"
{
    Properties { _BaseColor("Base Color", Color) = (0.2,0.5,1,1) _Glow("Glow", Range(0,8)) = 2 }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 normalWS : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor; float _Glow;
            CBUFFER_END
            Varyings vert(Attributes IN){Varyings OUT;OUT.positionHCS=TransformObjectToHClip(IN.positionOS.xyz);OUT.normalWS=TransformObjectToWorldNormal(IN.normalOS);return OUT;}
            half4 frag(Varyings IN):SV_Target{float rim=pow(1-saturate(dot(normalize(IN.normalWS),normalize(float3(0.3,0.7,0.5)))),2);return half4(_BaseColor.rgb*(1+rim*_Glow),_BaseColor.a);}
            ENDHLSL
        }
    }
}
