// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "uniSWF/Transparent/DiffuseTexDoubeSidedAlphaAdd" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1) 
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}
	
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Lighting Off
		Blend SrcAlpha One 
		Cull Off 
		ZWrite Off
		
		
		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;

			uniform float4 _MainTex_ST;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			float4 _Color;

			fixed4 frag (v2f i) : COLOR
			{
				//return tex2D(_MainTex, i.texcoord) * _Color;
				return tex2D(_MainTex, i.texcoord) * i.color;
			}
			ENDCG 
		}

	}

	Fallback off 
}
