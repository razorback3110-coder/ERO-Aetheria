Shader "ERO/Rift"
{
 Properties { _Color("Color",Color)=(0.15,0.4,1,1) _Speed("Speed",Range(0,5))=1.2 _Emission("Emission",Range(0,8))=3 }
 SubShader { Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha One ZWrite Off
  Pass { HLSLPROGRAM #pragma vertex vert #pragma fragment frag #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A{float4 p:POSITION;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;};CBUFFER_START(UnityPerMaterial)float4 _Color;float _Speed,_Emission;CBUFFER_END
   V vert(A i){V o;VertexPositionInputs x=GetVertexPositionInputs(i.p);o.p=x.positionCS;o.w=x.positionWS;return o;}half4 frag(V i):SV_Target{float wave=0.5+0.5*sin((i.w.x+i.w.z)*2+_Time.y*_Speed*5);float3 c=_Color.rgb*(_Emission*(.45+wave*.55));return half4(c,.35+wave*.35);}
  ENDHLSL}
 }
}
