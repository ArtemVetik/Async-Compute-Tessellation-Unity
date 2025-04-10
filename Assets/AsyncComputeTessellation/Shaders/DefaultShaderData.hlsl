#ifndef DEFAULT_SHADER_DATA
#define DEFAULT_SHADER_DATA

#include "Structs.hlsl"

struct VertexIn
{
    float3 PosW : POSITION0;
    uint Lvl : BLENDINDICES;
    float3 NormalW : NORMAL;
    uint Padding0 : COLOR0;
    float2 TexC : TEXCOORD;
    float2 LeafPos : POSITION1;
};

struct VertexOut
{
    float4 PosH : SV_POSITION;
    float3 PosW : POSITION1;
    float3 NormalW : NORMAL;
    float2 TexC : TEXCOORD;
    uint Lvl : TEXCOORD1;
    float2 LeafPos : TEXCOORD2;
};

SamplerState gsamPointWrap;
SamplerState gsamPointClamp;
SamplerState gsamLinearWrap;
SamplerState gsamLinearClamp;
SamplerState gsamAnisotropicWrap;
SamplerState gsamAnisotropicClamp;
SamplerComparisonState gsamShadow;

StructuredBuffer<VertexIn> PrepassVertexOut;
StructuredBuffer<uint> PrepassIndexOut;

Texture2D gDiffuseMap;

#endif