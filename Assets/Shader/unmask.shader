Shader "Custom/UnmaskingFadeIn"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _RevealCenter("Reveal Center", Vector) = (0, 0, 0, 0)
        _RevealRadius("Reveal Radius", Range(0, 1000)) = 0.0
        _EdgeWidth("Edge Width", Range(0, 1)) = 0.1
        _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
        _FadeSpeed("Fade Speed", Range(0, 5)) = 1.0
    }

        SubShader
        {
            Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }
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
                    float3 worldPos : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _RevealCenter;
                float _RevealRadius;
                float _EdgeWidth;
                float4 _EdgeColor;
                float _FadeSpeed;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Calculate the distance from the reveal center to each pixel
                    float dist = distance(i.worldPos, _RevealCenter.xyz);

                // Fade the edge color based on distance
                float mask = smoothstep(_RevealRadius - _EdgeWidth, _RevealRadius, dist);

                // Gradual fade-in effect over time
                float fade = clamp(_Time.y * _FadeSpeed, 0.0, 1.0); // Use _Time.y for the time-based fade

                // Sample the main texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Apply the mask and fade effect
                texColor.a *= mask * fade;

                return texColor;
            }

            ENDCG
        }
        }

            // Fallback to a simple unlit shader
                Fallback "Unlit/Color"
}
