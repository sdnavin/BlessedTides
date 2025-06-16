Shader "Hidden/PortalReveal"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Radius("Reveal Radius", Float) = 0
        _Center("Effect Center", Vector) = (0.5, 0.5, 0, 0)
        _PortalColor("Portal Color", Color) = (0, 0, 1, 1)
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

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
                };

                sampler2D _MainTex;
                float _Radius;
                float4 _Center;
                float4 _PortalColor;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    float dist = distance(i.uv, _Center.xy);

                    // Create portal effect
                    if (dist < _Radius)
                    {
                        float t = 1 - (dist / _Radius);
                        return lerp(col, _PortalColor, t * 0.6);
                    }

                    return col;
                }
                ENDCG
            }
        }
}