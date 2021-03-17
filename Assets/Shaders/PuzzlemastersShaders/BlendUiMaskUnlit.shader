Shader "Custom/BlendUiMaskUnlit"
{
	Properties
	{
		_FirstTex ("First", 2D) = "white" {}
		_SecTex ("Second ", 2D) = "white" {}
		_Blend ("Blend value", Range(0,1)) = 0.0

		_Color ("Color", Color) = (1,1,1,1)

		_StencilComp ("Stencil Comparison", Float) = 8
 		_Stencil ("Stencil ID", Float) = 0
 		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
 		_StencilReadMask ("Stencil Read Mask", Float) = 255
 		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="False"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ColorMask [_ColorMask]

		Cull Off
		Lighting Off
		ZWrite On
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
            #include "UnityUI.cginc"


            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION; 
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
			};
			

			fixed4 _Color;
			v2f vert(appdata_t IN)
			{
				v2f OUT;

                OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;

				return OUT;
			}

			sampler2D _FirstTex;
			sampler2D _SecTex;
		    half _Blend;
            float4 _ClipRect;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c1 = tex2D(_FirstTex, IN.texcoord) * IN.color;
				fixed4 c2 = tex2D(_SecTex, IN.texcoord) * IN.color;

                fixed4 c = lerp(c1, c2, _Blend);

				c.rgb *= c.a;
                c.a *=  UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				return c;
			}
		    ENDCG
		}
	}
}
