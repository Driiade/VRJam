// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Copy Depth" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }

	Tags { "RenderType"="Opaque" }
	LOD 100
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float z = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(i.texcoord, 0, 0));
				return float4(z.xxx, 1);
			}
		ENDCG
	}
}

}
