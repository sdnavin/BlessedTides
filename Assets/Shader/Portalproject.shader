Shader "Custom/ProjectorReveal"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _RevealRadius("Reveal Radius", Range(0, 1)) = 0.0
        _EdgeWidth("Edge Width", Range(0, 0.1)) = 0.05
        _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
    }
        SubShader
        {
            Tags { "Queue" = "Overlay" "IgnoreProjector" = "False" }
            Pass
            {
                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off

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
                };

                sampler2D _MainTex;
                float _RevealRadius;
                float _EdgeWidth;
                float4 _EdgeColor;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Sample the main texture
                    float4 mainColor = tex2D(_MainTex, i.uv);

                    // Calculate radial distance
                    float2 uv = i.uv * 2.0 - 1.0; // Center UV at (0,0)
                    float distance = length(uv);

                    // Determine reveal effect
                    float edge = smoothstep(_RevealRadius - _EdgeWidth, _RevealRadius, distance);
                    float alpha = step(_RevealRadius, distance);

                    // Blend with edge color
                    float4 finalColor = lerp(_EdgeColor, mainColor, edge);
                    finalColor.a *= 1.0 - alpha;

                    return finalColor;
                }
                ENDCG
            }
        }
}
