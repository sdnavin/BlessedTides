Shader "Custom/SwirlEffect"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" { }
        _SwirlCenter("Swirl Center", Vector) = (0.5, 0.5, 0, 0)
        _SwirlSpeed("Swirl Speed", float) = 1.0
        _SwirlStrength("Swirl Strength", float) = 1.0
        _Alpha("Alpha", float) = 1.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
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
                float4 _MainTex_ST;
                float2 _SwirlCenter;
                float _SwirlSpeed;
                float _SwirlStrength;
                float _Alpha;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                float2 Swirl(float2 uv, float2 center, float strength, float speed)
                {
                    float2 dir = uv - center;
                    float dist = length(dir);
                    float angle = speed * dist * 6.28; // 2*PI
                    float c = cos(angle);
                    float s = sin(angle);
                    dir = mul(float2x2(c, -s, s, c), dir);
                    return center + dir * (1.0 + strength * dist);
                }

                half4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.uv;
                    uv = Swirl(uv, _SwirlCenter, _SwirlStrength, _SwirlSpeed);

                    half4 col = tex2D(_MainTex, uv);
                    col.a *= _Alpha; // Set the transparency
                    return col;
                }
                ENDCG
            }
        }
}
