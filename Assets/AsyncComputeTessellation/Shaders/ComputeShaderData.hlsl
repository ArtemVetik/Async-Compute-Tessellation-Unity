#ifndef COMPUTE_SHADER_DATA
#define COMPUTE_SHADER_DATA

#include "Structs.hlsl"

#ifndef USE_STANDART_TESSELLATION
struct SPrepassVertexOut
{
    float3 PosW;
    uint Lvl;
    float3 NormalW;
    uint Padding0;
    float2 TexC;
    float2 LeafPos;
};
#endif

groupshared float cam_height_local;

RWStructuredBuffer<uint4> SubdBufferIn;
RWStructuredBuffer<uint4> SubdBufferOut;
RWStructuredBuffer<uint4> SubdBufferOutCulled;

#ifndef USE_STANDART_TESSELLATION
RWStructuredBuffer<SPrepassVertexOut> PrepassVertexOut;
RWStructuredBuffer<uint> PrepassIndexOut;
#endif
RWStructuredBuffer<uint> SubdCounter;
RWStructuredBuffer<uint> DrawArgs;

StructuredBuffer<Vertex> MeshDataVertex;
StructuredBuffer<uint> MeshDataIndex;

#ifndef USE_STANDART_TESSELLATION
StructuredBuffer<float2> LeafVertex;
StructuredBuffer<uint> LeafIndex;
#endif

#endif