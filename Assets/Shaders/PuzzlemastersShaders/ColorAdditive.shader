
// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Custom/Particles/Color Additive" {
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Blend SrcAlpha One
    Cull Off
    Lighting Off
    ZWrite Off
    Fog { Color (0,0,0,0) }
    
    BindChannels {
        Bind "Color", color
        Bind "Vertex", vertex
        Bind "TexCoord", texcoord
    }
    
    SubShader {
        Pass {
            SetTexture [_MainTex] {
                ConstantColor [_Color]
                Combine constant * primary
            }

           SetTexture [_MainTex] {
                combine texture * previous
            }
        }
    }
}
}