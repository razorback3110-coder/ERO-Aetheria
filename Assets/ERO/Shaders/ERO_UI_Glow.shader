Shader "ERO/UI Glow"
{
 Properties { _Color("Color",Color)=(0.2,0.55,1,1) _Glow("Glow",Range(0,5))=1.5 }
 SubShader { Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
  Pass { HLSLPROGRAM #pragma vertex vert #pragma fragment frag #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};CBUFFER_START(UnityPerMaterial)float4 _Color;float _Glow;CBUFFER_END
   V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}half4 frag(V i):SV_Target{float2 q=i.uv-.5;float g=exp(-dot(q,q)*12)*_Glow;return half4(_Color.rgb*(1+g),_Color.a*(.65+g*.2));}
  ENDHLSL}
 }
}
