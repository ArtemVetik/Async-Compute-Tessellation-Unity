using System.Runtime.InteropServices;
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
    };
    
    public struct TessellationParams
    {
        public enum MeshMode
        {
            Terrain, Mesh
        }
        
        public MeshMode Mesh;
        public bool WireframeMode;
        public bool FlatNormals;
        public int CPULodLevel;
        public bool Uniform;
        public float TargetLength;
        public bool UseDisplaceMapping;
        public bool Freeze;

        public ConstantBuffer CB;
        
        public struct ConstantBuffer
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
        }
    }

    internal struct PerFrameData
    {
        public float3 PredictedCamPosition;
        public uint Padding;
        
        public float4 FrustrumPlane1;
        public float4 FrustrumPlane2;
        public float4 FrustrumPlane3;
        public float4 FrustrumPlane4;
        public float4 FrustrumPlane5;
        public float4 FrustrumPlane6;
    };

    internal struct Vertex
    {
        public float4 Position;
        public float4 Normal;
        public float4 TangentU;
        public float4 TexC;
    };
}
