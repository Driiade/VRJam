// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Depth Restore" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	ZTest Always
	ColorMask 0
	Cull Off 
	ZWrite On 
	Fog { Mode Off }

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
			
			void frag (v2f i, out float4 fragColor : SV_Target, out float fragDepth : SV_Depth) 
			{
				float z = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(i.texcoord, 0, 0));
				fragColor = float4(1, 0, 1, 1);
				fragDepth = z;
			}
		ENDCG
	}
}

}
