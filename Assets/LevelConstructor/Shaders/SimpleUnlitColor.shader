Shader "Custom/SimpleUnlitColor"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
        
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            uniform float4 _Color;
            
            float4 vert(float4 position : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(position);
            }
            
            
            float4 frag(void) : COLOR
            {
                return _Color;
            }

            ENDCG
        }
    }
}
