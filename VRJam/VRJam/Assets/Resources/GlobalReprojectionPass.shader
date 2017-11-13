Shader "Custom/GlobalReprojectionPass"
{
	Properties
	{
		_ReprojectionMaskColor("Reprojection Mask Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Cull Off
		ZWrite On
		ZTest Always

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			#define _DepthThreshold    0.001f
			float4x4 _OtherEyeViewProj;
			float4 _ReprojectionMaskColor;
			UNITY_DECLARE_TEX2D(_OtherEyeTex);
			UNITY_DECLARE_TEX2D(_OtherEyeDepthTexture);
			sampler2D _CameraDepthTexture;
			float4x4 _InvViewProj;
			float _ShowReprojected;

			// Sample a triangle shape and found the closet depth to the eye
			// Note: Unity 5.6 is using inversed depth buffer here, so the bigger value , the closer
			float GetOtherEyeConservativeDepth(float2 otherEyeUV)
			{
				float otherEyeDepth = _OtherEyeDepthTexture.Sample(sampler_OtherEyeDepthTexture, otherEyeUV + float2(0, 2 / _ScreenParams.y), 0);
				otherEyeDepth = max(otherEyeDepth, _OtherEyeDepthTexture.Sample(sampler_OtherEyeDepthTexture, otherEyeUV + float2(2 / _ScreenParams.x, -2 / _ScreenParams.y), 0));
				otherEyeDepth = max(otherEyeDepth, _OtherEyeDepthTexture.Sample(sampler_OtherEyeDepthTexture, otherEyeUV + float2(-2 / _ScreenParams.x, -2 / _ScreenParams.y), 0));
				return otherEyeDepth;
			}

			float4 frag(v2f i) : SV_Target
			{
				// read depth and reconstruct world position of pixel
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float4 clipSpacePos = float4(i.uv*2.0 - 1.0, depth, 1);
				float4 worldPos = mul(_InvViewProj, clipSpacePos);
				worldPos.xyz /= worldPos.w;

				// transform into other eye's view
				float4 otherClipSpacePos = mul(_OtherEyeViewProj, float4(worldPos.xyz, 1));
				otherClipSpacePos.xyz /= otherClipSpacePos.w;

				// read color
				float2 otherEyeUV = otherClipSpacePos.xy*0.5 + 0.5;
				float4 otherEyeColor = _OtherEyeTex.Sample(sampler_OtherEyeTex, otherEyeUV, 0);
				float isReprojectable = otherEyeColor.a;

				// Verify if if it is a valid reprojection by using conservativeFilter
				float otherEyeDepth = GetOtherEyeConservativeDepth(otherEyeUV);
				float diff = otherEyeDepth - depth;
				if (diff > _DepthThreshold * isReprojectable)
				{
					discard;            // this will be filled in later
				}
				
				return _ReprojectionMaskColor * otherEyeColor;
			 }
			 ENDCG
		 }
	}
}
