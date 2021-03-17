Shader "Custom/AlwaysVisibleTexture"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Pass
        {
            ZWrite Off
            ZTest Always
            
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D _MainTex;
            
            struct vertexInput
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct vertexOutput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            
            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;
                
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                
                return output;
            }
            
            
            float4 frag(vertexOutput input) : COLOR
            {
                return tex2D(_MainTex, input.uv);
            }
            
            ENDCG
        }
    }
}
