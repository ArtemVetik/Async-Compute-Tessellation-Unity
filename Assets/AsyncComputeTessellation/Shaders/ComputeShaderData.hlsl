#ifndef COMPUTE_SHADER_DATA
#define COMPUTE_SHADER_DATA

#include "Structs.hlsl"

struct SPrepassVertexOut
{
    float3 PosW;
    uint Lvl;
    float3 NormalW;
    uint Padding0;
    float2 TexC;
    float2 LeafPos;
};

groupshared float cam_height_local;

RWStructuredBuffer<uint4> SubdBufferIn;
RWStructuredBuffer<uint4> SubdBufferOut;
RWStructuredBuffer<uint4> SubdBufferOutCulled;

RWStructuredBuffer<SPrepassVertexOut> PrepassVertexOut;
RWStructuredBuffer<uint> PrepassIndexOut;
RWStructuredBuffer<uint> SubdCounter;
RWStructuredBuffer<uint> DrawArgs;

StructuredBuffer<Vertex> MeshDataVertex;
StructuredBuffer<uint> MeshDataIndex;

StructuredBuffer<float2> LeafVertex;
StructuredBuffer<uint> LeafIndex;

#endif