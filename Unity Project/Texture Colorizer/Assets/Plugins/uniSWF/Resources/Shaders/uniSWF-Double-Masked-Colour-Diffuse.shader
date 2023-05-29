// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "uniSWF/Transparent/uniSWF-Double-Masked-Colour-Diffuse" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1) 
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex ("Base (RGB)", 2D) = "white" {}
		_Matrix2D ("_Matrix2D", Vector) = (0,0,0,0)
		//_Translate ("_Translate", Vector) = (0,0,0,0)	
		_TestX ("_tX", Float) = 0
		_TestY ("_tY", Float) = 0
	}
	
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha 
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
			sampler2D _MaskTex;
		//	float4 _Translate;
			float4 _Matrix2D;
			float _tX;
			float _tY;	
		
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
				
				// Build position from sprite space to UV
				//float2 inPos = float2( (i.texcoord.x)-_Translate.x, _Translate.y+(1-i.texcoord.y) );
				float2 inPos = float2( i.texcoord.x, 1-i.texcoord.y );
				
				// Unpack matrix				
				float a = _Matrix2D[0];
				float b = _Matrix2D[1];
				float c = _Matrix2D[2];
				float d = _Matrix2D[3];
				
				// Matrix2D scale & rotate
				float2 p;
				
				p.x = inPos.x*a + inPos.y*c;
				p.y = inPos.x*b + inPos.y*d;				
												
				// Offset Y ( flipped from sprite space adjustment )
				p.x += _tX;
				p.y += _tY;
				p.y += 1;

				// Read mask using transforms UV's
				half4 mask = tex2D (_MaskTex, p);

				
				if( p.x > 1 ) {
					mask = 0;
				} else if( p.x < 0 ) {
					mask = 0;
				}
				if( p.y > 1 ) {
					mask = 0;
				} else if( p.y < 0 ) {
					mask = 0;
				}
																
				float4 result = tex2D(_MainTex, i.texcoord);
			
				result.a = mask.a;
				
				return result;
			}
			ENDCG 
		}

	}

	Fallback off 
}
