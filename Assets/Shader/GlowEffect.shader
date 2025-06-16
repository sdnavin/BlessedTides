Shader "Custom/SimpleGlowShader"
{
    Properties
    {
        _GlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _GlowIntensity("Glow Intensity", Range(0, 10)) = 2
        _GlowSpeed("Glow Speed", Range(0.1, 5)) = 1
        _GlowMinimum("Glow Minimum", Range(0, 1)) = 0
        _GlowMaximum("Glow Maximum", Range(0, 1)) = 1
        _BlendMode("Blend Mode", Float) = 1 // 0 = Add, 1 = Overlay, 2 = Screen
    }

        SubShader
    {
        Tags
        {
            "Queue" = "Transparent+100"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowSpeed;
            float _GlowMinimum;
            float _GlowMaximum;
            float _BlendMode;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Calculate rim effect (stronger on edges)
                float rim = 1.0 - saturate(dot(i.normal, i.viewDir));
                rim = pow(rim, 3.0) * 2.0;

                // Calculate pulse effect
                float pulse = (sin(_Time.y * _GlowSpeed) + 1.0) * 0.5;
                pulse = lerp(_GlowMinimum, _GlowMaximum, pulse);

                // Apply glow with rim and pulse effects
                float4 glowFinal = _GlowColor * _GlowIntensity * pulse;
                glowFinal.a = _GlowColor.a * pulse * (1.0 + rim * 0.5);

                // Handle different blend modes
                if (_BlendMode < 0.5) {
                    // Additive blending handled by return
                    return glowFinal;
                }
                else if (_BlendMode < 1.5) {
                    // Overlay-like effect 
                    glowFinal.rgb *= 0.8; // Slightly reduce intensity for overlay
                    return glowFinal;
                }
                else {
                    // Screen-like effect
                    glowFinal.rgb *= 0.7; // Reduce intensity for screen
                    glowFinal.a *= 0.8;  // More transparent for screen
                    return glowFinal;
                }
            }
            ENDCG
        }
    }

        FallBack "Diffuse"

                CustomEditor "SimpleGlowShaderGUI"
}