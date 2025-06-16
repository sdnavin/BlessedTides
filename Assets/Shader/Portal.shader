Shader "Custom/SwirlPortalTransparent"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" { }
        _DistortionTex("Distortion Texture", 2D) = "bump" { }
        _SwirlSpeed("Swirl Speed", Float) = 1.0
        _SwirlAmount("Swirl Amount", Float) = 0.1
        _Radius("Portal Radius", Float) = 5.0
        _PortalColor("Portal Color", Color) = (0, 1, 1, 1)
        _EmissionStrength("Emission Strength", Float) = 1.0
        _Opacity("Opacity", Range(0, 1)) = 1.0 // Control transparency
    }
        SubShader
        {
            Tags {"Queue" = "Overlay" "RenderType" = "Transparent"}
            Pass
            {
                // Set the blending mode to transparent
                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off // Disable depth writing for transparency

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

            // Properties
            sampler2D _MainTex;
            sampler2D _DistortionTex;
            float _SwirlSpeed;
            float _SwirlAmount;
            float _Radius;
            float4 _PortalColor;
            float _EmissionStrength;
            float _Opacity;

            // Shader variables
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Fragment Shader
            half4 frag(v2f i) : SV_Target
            {
                // Get the distortion effect using the distortion texture
                float2 distortion = tex2D(_DistortionTex, i.uv).rg;

                // Calculate the swirl effect using the sine and cosine functions
                float2 center = float2(0.5, 0.5);  // Center of the texture (UV space)
                float2 uvOffset = i.uv - center;  // Get the offset from the center
                float angle = atan2(uvOffset.y, uvOffset.x);  // Calculate angle based on offset
                float radius = length(uvOffset);  // Calculate the distance from the center

                // Apply the swirl effect (rotate the UVs based on radius and time)
                angle += _SwirlSpeed * _Time.y + _SwirlAmount * radius;

                // Convert back to UV space after the rotation
                uvOffset = float2(cos(angle), sin(angle)) * radius;
                i.uv = center + uvOffset;

                // Sample the main texture with the distorted UVs
                half4 mainColor = tex2D(_MainTex, i.uv);

                // Calculate the distance from the center to fade out the portal
                float distanceFromCenter = length(i.uv - 0.5);
                float fade = smoothstep(_Radius, _Radius - 0.1, distanceFromCenter);

                // Apply the fade to the portal's color
                mainColor *= fade;

                // Apply the color tint (can be used to give a glowing effect)
                mainColor *= _PortalColor;

                // Set the emission strength for glowing effect
                mainColor.rgb *= _EmissionStrength;

                // Use _Opacity to control transparency, black (0) will be transparent, white (1) fully opaque
                mainColor.a *= _Opacity;

                return mainColor;
            }
            ENDCG
        }
        }
            FallBack "Diffuse"
}
