Shader "Custom/CircularRevealWorldSpace"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _CircleCenter("Circle Center", Vector) = (0, 0, 0, 0)
        _RevealRadius("Reveal Radius", Range(0, 1000)) = 0.0
        _EdgeWidth("Edge Width", Range(0, 10)) = 0.1
        _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeEmission("Edge Emission", Float) = 1.0
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            LOD 200

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
                float4 _CircleCenter;
                float _RevealRadius;
                float _EdgeWidth;
                float4 _EdgeColor;
                float _EdgeEmission;

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
                    // Calculate the radial distance from the circle center
                    float radialDist = distance(i.worldPos, _CircleCenter.xyz);

                // Reveal effect logic
                float edge = smoothstep(_RevealRadius - _EdgeWidth, _RevealRadius, radialDist);
                float alpha = step(_RevealRadius, radialDist);

                // Sample the main texture
                fixed4 mainColor = tex2D(_MainTex, i.uv);

                // Add emission to the edge
                fixed4 edgeWithEmission = _EdgeColor * _EdgeEmission;

                // Blend edge color and fade unrevealed areas
                fixed4 finalColor = lerp(edgeWithEmission, mainColor, edge);
                finalColor.a *= 1.0 - alpha;

                return finalColor;
            }
            ENDCG
        }
        }
}
