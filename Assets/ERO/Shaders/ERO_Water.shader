Shader "ERO/Water"
{
 Properties { _Color("Color",Color)=(0.03,0.18,0.35,1) _Speed("Speed",Range(0,4))=1 _Opacity("Opacity",Range(0,1))=.75 }
 SubShader { Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off
  Pass { HLSLPROGRAM #pragma vertex vert #pragma fragment frag #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A{float4 p:POSITION;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;};CBUFFER_START(UnityPerMaterial)float4 _Color;float _Speed,_Opacity;CBUFFER_END
   V vert(A i){V o;VertexPositionInputs x=GetVertexPositionInputs(i.p);o.p=x.positionCS;o.w=x.positionWS;return o;}half4 frag(V i):SV_Target{float w=.5+.5*sin(i.w.x*2+i.w.z*1.7+_Time.y*_Speed*2);return half4(_Color.rgb*(.65+w*.35),_Opacity);}
  ENDHLSL}
 }
}
