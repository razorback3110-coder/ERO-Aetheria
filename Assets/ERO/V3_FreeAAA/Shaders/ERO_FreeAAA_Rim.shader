Shader "ERO/V3/FreeAAA/RimLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.65,0.70,0.80,1)
        _RimColor ("Rim", Color) = (0.25,0.55,1,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
        _RimStrength ("Rim Strength", Range(0,3)) = 0.65
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
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 viewWS:TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor; float4 _RimColor; float _RimPower; float _RimStrength;
            CBUFFER_END
            V vert(A a)
            {
                V o; VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);
                VertexNormalInputs n=GetVertexNormalInputs(a.normalOS);
                o.positionCS=p.positionCS; o.normalWS=n.normalWS; o.viewWS=GetWorldSpaceViewDir(p.positionWS); return o;
            }
            half4 frag(V i):SV_Target
            {
                float3 n=normalize(i.normalWS); float3 v=normalize(i.viewWS); Light l=GetMainLight();
                float ndl=saturate(dot(n,l.direction)); float rim=pow(1.0-saturate(dot(n,v)),_RimPower);
                return half4(_BaseColor.rgb*(0.22+ndl*l.color.rgb)+_RimColor.rgb*rim*_RimStrength,_BaseColor.a);
            }
            ENDHLSL
        }
    }
}
