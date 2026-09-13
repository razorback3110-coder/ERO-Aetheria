Shader "ERO/Holographic"
{
 Properties { _BaseColor("Base Color",Color)=(0.08,0.25,0.55,1) _Emission("Emission",Color)=(0.15,0.55,1,1) _Scan("Scan",Range(0,8))=2 _Fresnel("Fresnel",Range(0,8))=3 }
 SubShader { Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off
  Pass { HLSLPROGRAM #pragma vertex vert #pragma fragment frag #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;}; struct V{float4 positionHCS:SV_POSITION;float3 n:NORMAL;float3 v:TEXCOORD0;};
   CBUFFER_START(UnityPerMaterial) float4 _BaseColor,_Emission;float _Scan,_Fresnel; CBUFFER_END
   V vert(A i){V o;VertexPositionInputs p=GetVertexPositionInputs(i.positionOS.xyz);o.positionHCS=p.positionCS;o.n=TransformObjectToWorldNormal(i.normalOS);o.v=GetWorldSpaceViewDir(p.positionWS);return o;}
   half4 frag(V i):SV_Target{float3 n=normalize(i.n),v=normalize(i.v);float fres=pow(1-saturate(dot(n,v)),_Fresnel);float scan=.5+.5*sin((i.positionHCS.y+i.positionHCS.x)*.03*_Scan);float3 col=_BaseColor.rgb+_Emission.rgb*(fres*.8+scan*.15);return half4(col,saturate(.35+fres*.45));}
  ENDHLSL }
 }
}
