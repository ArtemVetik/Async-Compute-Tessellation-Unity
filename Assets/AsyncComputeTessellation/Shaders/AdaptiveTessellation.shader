Shader "Unlit/AdaptiveTessellation" {
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "DefaultShaderData.hlsl"
            #include "UnityCG.cginc"

            float4 levelColor(uint lvl)
            {
                float4 c = float4(0.5, 0.5, 0.5, 1);
                uint mod = lvl % 4;
                if (mod == 0)
                {
                    c.r += 0.5;
                }
                else if (mod == 1)
                {
                    c.g += 0.5;
                }
                else if (mod == 2)
                {
                    c.b += 0.5;
                }
                return c;
            }

            float gridFactor(float2 vBC, float width)
            {
                float3 bary = float3(vBC.x, vBC.y, 1.0 - vBC.x - vBC.y);
                float3 d = fwidth(bary);
                float3 a3 = smoothstep(d * (width - 0.5), d * (width + 0.5), bary);
                return min(min(a3.x, a3.y), a3.z);
            }

            VertexOut vert (uint vertexID: SV_VertexID) 
            {
                VertexOut output;

                uint idx = PrepassIndexOut[vertexID];
                VertexIn vIn = PrepassVertexOut[idx];

                output.PosH = UnityObjectToClipPos(vIn.PosW);

                output.PosW = vIn.PosW;
                output.NormalW = vIn.NormalW;
                output.TexC = vIn.TexC;
                output.LeafPos = vIn.LeafPos;
                output.Lvl = vIn.Lvl;

                return output;
            }
            
            fixed4 frag (VertexOut i) : SV_Target {
                float3 p = i.PosW;
    
                float4 c = levelColor(i.Lvl);

                float wireframe_factor = gridFactor(i.LeafPos, 0.5);

                return float4(c.xyz * wireframe_factor, 1);

                return fixed4(i.TexC, 0, 1);
            }
            ENDCG
        }
    }
}