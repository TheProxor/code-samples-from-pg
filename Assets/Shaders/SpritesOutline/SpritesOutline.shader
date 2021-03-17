Shader "Custom/SpritesOutline" {
	Properties {
		[NoScaleOffset][PerRendererData] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0

		  _OutlineWidth("Outline Width", Range(0,100)) = 10.0
		  _OutlineColor("Outline Color", Color) = (1,1,0,1)

		  _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		  _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		  _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		  _Offset("Outline Pixels Offset", float) = 0.7

		  _TextureSpaceMultiplier("Texture Space Multiplier", float) = 1.0
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "DisableBatching" = "true" }

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off
		Pass
		{
			Name "Outline"

			CGPROGRAM
			#pragma vertex vertOutline
			#pragma fragment fragOutline
			#include "CGIncludes/Sprites-Outline-Pass.cginc"
			ENDCG
		}
	}

	FallBack "Sprites/Default"
}
