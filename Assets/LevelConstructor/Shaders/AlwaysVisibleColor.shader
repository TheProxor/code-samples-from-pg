Shader "Custom/AlwaysVisibleColor"
{
    Properties
    {
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags 
        {
            "Queue" = "Transparent"
        }
        Pass
        {
            ZWrite Off
            ZTest Always
            
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            uniform float4 _Color;
            
            float4 vert(float4 position : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(position);
            }
            
            
            float4 frag(void) : Color
            {
                return _Color;
            }
            
            ENDCG
        }
    }
}
