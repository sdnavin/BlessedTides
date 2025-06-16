Shader "Custom/RevealObjectB"
{
    Properties
    {
        _Color("Base Color", Color) = (1, 1, 1, 1)
        _RevealAmount("Reveal Amount", Range(0, 1)) = 0.0
        _CircleCenter("Circle Center", Vector) = (0, 0, 0, 0)
        _Radius("Reveal Radius", Range(0, 1000)) = 0.0
    }

        SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _RevealAmount;
            float4 _CircleCenter;
            float _Radius;

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
                // Calculate radial distance from center of the reveal circle
                float radialDist = distance(i.worldPos, _CircleCenter.xyz);

            // Fade effect based on reveal radius
            float alpha = smoothstep(_Radius - 0.1, _Radius, radialDist); // Smooth transition

            // Apply the reveal effect based on _RevealAmount
            alpha *= _RevealAmount;

            fixed4 color = _Color;
            color.a *= alpha; // Control visibility based on reveal amount

            return color;
        }
        ENDCG
    }
    }
}
