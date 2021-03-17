#include "UnityCG.cginc"

sampler2D _MainTex;

float _Offset;
float _OutlineWidth;
float4 _OutlineColor;
float4 _MainTex_TexelSize;
float _ThresholdEnd;
float _OutlineSmoothness;
int _OutlineReferenceTexWidth;

float _TextureSpaceMultiplier;

struct VertexInput
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float4 vertexColor : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
 {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float vertexColorAlpha : COLOR;
};


VertexOutput vertOutline(VertexInput v)
 {
	VertexOutput o;

	UNITY_SETUP_INSTANCE_ID(v);
	o.pos = UnityObjectToClipPos(v.vertex * _TextureSpaceMultiplier);
	o.uv = v.uv;

	o.vertexColorAlpha = v.vertexColor.a;
	return o;
}

float4 fragOutline(VertexOutput i) : SV_Target
{
	float4 texColor = fixed4(0,0,0,0);
	float2 tiling = float2(_TextureSpaceMultiplier, _TextureSpaceMultiplier);

	float uvOffsetValue = (_TextureSpaceMultiplier * -0.5) + 0.5;
	float2 uvOffset = float2(uvOffsetValue, uvOffsetValue);
	float2 uv = i.uv * tiling + uvOffset;
 
	float outlineWidthCompensated = _OutlineWidth / (_OutlineReferenceTexWidth * _MainTex_TexelSize.x);
	float2 outlineOffset = _MainTex_TexelSize.xy * outlineWidthCompensated;
 
	static const float2 offsets[] = 
	{
		float2(0, 1),
		float2(0, -1),
		float2(-1, 0),
		float2(1, 0),
		float2(-1, 1),
		float2(1, 1),
		float2(-1, -1),
		float2(1, -1),
	};

	float averagePixelOffset = 0.0;
	for (int index = 0; index < offsets.Length; index++)
	{
		float additionalDiagonalOffset = lerp(1.0, _Offset, index > 3);
		float2 pixelOffset =  offsets[index] * outlineOffset * additionalDiagonalOffset;

		averagePixelOffset += tex2D(_MainTex, uv + pixelOffset).a;
	}

	float average = averagePixelOffset * i.vertexColorAlpha / offsets.Length;

	float thresholdStart = _ThresholdEnd * (1.0 - _OutlineSmoothness);
	float pixelCurrent = tex2D(_MainTex, uv).a;
	float outlineAlpha = saturate((average - thresholdStart) / (_ThresholdEnd - thresholdStart)) - pixelCurrent;

	texColor.rgba = lerp(texColor, _OutlineColor, outlineAlpha);
 
	return texColor;
}
