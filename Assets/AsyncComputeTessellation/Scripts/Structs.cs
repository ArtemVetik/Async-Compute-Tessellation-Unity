using Unity.Mathematics;

namespace AV.AsyncComputeTessellation
{
    internal struct VertexOut
    {
        public float3 PosW;
        public uint Lvl;
        public float3 NormalW;
        public uint Padding0;
        public float2 TexC;
        public float2 LeafPos;
    };

    internal struct ObjectData
    {
        public float4x4 World;
        public float4x4 TexTransform;

        public static ObjectData CreateDefault()
        {
            return new ObjectData()
            {
                World = float4x4.identity,
                TexTransform = float4x4.identity
            };
        }
    };

    internal struct TessellationData
    {
        public uint SubdivisionLevel;
        public uint ScreenRes;
        public float DisplaceFactor;
        public uint WavesAnimationFlag;
        public float DisplaceLacunarity;
        public float DisplacePosScale;
        public float DisplaceH;
        public float LodFactor;
        public uint IndicesCount;
        public uint TrianglesCount;
        public float2 Padding;

        public static TessellationData CreateDefault()
        {
            return new TessellationData()
            {
                SubdivisionLevel = 5,
                ScreenRes = 1920,
                DisplaceFactor = 10.0f,
                WavesAnimationFlag = 0,
                DisplaceLacunarity = 1.99f,
                DisplacePosScale = 0.02f,
                DisplaceH = 0.96f,
                LodFactor = 0.00008f,
                IndicesCount = 3,
                TrianglesCount = 3,
            };
        }
    };

    internal unsafe struct PerFrameData
    {
        public float4x4 ViewProj;
        public float3 CamPosition;
        public float DeltaTime;
        public float3 PredictedCamPosition;
        public float TotalTime;
        public fixed float FrustrumPlanes[4 * 6]; // float4 x 6
    };

    internal struct Vertex
    {
        public float4 Position;
        public float4 Normal;
        public float4 TangentU;
        public float4 TexC;
    };
}
