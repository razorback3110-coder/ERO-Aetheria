Shader "ERO/RiftGlow"
{
    Properties { _BaseColor("Base Color", Color)=(0.12,0.22,1,1) _Glow("Glow", Range(0,8))=2.5 _Pulse("Pulse", Range(0,4))=1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionHCS:SV_POSITION; float3 normalWS:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor; float _Glow; float _Pulse;
            CBUFFER_END
            Varyings vert(Attributes IN){ Varyings OUT; OUT.positionHCS=TransformObjectToHClip(IN.positionOS.xyz); OUT.normalWS=TransformObjectToWorldNormal(IN.normalOS); return OUT; }
            half4 frag(Varyings IN):SV_Target { float p=1+sin(_Time.y*2.2)*0.18*_Pulse; float fres=pow(1-saturate(dot(normalize(IN.normalWS),normalize(GetWorldSpaceViewDir(0)))),2); return half4(_BaseColor.rgb*(_Glow*p)*(1+fres),_BaseColor.a); }
            ENDHLSL
        }
    }
}
