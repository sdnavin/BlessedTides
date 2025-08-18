Shader "Custom/InvBilinearQuad"
{
    Properties
    {
        _MainTex("Main Tex", 2D) = "white" {}
    // Corner positions in OBJECT space
    _P00("Bottom-Left P00 (uv 0,0)", Vector) = (-0.5, -0.5, 0, 1)
    _P10("Bottom-Right P10(uv 1,0)", Vector) = (0.5, -0.5, 0, 1)
    _P11("Top-Right P11   (uv 1,1)", Vector) = (0.5,  0.5, 0, 1)
    _P01("Top-Left P01    (uv 0,1)", Vector) = (-0.5,  0.5, 0, 1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off
        ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _P00;
            float4 _P10;
            float4 _P11;
            float4 _P01;

            struct appdata
            {
                float4 vertex : POSITION; // original quad vertex (usually -0.5..0.5)
                float2 uv     : TEXCOORD0; // 0..1
            };

            struct v2f
            {
                float2 originalUV : TEXCOORD0; // for material tiling if you want
                float3 warpedPos  : TEXCOORD1; // object-space warped position (xyz)
                float4 pos        : SV_POSITION;
            };

            // straightforward bilinear interpolation (P(u,v))
            float3 bilinearPos(float u, float v, float3 p00, float3 p10, float3 p11, float3 p01)
            {
                // (1-u)(1-v) * p00 + u(1-v) * p10 + u v * p11 + (1-u) v * p01
                float a = (1.0 - u) * (1.0 - v);
                float b = u * (1.0 - v);
                float c = u * v;
                float d = (1.0 - u) * v;
                return p00 * a + p10 * b + p11 * c + p01 * d;
            }

            v2f vert(appdata v)
            {
                v2f o;
                // pass original uv (with Unity tiling/offset)
                o.originalUV = TRANSFORM_TEX(v.uv, _MainTex);

                // flip uv.y so uv.y=0 is bottom (Unity quad already bottom=0, but keep explicit)
                float u = v.uv.x;
                float vflip = v.uv.y; // keep as-is; P00 is bottom-left, P01 top-left, etc.

                // Corners in object-space (drop .w)
                float3 p00 = _P00.xyz;
                float3 p10 = _P10.xyz;
                float3 p11 = _P11.xyz;
                float3 p01 = _P01.xyz;

                // compute warped object-space position by bilinear interpolation
                float3 posObj = bilinearPos(u, vflip, p00, p10, p11, p01);

                o.warpedPos = posObj;
                o.pos = UnityObjectToClipPos(float4(posObj, 1.0));

                return o;
            }

            // Compute partial derivatives dP/du and dP/dv of the bilinear mapping
            void bilinearPartials(float u, float v, float3 p00, float3 p10, float3 p11, float3 p01,
                                  out float3 dPdu, out float3 dPdv)
            {
                // dP/du = (1-v)*(p10 - p00) + v*(p11 - p01)
                dPdu = (1.0 - v) * (p10 - p00) + v * (p11 - p01);

                // dP/dv = (1-u)*(p01 - p00) + u*(p11 - p10)
                dPdv = (1.0 - u) * (p01 - p00) + u * (p11 - p10);
            }

            // Inverse bilinear: given a position P, find (u,v) s.t. P = B(u,v).
            // We use Newton-Raphson on the 2D system. Iterations = 4 (fast, accurate enough).
            float2 invBilinear(float3 P, float3 p00, float3 p10, float3 p11, float3 p01)
            {
                // initial guess: use an affine approximation (solve for uv by projecting onto
                // a rectangle formed by corner centers). A simple good start is barycentric-like:
                float2 uv = float2(0.5, 0.5);

                // Better initial guess: solve linear interpolation along diagonals
                // project onto diagonal vector to get a crude u and v
                // Compute center
                float3 center = 0.25 * (p00 + p10 + p11 + p01);
                float3 right = 0.5 * (p10 + p11) - 0.5 * (p00 + p01); // approximate X axis
                float3 up = 0.5 * (p01 + p11) - 0.5 * (p00 + p10); // approximate Y axis

                // Avoid 0-length
                float rlen2 = max(dot(right,right), 1e-6);
                float uGuess = saturate(dot(P - center, right) / rlen2 * 0.5 + 0.5);

                float ulen2 = max(dot(up,up), 1e-6);
                float vGuess = saturate(dot(P - center, up) / ulen2 * 0.5 + 0.5);

                uv = float2(uGuess, vGuess);

                // Newton iterations
                for (int i = 0; i < 5; ++i) // 5 iterations for robustness
                {
                    // Evaluate bilinear at current uv
                    float u = uv.x;
                    float v = uv.y;
                    float3 B = bilinearPos(u, v, p00, p10, p11, p01);

                    // residual
                    float3 R = B - P;

                    // if residual small, break
                    float r2 = dot(R, R);
                    if (r2 < 1e-8) break;

                    // Jacobian columns
                    float3 dPdu;
                    float3 dPdv;
                    bilinearPartials(u, v, p00, p10, p11, p01, dPdu, dPdv);

                    // Build 2x2 normal equations J^T J * delta = -J^T * R (safer than inverting J)
                    float a00 = dot(dPdu, dPdu);
                    float a01 = dot(dPdu, dPdv);
                    float a11 = dot(dPdv, dPdv);

                    float b0 = dot(dPdu, R);
                    float b1 = dot(dPdv, R);

                    // Solve [a00 a01; a01 a11] * delta = -[b0; b1]
                    float det = a00 * a11 - a01 * a01;
                    if (abs(det) < 1e-10) break;

                    float invDet = 1.0 / det;
                    float2 delta;
                    delta.x = (-b0 * a11 - (-b1) * a01) * invDet; // careful signs
                    delta.y = (a00 * (-b1) - a01 * (-b0)) * invDet;

                    // update uv, step-size clamped for stability
                    uv += delta;
                    uv = clamp(uv, 0.0, 1.0);
                }

                return uv;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // read corners (object-space)
                float3 p00 = _P00.xyz;
                float3 p10 = _P10.xyz;
                float3 p11 = _P11.xyz;
                float3 p01 = _P01.xyz;

                // compute uv via inverse bilinear from the interpolated warped position
                float2 uvSolved = invBilinear(i.warpedPos, p00, p10, p11, p01);

                // sample texture with solved uv (note: pumpkin article uses uv (0,0) bottom-left)
                // but Unity texture convention is the same here for sampling.
                float2 texUV = uvSolved;
                float4 col = tex2D(_MainTex, texUV);

                return col;
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}
