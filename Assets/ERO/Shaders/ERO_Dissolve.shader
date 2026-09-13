Shader "ERO/Dissolve"
{
 Properties { _BaseColor("Base Color",Color)=(0.2,0.35,0.65,1) _EdgeColor("Edge",Color)=(0.4,0.8,1,1) _Dissolve("Dissolve",Range(0,1))=0 _EdgeWidth("Edge Width",Range(0.001,0.2))=.05 }
 SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" } Pass { HLSLPROGRAM #pragma vertex vert #pragma fragment frag #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  struct A{float4 p:POSITION;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;};CBUFFER_START(UnityPerMaterial)float4 _BaseColor,_EdgeColor;float _Dissolve,_EdgeWidth;CBUFFER_END
  V vert(A i){V o;VertexPositionInputs x=GetVertexPositionInputs(i.p.xyz);o.p=x.positionCS;o.w=x.positionWS;return o;} half4 frag(V i):SV_Target{float n=frac(sin(dot(i.w,float3(12.9898,78.233,37.719)))*43758.5453);float d=n-_Dissolve;if(d<0)discard;float edge=1-smoothstep(0,_EdgeWidth,d);return half4(lerp(_BaseColor.rgb,_EdgeColor.rgb,edge),1);}
 ENDHLSL}}
}
