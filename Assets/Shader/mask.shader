Shader "Custom/CircularMeshMask"
{
    Properties
    {
        _CircleCenter("Circle Center", Vector) = (0, 0, 0, 0)
        _Radius("Mask Radius", Range(0, 1000)) = 0.0
        _MaskColor("Mask Color", Color) = (0, 0, 0, 0) // Transparent mask color
    }

        SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        Pass
        {
            // Setup stencil to mask inside the circle
            Stencil
            {
                Ref 1
                Comp always
                Pass replace
            }

        // Set blend mode and other settings
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ColorMask RGB

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

        float4 _CircleCenter;
        float _Radius;
        float4 _MaskColor;

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

        // If inside the circle, discard the fragment (mask it out)
        if (radialDist < _Radius)
            discard;  // Mask out

        return _MaskColor;
    }
    ENDCG
}
    }

        SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        Pass
        {
            // Apply the mask after the first pass using the stencil buffer
            Stencil
            {
                Ref 1
                Comp equal
                Pass keep
            }

        // Set blend mode and other settings
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ColorMask RGB

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        float4 _MaskColor;

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

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            return _MaskColor; // Only render the mask effect in the stencil area
        }
        ENDCG
    }
    }
}
